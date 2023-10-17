using System.Text;
using AutoMapper;
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
public class UserEFController : ControllerBase
{
    readonly DataContextEF entityFramework;
    IMapper _mapper;

    public UserEFController(IConfiguration configuration)
    {
        entityFramework = new(configuration);
        _mapper = new Mapper(
            new MapperConfiguration(config =>
            {
                config.CreateMap<UserDto, User>();
            })
        );
    }

    [HttpGet("GetUsers")]
    // public IEnumerable<User> GetUsers()
    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = entityFramework.Users.ToList<User>();
        return users;
    }

    [HttpGet("GetUser/{userId}")]
    public User GetUser(int userId)
    {
        User? user =
            entityFramework.Users.Where(user => user.UserId == userId).FirstOrDefault()
            ?? throw new Exception("Failed to get user");
        return user;
    }

    [HttpPut("EditUser")]
    // IActionResult when we are not returning actual data but we want to notify the dev if it was a successful request
    public IActionResult EditUser(User editedUser)
    {
        User? matchedDbUser =
            entityFramework.Users.Where(user => user.UserId == editedUser.UserId).FirstOrDefault()
            ?? throw new Exception("Failed to edit user");

        matchedDbUser.Active = editedUser.Active;
        matchedDbUser.LastName = editedUser.LastName;
        matchedDbUser.FirstName = editedUser.FirstName;
        matchedDbUser.Gender = editedUser.Gender;
        matchedDbUser.Email = editedUser.Email;

        if (entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }

        throw new Exception("Failed to edit user");
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserDto user)
    {
        User userDb = _mapper.Map<User>(user);

        // add the user & then save the changes
        entityFramework.Users.Add(userDb);

        if (entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }

        throw new Exception("Failed to add new user");
    }

    // DELETE
    [HttpDelete("Delete/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? matchedDbUser =
            entityFramework.Users.Where(user => user.UserId == userId).FirstOrDefault()
            ?? throw new Exception("Unable to delete user");

        entityFramework.Users.Remove(matchedDbUser);

        if (entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }

        throw new Exception("Unable to delete user");
    }
}
