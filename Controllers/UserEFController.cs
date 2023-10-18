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
    IUserRepsository _userRepsository;
    IMapper _mapper;

    public UserEFController(IUserRepsository userRepsository)
    {
        _userRepsository = userRepsository;

        _mapper = new Mapper(
            new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserDto, User>();
                cfg.CreateMap<UserSalary, UserSalary>().ReverseMap();
                cfg.CreateMap<UserJobInfo, UserJobInfo>().ReverseMap();
            })
        );
    }

    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers() => _userRepsository.GetUsers();

    [HttpGet("GetSingleUser/{userId}")]
    public User GetSingleUser(int userId) => _userRepsository.GetSingleUser(userId);

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        User? userDb = _userRepsository.GetSingleUser(user.UserId);

        if (userDb != null)
        {
            userDb.Active = user.Active;
            userDb.FirstName = user.FirstName;
            userDb.LastName = user.LastName;
            userDb.Email = user.Email;
            userDb.Gender = user.Gender;
            if (_userRepsository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to Update User");
        }

        throw new Exception("Failed to Get User");
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserDto user)
    {
        User userDb = _mapper.Map<User>(user);

        _userRepsository.AddEntity<User>(userDb);

        if (_userRepsository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Failed to Add User");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? userDb = _userRepsository.GetSingleUser(userId);

        if (userDb != null)
        {
            _userRepsository.RemoveEntity<User>(userDb);
            if (_userRepsository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to Delete User");
        }

        throw new Exception("Failed to Get User");
    }

    [HttpGet("UserSalary/{userId}")]
    public UserSalary GetUserSalaryEF(int userId)
    {
        return _userRepsository.GetSingleUserSalary(userId);
    }

    [HttpPost("UserSalary")]
    public IActionResult PostUserSalaryEf(UserSalary userForInsert)
    {
        _userRepsository.AddEntity<UserSalary>(userForInsert);

        if (_userRepsository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Adding UserSalary failed on save");
    }

    [HttpPut("UserSalary")]
    public IActionResult PutUserSalaryEf(UserSalary userForUpdate)
    {
        UserSalary? userToUpdate = _userRepsository.GetSingleUserSalary(userForUpdate.UserId);

        if (userToUpdate != null)
        {
            _mapper.Map(userForUpdate, userToUpdate);
            if (_userRepsository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Updating UserSalary failed on save");
        }
        throw new Exception("Failed to find UserSalary to Update");
    }

    [HttpDelete("UserSalary/{userId}")]
    public IActionResult DeleteUserSalaryEf(int userId)
    {
        UserSalary? userToDelete = _userRepsository.GetSingleUserSalary(userId);

        if (userToDelete != null)
        {
            _userRepsository.RemoveEntity<UserSalary>(userToDelete);

            if (_userRepsository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Deleting UserSalary failed on save");
        }
        throw new Exception("Failed to find UserSalary to delete");
    }

    [HttpGet("UserJobInfo/{userId}")]
    public UserJobInfo GetUserJobInfoEF(int userId)
    {
        return _userRepsository.GetSingleUserJobInfo(userId);
    }

    [HttpPost("UserJobInfo")]
    public IActionResult PostUserJobInfoEf(UserJobInfo userForInsert)
    {
        _userRepsository.AddEntity<UserJobInfo>(userForInsert);
        if (_userRepsository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Adding UserJobInfo failed on save");
    }

    [HttpPut("UserJobInfo")]
    public IActionResult PutUserJobInfoEf(UserJobInfo userForUpdate)
    {
        UserJobInfo? userToUpdate = _userRepsository.GetSingleUserJobInfo(userForUpdate.UserId);

        if (userToUpdate != null)
        {
            _mapper.Map(userForUpdate, userToUpdate);
            if (_userRepsository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Updating UserJobInfo failed on save");
        }
        throw new Exception("Failed to find UserJobInfo to Update");
    }

    [HttpDelete("UserJobInfo/{userId}")]
    public IActionResult DeleteUserJobInfoEf(int userId)
    {
        UserJobInfo? userToDelete = _userRepsository.GetSingleUserJobInfo(userId);

        if (userToDelete != null)
        {
            _userRepsository.RemoveEntity<UserJobInfo>(userToDelete);
            if (_userRepsository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Deleting UserJobInfo failed on save");
        }
        throw new Exception("Failed to find UserJobInfo to delete");
    }
}
