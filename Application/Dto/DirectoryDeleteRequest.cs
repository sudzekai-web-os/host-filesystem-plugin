namespace Application.Dto
{
    public sealed record DirectoryDeleteRequest(string? Path, bool Recursive);
}
