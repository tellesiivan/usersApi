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

    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadSingleData<DateTime>("SELECT GETDATE()");
    }

    [HttpGet("GetUsers/{testValue}")]
    // public IEnumerable<User> GetUsers()
    public string[] GetUsers(string testValue)
    {

        return new string[] { "Hello", "Wes", "Test", testValue };
        // return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        // {
        //     Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //     TemperatureC = Random.Shared.Next(-20, 55),
        //     Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        // })
        // .ToArray();

    }
}
