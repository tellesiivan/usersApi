using System.Text;
using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

// tells c-sharpe that this is a controller and needs to be mapped into our endpoints
[ApiController]
// Tells us where we want to look at this controller
// Would be the same as writting 'User' --> Name before `Controller` of the class
// public class UserController --> User
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{
    readonly DataContextDapper _dapper;

    public UserCompleteController(IConfiguration configuration)
    {
        _dapper = new(configuration);
        // By running a request for this controller, the controller was created, therefore the contructor gave us access to configuration.
        Console.WriteLine(configuration.GetConnectionString("DefaultConnection"));
    }

    [HttpGet("GetUsers")]
    public IEnumerable<UserComplete> GetUsers(int? userId, bool? isActive)
    {
        string SQLquery = @"EXEC TutorialAppSchema.spUsers_Get ";
        string parameters = "";

        // if we pass in a userId, then we filter by the @UserId
        if (userId != null)
        {
            parameters += ", @UserId= " + userId.ToString();
        }
        // if we pass in isActive, then we filter by the @Active=
        if (isActive != null)
        {
            parameters += ", @Active= " + isActive;
        }

        SQLquery += parameters[1..];

        IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(SQLquery);
        return users;
    }

    [HttpPut("UpsertUser")]
    // IActionResult when we are not returning actual data but we want to notify the dev if it was a successful request
    public IActionResult UpsertUser(UserComplete user)
    {
        string SQLquery =
            @"EXEC TutorialAppSchema.spUser_UpSert
         @FirstName = '"
            + user.FirstName
            + "', @LastName = '"
            + user.LastName
            + "', @Email = '"
            + user.Email
            + "', @Gender = '"
            + user.Gender
            + "', @JobTitle = '"
            + user.JobTitle
            + "', @Department = '"
            + user.Department
            + "', @Salary = '"
            + user.Salary
            + "', @Active = '"
            + user.Active
            + "', @UserId = "
            + user.UserId;

        if (_dapper.ExecuteSql(SQLquery))
        {
            // OK --> Comes from ControllerBase: 200 response
            return Ok();
        }

        return StatusCode(412, "Failed to Update OR Add user");
    }

    // DELETE
    [HttpDelete("Delete/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string SQLquery =
            @"
        DELETE FROM TutorialAppSchema.Users WHERE UserId = " + userId.ToString();

        if (_dapper.ExecuteSql(SQLquery))
        {
            return Ok();
        }

        throw new Exception("Unable to delete user");
    }
}
