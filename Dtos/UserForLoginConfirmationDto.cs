namespace DotnetApi.Dtos;

public partial class UserForLoginConfirmationDto
{
    byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
}
