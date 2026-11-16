namespace Application.Dto
{
    public sealed record FileDetails(string Name, string FullName, string Extension, long Length,
        string Attributes, bool IsReadOnly, DateTime CreationTimeUtc, DateTime LastAccessTimeUtc, DateTime LastWriteTimeUtc);
}
