namespace Application.Dto
{
    public sealed record HistoryEntry(
        long UserId,
        DateTime DateTime,
        string Action,
        string Summary
    );
}
