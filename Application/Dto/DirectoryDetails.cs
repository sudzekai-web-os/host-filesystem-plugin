namespace Application.Dto
{
    public sealed record DirectoryDetails(string Name, string FullName, long Length,
        long FilesCount, long DirectoriesCount, string Attributes, DateTime CreationTimeUtc, DateTime LastAccessTimeUtc, DateTime LastWriteTimeUtc);
}
