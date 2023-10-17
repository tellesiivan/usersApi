using DotnetApi.Dtos;

namespace DotnetApi.Models;

public class UserSummary
{
    public int UserId;
    public required UserDto UserInfo;
    public required UserJobInfoDto JobInfo;
}
