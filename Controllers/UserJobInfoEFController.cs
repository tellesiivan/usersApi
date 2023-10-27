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
public class UserJobInfoEFController : ControllerBase
{
    readonly DataContextEF entityFramework;

    public UserJobInfoEFController(IConfiguration configuration)
    {
        entityFramework = new(configuration);
    }

    [HttpGet("Summary/{userId}")]
    public UserSummary GetUserSummary(int userId)
    {
        // User? dbUser =
        //     entityFramework.Users.Where<User>(user => user.UserId == userId).FirstOrDefault()
        //     ?? throw new Exception("No user Found");

        User? user =
            entityFramework.Users.Where(user => user.UserId == userId).FirstOrDefault()
            ?? throw new Exception("Failed to get user");

        Console.WriteLine(user.ToString());

        UserJobInfo? dbUserJobInfo =
            entityFramework.UsersJobInfo
                .Where<UserJobInfo>(jobInfo => jobInfo.UserId == userId)
                .FirstOrDefault() ?? throw new Exception("No user Found under Job Info");

        Console.WriteLine(dbUserJobInfo.ToString());

        UserJobInfoDto jobInfo =
            new() { Department = dbUserJobInfo.Department, JobTitle = dbUserJobInfo.JobTitle };

        UserDto userInfo =
            new()
            {
                Active = user.Active,
                Email = user.Email,
                FirstName = user.FirstName,
                Gender = user.Gender,
                LastName = user.LastName
            };

        UserSummary userSummary =
            new()
            {
                UserId = user.UserId,
                JobInfo = jobInfo,
                UserInfo = userInfo
            };

        return userSummary;
    }
}
