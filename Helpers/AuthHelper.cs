using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using DotnetApi.Data;
using DotnetApi.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetApi.Helpers;

public class AuthHelper
{
    private readonly IConfiguration _config;
    private readonly DataContextDapper _dapper;

    public AuthHelper(IConfiguration configuration)
    {
        _dapper = new(configuration);
        _config = configuration;
    }

    public byte[] GetPasswordHash(string password, byte[] passwordSalt)
    {
        string passwordSaltPlusString =
            _config.GetSection("AppSettings.PasswordKey").Value
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

        string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;

        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(tokenKeyString ?? ""));

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

    public bool SetPassword(UserForLoginDto userForSetPassword)
    {
        byte[] passwordSalt = new byte[128 / 8];

        // random number generator
        using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
        {
            randomNumberGenerator.GetNonZeroBytes(passwordSalt);
        }

        byte[] passwordHash = this.GetPasswordHash(userForSetPassword.Password, passwordSalt);

        // @ --> Means we have a variable
        // @NameOfParamInStoreProcedure = @NameOfActualParamValues,
        string sqlAddAuth =
            @"EXEC TutorialAppSchema.spRegistration_Upsert
                   @Email = @EmailParam,
                   @PasswordHash = @PasswordHashParam,
                   @PasswordSalt = @PasswordSaltParam";

        // List<SqlParameter> sqlParameters = new();

        // SqlParameter EmailParameter =
        //     new("@EmailParam", SqlDbType.VarChar) { Value = userForSetPassword.Email };
        // sqlParameters.Add(EmailParameter);

        // SqlParameter passwordSaltParameter =
        //     new("@PasswordSaltParam", SqlDbType.VarBinary) { Value = passwordSalt };
        // sqlParameters.Add(passwordSaltParameter);

        // SqlParameter passwordHashParameter =
        //     new("@PasswordHashParam", SqlDbType.VarBinary) { Value = passwordHash };
        // sqlParameters.Add(passwordHashParameter);

        DynamicParameters dynamicParameters = new();
        dynamicParameters.Add("@EmailParam", userForSetPassword.Email, DbType.String);
        dynamicParameters.Add("@PasswordSaltParam", passwordSalt, DbType.Binary);
        dynamicParameters.Add("@PasswordHashParam", passwordHash, DbType.Binary);

        bool isSuccessfulRegistration = _dapper.ExecuteSqlWithParameter(
            sqlAddAuth,
            dynamicParameters
        );

        return isSuccessfulRegistration;
    }
}
