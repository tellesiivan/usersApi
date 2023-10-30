using System.Data;
using System.Security.Cryptography;
using DotnetApi.Data;
using DotnetApi.Helpers;
using DotnetApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DotnetApi.Controllers;

// You need to be authorized to access one of the following endpoints unless adding [AllowAnonymous] on top of the endpoint(Login/Register)
[Authorize]
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly AuthHelper _authHelper;

    public AuthController(IConfiguration config)
    {
        _dapper = new(config);
        _authHelper = new(config);
    }

    [AllowAnonymous]
    [HttpPost("Register")]
    public IActionResult Register(UserForRegistrationDto userForRegistration)
    {
        string Password = userForRegistration.Password;
        string ConfirmationalPassword = userForRegistration.PasswordConfirm;

        string sqlCheckIfUserExist =
            "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '"
            + userForRegistration.Email
            + "'";

        IEnumerable<string> existingUser = _dapper.LoadData<string>(sqlCheckIfUserExist);

        // Throw an Exception if password and confirmational password do not match
        if (!Password.Equals(ConfirmationalPassword))
        {
            throw new Exception("Passwords do not match");
        }
        // Throw an Exception if there is already user/s with that email
        else if (existingUser.Any())
        {
            return StatusCode(420, "User with this email already exists!");
        }

        byte[] passwordSalt = new byte[128 / 8];

        // random number generator
        using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
        {
            randomNumberGenerator.GetNonZeroBytes(passwordSalt);
        }

        byte[] passwordHash = _authHelper.GetPasswordHash(
            userForRegistration.Password,
            passwordSalt
        );

        // @ --> Means we have a variable
        // @NameOfParamInStoreProcedure = @NameOfActualParamValues,
        string sqlAddAuth =
            @"EXEC TutorialAppSchema.spRegistration_Upsert
                   @Email = @EmailParam,
                   @PasswordHash = @PasswordHashParam,
                   @PasswordSalt = @PasswordSaltParam";

        List<SqlParameter> sqlParameters = new();

        SqlParameter EmailParameter =
            new("@EmailParam", SqlDbType.VarChar) { Value = userForRegistration.Email };
        sqlParameters.Add(EmailParameter);

        SqlParameter passwordSaltParameter =
            new("@PasswordSaltParam", SqlDbType.VarBinary) { Value = passwordSalt };
        sqlParameters.Add(passwordSaltParameter);

        SqlParameter passwordHashParameter =
            new("@PasswordHashParam", SqlDbType.VarBinary) { Value = passwordHash };
        sqlParameters.Add(passwordHashParameter);

        bool isSuccessfulRegistration = _dapper.ExecuteSqlWithParameter(sqlAddAuth, sqlParameters);

        if (!isSuccessfulRegistration)
        {
            throw new Exception("Unable to register user at this time");
        }

        string sqlAddUser =
            @"EXEC TutorialAppSchema.spUser_Upsert
                            @FirstName = '"
            + userForRegistration.FirstName
            + "', @LastName = '"
            + userForRegistration.LastName
            + "', @Email = '"
            + userForRegistration.Email
            + "', @Gender = '"
            + userForRegistration.Gender
            + "', @Active = 1"
            + ", @JobTitle = '"
            + userForRegistration.JobTitle
            + "', @Department = '"
            + userForRegistration.Department
            + "', @Salary = '"
            + userForRegistration.Salary
            + "'";

        if (!_dapper.ExecuteSql(sqlAddUser))
        {
            return StatusCode(423, "Failed to add user");
        }

        // successful registration
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public IActionResult Login(UserForLoginDto userForLogin)
    {
        string sqlForHashAndSalt =
            @"SELECT [PasswordHash],
                     [PasswordSalt]
            FROM TutorialAppSchema.Auth WHERE Email = '"
            + userForLogin.Email
            + "'";

        UserForLoginConfirmationDto userForLoginConfirmation =
            _dapper.LoadSingleData<UserForLoginConfirmationDto>(sqlForHashAndSalt);

        byte[] passwordHash = _authHelper.GetPasswordHash(
            userForLogin.Password,
            userForLoginConfirmation.PasswordSalt
        );

        for (int index = 0; index < passwordHash.Length; index++)
        {
            // cannot compare both password hashes (passwordHash == userForLoginConfirmation.PasswordHash) since they are objects, therefore we need to compare each byte in the array
            if (passwordHash[index] != userForLoginConfirmation.PasswordHash[index])
            {
                return StatusCode(401, "Password was incorrect!!");
            }
        }

        string userIdSql =
            @"
                SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '"
            + userForLogin.Email
            + "'";

        int userId = _dapper.LoadSingleData<int>(userIdSql);

        string JwtToken = _authHelper.CreateToken(userId);

        // return a key value pair as the response
        Dictionary<string, string> response = new() { { "token", JwtToken } };

        return Ok(response);
    }

    [HttpGet("RefreshToken")]
    public string RefreshToken()
    {
        string sqlGetUserId =
            @"
                SELECT UserId FROM TutorialAppSchema.Users WHERE UserId = '"
            + User.FindFirst("userId")?.Value
            + "'";

        int userId = _dapper.LoadSingleData<int>(sqlGetUserId);

        return _authHelper.CreateToken(userId);
    }
}
