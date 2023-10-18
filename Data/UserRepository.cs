using DotnetApi.Models;

namespace DotnetApi.Data;

// inherit IUserRepsository onto UserRepository class to be able to call them and implement each member fromt the declared interface
public class UserRepository : IUserRepsository
{
    DataContextEF _entityFramework;

    public UserRepository(IConfiguration config)
    {
        _entityFramework = new DataContextEF(config);
    }

    // Base Methods
    public bool SaveChanges() => _entityFramework.SaveChanges() > 0;

    public void AddEntity<T>(T entityAdd)
    {
        if (entityAdd != null)
        {
            _entityFramework.Add(entityAdd);
        }
    }

    public void RemoveEntity<K>(K removeEntity)
    {
        if (removeEntity != null)
        {
            _entityFramework.Remove(removeEntity);
        }
    }

    // User --> Endpoints
    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = _entityFramework.Users.ToList<User>();
        return users;
    }

    public User GetSingleUser(int userId)
    {
        User? user = _entityFramework.Users.Where(u => u.UserId == userId).FirstOrDefault<User>();

        if (user != null)
        {
            return user;
        }

        throw new Exception("Failed to Get User");
    }

    public UserSalary GetSingleUserSalary(int userId)
    {
        UserSalary? userSalary = _entityFramework.UserSalary
            .Where(u => u.UserId == userId)
            .FirstOrDefault<UserSalary>();

        if (userSalary != null)
        {
            return userSalary;
        }

        throw new Exception("Failed to Get User");
    }

    public UserJobInfo GetSingleUserJobInfo(int userId)
    {
        UserJobInfo? userJob = _entityFramework.UsersJobInfo
            .Where(u => u.UserId == userId)
            .FirstOrDefault<UserJobInfo>();

        if (userJob != null)
        {
            return userJob;
        }

        throw new Exception("Failed to Get User");
    }
}
