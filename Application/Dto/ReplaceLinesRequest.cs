namespace Application.Dto
{
    public sealed record ReplaceLinesRequest(string? Path, Dictionary<int, string>? Lines);
}
