using DotnetApi.Dtos;

namespace DotnetApi.Models;

public class UserSummary
{
    public int UserId { get; set; }
    public required UserDto UserInfo { get; set; }
    public required UserJobInfoDto JobInfo { get; set; }
}
