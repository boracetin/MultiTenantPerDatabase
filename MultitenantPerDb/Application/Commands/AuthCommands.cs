namespace MultitenantPerDb.Application.Commands;

public record LoginCommand
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
