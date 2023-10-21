using System.Data;
using System.Security.Cryptography;
using System.Text;
using DotnetApi.Data;
using DotnetApi.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DotnetApi.Controllers;

public class AuthController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration config)
    {
        _dapper = new(config);
        _configuration = config;
    }

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
            throw new Exception("User with this email already exists!");
        }

        byte[] passwordSalt = new byte[128 / 8];

        // random number generator
        using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
        {
            randomNumberGenerator.GetNonZeroBytes(passwordSalt);
        }

        byte[] passwordHash = GetPasswordHash(userForRegistration.Password, passwordSalt);

        // @ --> Means we have a variable
        string sqlAddAuth =
            @"
                        INSERT INTO TutorialAppSchema.Auth  ([Email],
                        [PasswordHash],
                        [PasswordSalt]) VALUES ('"
            + userForRegistration.Email
            + "', @PasswordHash, @PasswordSalt)";

        List<SqlParameter> sqlParameters = new();
        SqlParameter passwordSaltParameter =
            new("@PasswordSalt", SqlDbType.VarBinary) { Value = passwordSalt };
        SqlParameter passwordHashParameter =
            new("@PasswordHash", SqlDbType.VarBinary) { Value = passwordHash };

        sqlParameters.Add(passwordSaltParameter);
        sqlParameters.Add(passwordHashParameter);

        bool isSuccessfulRegistration = _dapper.ExecuteSqlWithParameter(sqlAddAuth, sqlParameters);

        if (!isSuccessfulRegistration)
        {
            throw new Exception("Unable to register user at this time");
        }

        string SqlAddUser =
            @"INSERT INTO TutorialAppSchema.Users(
                [FirstName],
                [LastName],
                [Email],
                [Gender],
                [Active]
            ) VALUES ("
            + "'"
            + userForRegistration.FirstName
            + "', '"
            + userForRegistration.LastName
            + "', '"
            + userForRegistration.Email
            + "', '"
            + userForRegistration.Gender
            + "', 1)";

        if (!_dapper.ExecuteSql(SqlAddUser))
        {
            return StatusCode(423, "Failed to add user");
        }

        // successful registration
        return Ok();
    }

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

        byte[] passwordHash = GetPasswordHash(
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

        return Ok();
    }

    private byte[] GetPasswordHash(string password, byte[] passwordSalt)
    {
        string passwordSaltPlusString =
            _configuration.GetSection("AppSettings.PasswordKey").Value
            + Convert.ToBase64String(passwordSalt);

        byte[] passwordHash = KeyDerivation.Pbkdf2(
            password: password,
            salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
            // SCHEMA OF HOW TO RANDOMIZE
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8
        );

        return passwordHash;
    }
}
