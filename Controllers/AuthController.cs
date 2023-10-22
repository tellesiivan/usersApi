using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetApi.Data;
using DotnetApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetApi.Controllers;

// You need to be authorized to access one of the following endpoints unless adding [AllowAnonymous] on top of the endpoint(Login/Register)
[Authorize]
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration config)
    {
        _dapper = new(config);
        _configuration = config;
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

        string userIdSql =
            @"
                SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '"
            + userForLogin.Email
            + "'";

        int userId = _dapper.LoadSingleData<int>(userIdSql);

        string JwtToken = CreateToken(userId);

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

        return CreateToken(userId);
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

    public string CreateToken(int userId)
    {
        // claim in a token --> piece of info in a token
        Claim[] claims = new Claim[] { new("userId", userId.ToString()) };

        // todo: find out why the test sample key returns an empty string
        string tokenKeyString =
            _configuration.GetSection("AppSettings.TokenKey").Value
            ?? "38128ewqrdbhsw=1293210348-2903hsjiadak";

        SymmetricSecurityKey securityKey =
            new(Encoding.UTF8.GetBytes("38128ewqrdbhsw=1293210348-2903hsjiadak"));

        SigningCredentials signingCredentials =
            new(securityKey, SecurityAlgorithms.HmacSha512Signature);

        SecurityTokenDescriptor securityTokenDescriptor =
            new()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = signingCredentials,
                Expires = DateTime.Now.AddDays(1)
            };

        JwtSecurityTokenHandler tokenHandler = new();

        // Creates a Json Web Token (JWT).
        SecurityToken securityToken = tokenHandler.CreateToken(securityTokenDescriptor);

        // converts the JWT token into a string, so it can easily be portable
        return tokenHandler.WriteToken(securityToken);
    }
}
