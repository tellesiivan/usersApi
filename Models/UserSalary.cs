namespace DotnetApi.Models;

public partial class UserSalary
{
    public decimal Salary { get; set; }
    public int UserId { get; set; }
}

public static class UserSalaryUtils
{
    public static string ToCurrencyString(this UserSalary userSalary) => $"${userSalary.Salary}";
}
