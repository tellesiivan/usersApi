using System.Data;
using Dapper;
using DotnetApi.Data;
using DotnetApi.Helpers;
using DotnetApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

// tells c-sharpe that this is a controller and needs to be mapped into our endpoints
[Authorize]
[ApiController]
// Tells us where we want to look at this controller
// Would be the same as writting 'User' --> Name before `Controller` of the class
// public class UserController --> User
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{
    readonly DataContextDapper _dapper;
    private readonly ReusableSql _reusableSQL;

    public UserCompleteController(IConfiguration configuration)
    {
        _dapper = new(configuration);
        _reusableSQL = new(configuration);
        // By running a request for this controller, the controller was created, therefore the contructor gave us access to configuration.
        Console.WriteLine(configuration.GetConnectionString("DefaultConnection"));
    }

    [HttpGet("GetUsers")]
    public IEnumerable<UserComplete> GetUsers(int? userId, bool? isActive)
    {
        string SQLquery = @"EXEC TutorialAppSchema.spUsers_Get ";
        DynamicParameters dynamicParameters = new();

        string stringParameters = "";

        // if we pass in a userId, then we filter by the @UserId
        if (userId is not null)
        {
            stringParameters += ", @UserId=@UserIdParam";
            dynamicParameters.Add("@UserIdParam", userId, DbType.Int32);
        }
        // if we pass in isActive, then we filter by the @Active=
        if (isActive is not null)
        {
            stringParameters += ", @Active=@ActiveParam";
            dynamicParameters.Add("@ActiveParam", isActive, DbType.Boolean);
        }

        if (stringParameters.Any())
        {
            SQLquery += stringParameters[1..];
        }

        IEnumerable<UserComplete> users = _dapper.LoadDataWithParameters<UserComplete>(
            SQLquery,
            dynamicParameters
        );

        return users;
    }

    [HttpPut("UpsertUser")]
    public IActionResult UpsertUser(UserComplete user)
    {
        if (_reusableSQL.UpsertUser(user))
        {
            return Ok();
        }

        throw new Exception("Failed to Update User");
    }

    // DELETE
    [HttpDelete("Delete/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string SQLquery = @"EXEC TutorialAppSchema.spUser_Delete @UserId=@UserIdParam";

        DynamicParameters dynamicParameters = new();
        dynamicParameters.Add("@UserIdParam", userId, DbType.Int32);

        if (_dapper.ExecuteSqlWithParameter(SQLquery, dynamicParameters))
        {
            return Ok(
                new Dictionary<string, string>()
                {
                    { "successMessage", "You have succesfully deleted the user" }
                }
            );
        }

        throw new Exception("Unable to delete user");
    }
}
