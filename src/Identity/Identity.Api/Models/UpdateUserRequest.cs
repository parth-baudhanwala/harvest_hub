namespace Identity.Api.Models;

public sealed class UpdateUserRequest
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
