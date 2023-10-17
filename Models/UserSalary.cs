namespace DotnetApi.Models;

public partial class UserSalary
{
    public decimal Salary { get; set; }
    public Decimal AvgSalary { get; set; }
}

public static class UserSalaryUtils
{
    public static string ToCurrencyString(this UserSalary userSalary) => $"${userSalary.Salary}";
}
