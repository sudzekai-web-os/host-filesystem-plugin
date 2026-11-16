namespace Application.Dto
{
    internal sealed record LoginResponse(
        string Token,
        string Username,
        long Id,
        int RoleId,
        int ExpiresIn,
        string TokenType
    );
}
