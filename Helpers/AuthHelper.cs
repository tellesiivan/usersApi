using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

namespace DotnetApi.Helpers;

public class AuthHelper
{
    private readonly IConfiguration _config;

    public AuthHelper(IConfiguration configuration)
    {
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
}
