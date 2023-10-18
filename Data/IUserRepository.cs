using DotnetApi.Models;

namespace DotnetApi.Data;

public interface IUserRepsository
{
    public bool SaveChanges();
    public void AddEntity<T>(T entityAdd);
    public void RemoveEntity<K>(K removeEntity);
    public IEnumerable<User> GetUsers();
    public User GetSingleUser(int userId);
    public UserSalary GetSingleUserSalary(int userId);
    public UserJobInfo GetSingleUserJobInfo(int userId);
}
