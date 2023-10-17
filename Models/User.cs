namespace DotnetApi.Models;

public partial class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Gender { get; set; } = "";
    public bool Active { get; set; }

    public static int Add(int a, int b) => a + b;
}

// nobody can inherit (sealed)
public sealed record UserModel(
    int UserId,
    string FirstName,
    string LastName,
    string Email,
    string Gender,
    bool Active
);

public static class UserModelUtils
{
    public static UserModel DeleteEmail(this UserModel user) => user with { Email = "" };
}


// user.DeleteEmail();
