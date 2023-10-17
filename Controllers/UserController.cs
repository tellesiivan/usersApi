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
public class UserController : ControllerBase
{
    readonly DataContextDapper _dapper;

    public UserController(IConfiguration configuration)
    {
        _dapper = new(configuration);
        // By running a request for this controller, the controller was created, therefore the contructor gave us access to configuration.
        Console.WriteLine(configuration.GetConnectionString("DefaultConnection"));
    }

    [HttpGet("GetUsers")]
    // public IEnumerable<User> GetUsers()
    public IEnumerable<User> GetUsers()
    {
        string SQLquery =
            @"
        SELECT [UserId],
        [FirstName],
        [LastName],
        [Email],
        [Gender],
        [Active] FROM TutorialAppSchema.Users
        ";

        IEnumerable<User> users = _dapper.LoadData<User>(SQLquery);

        return users;
    }

    [HttpGet("GetUser/{userId}")]
    public User GetUser(int userId)
    {
        string SQLquery =
            @"
        SELECT [UserId],
        [FirstName],
        [LastName],
        [Email],
        [Gender],
        [Active] FROM TutorialAppSchema.Users
        WHERE UserId =
        ";

        StringBuilder sb = new(SQLquery + userId.ToString());
        User user = _dapper.LoadSingleData<User>(sb.ToString());

        return user;
    }

    [HttpPut("EditUser")]
    // IActionResult when we are not returning actual data but we want to notify the dev if it was a successful request
    public IActionResult EditUser(User user)
    {
        int IsUserActive = user.Active ? 1 : 0;

        string SQLquery =
            @"
        UPDATE TutorialAppSchema.Users
         SET [FirstName] = '"
            + user.FirstName
            + "', [LastName] = '"
            + user.LastName
            + "', [Email] = '"
            + user.Email
            + "', [Gender] = '"
            + user.Gender
            + "', [Active] = '"
            + IsUserActive
            + "' WHERE UserId = "
            + user.UserId;

        if (_dapper.ExecuteSql(SQLquery))
        {
            // OK --> Comes from ControllerBase: 200 response
            return Ok();
        }

        throw new Exception("Failed to update user");
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserDto user)
    {
        int IsUserActive = user.Active ? 1 : 0;

        string SQLquery =
            @"INSERT INTO TutorialAppSchema.Users(
                [FirstName],
                [LastName],
                [Email],
                [Gender],
                [Active]
            ) VALUES ("
            + "'"
            + user.FirstName
            + "', '"
            + user.LastName
            + "', '"
            + user.Email
            + "', '"
            + user.Gender
            + "', '"
            + IsUserActive
            + "')";

        Console.WriteLine(SQLquery);

        if (_dapper.ExecuteSql(SQLquery))
        {
            return Ok();
        }

        throw new Exception("Failed to Add user");
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
