using Application.Dto;
using System.Net;

namespace Application.Services.Filesystem
{
    internal sealed class DirectoryService(FilesystemPathResolver paths)
    {
        public DirectoryEntry[] GetDirectories(string? path)
            => [.. GetDirectory(path).EnumerateDirectories().Where(FilesystemPathResolver.IsRegularEntry)
                .OrderBy(entry => entry.Name, StringComparer.Ordinal).Select(ToDirectoryEntry)];

        public FileDetails[] GetFiles(string? path)
            => [.. GetDirectory(path).EnumerateFiles().Where(FilesystemPathResolver.IsRegularEntry)
                .OrderBy(entry => entry.Name, StringComparer.Ordinal).Select(file => FileService.ToFileDetails(paths, file))];

        public DirectoryDetails GetDirectoryDetails(string? path)
        {
            var directory = GetDirectory(path);
            long length = 0, files = 0, directories = 0;
            foreach (var entry in EnumerateTree(directory, skipSpecialEntries: true))
            {
                if (entry is FileInfo file)
                {
                    length += file.Length;
                    files++;
                }
                else
                    directories++;
            }

            return new(directory.Name, paths.ToVirtualPath(directory.FullName), length, files, directories, directory.Attributes.ToString(),
                directory.CreationTimeUtc, directory.LastAccessTimeUtc, directory.LastWriteTimeUtc);
        }

        public object GetDirectorySecurity(string? path)
            => FilesystemSecurity.Read(GetDirectory(path));

        public DirectoryEntry CreateDirectory(PathRequest request)
        {
            var path = paths.ResolveNew(request.Path);
            return ToDirectoryEntry(Directory.CreateDirectory(path));
        }

        public void DeleteDirectory(DirectoryDeleteRequest request)
        {
            var directory = GetDirectory(request.Path, mutation: true);
            if (request.Recursive)
                ValidateTree(directory);
            else if (directory.EnumerateFileSystemInfos().Any())
                throw new FilesystemException(HttpStatusCode.Conflict, "Директория не пуста. Укажите recursive: true для рекурсивного удаления.");

            directory.Delete(request.Recursive);
        }

        public DirectoryEntry CopyDirectory(TransferRequest request)
        {
            var source = GetDirectory(request.Source);
            var destination = GetDirectoryDestination(source, request.Destination);
            var staging = Path.Combine(Path.GetDirectoryName(destination)!, ".linctrl-" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(staging);
                foreach (var entry in EnumerateTree(source, skipSpecialEntries: false))
                {
                    var target = Path.Combine(staging, Path.GetRelativePath(source.FullName, entry.FullName));
                    if (entry is FileInfo file)
                        file.CopyTo(target);
                    else
                        Directory.CreateDirectory(target);
                }
                Directory.SetLastWriteTimeUtc(staging, source.LastWriteTimeUtc);
                Directory.Move(staging, destination);
            }
            finally
            {
                if (Directory.Exists(staging))
                    Directory.Delete(staging, recursive: true);
            }

            return ToDirectoryEntry(new DirectoryInfo(destination));
        }

        public DirectoryEntry MoveDirectory(TransferRequest request)
        {
            var source = GetDirectory(request.Source, mutation: true);
            var destination = GetDirectoryDestination(source, request.Destination);
            ValidateTree(source);
            Directory.Move(source.FullName, destination);
            return ToDirectoryEntry(new DirectoryInfo(destination));
        }

        private DirectoryInfo GetDirectory(string? input, bool mutation = false)
        {
            var path = paths.Resolve(input, mutation);
            if (!FilesystemPathResolver.TryGetAttributes(path, out var attributes) || (attributes & FileAttributes.Directory) == 0)
                throw new FilesystemException(HttpStatusCode.NotFound, "Директория не найдена.");
            return new DirectoryInfo(path);
        }

        private string GetDirectoryDestination(DirectoryInfo source, string? input)
        {
            var destination = paths.ResolveNew(input);
            if (FilesystemPathResolver.Contains(source.FullName, destination))
                throw new FilesystemException(HttpStatusCode.Conflict, "Директорию нельзя перенести или скопировать внутрь самой себя.");
            return destination;
        }

        private static void ValidateTree(DirectoryInfo directory)
        {
            foreach (var entry in EnumerateTree(directory, skipSpecialEntries: false))
            {
                // Проверка выполняется до операции с деревом.
            }
        }

        private static IEnumerable<FileSystemInfo> EnumerateTree(DirectoryInfo directory, bool skipSpecialEntries)
        {
            var pending = new Stack<DirectoryInfo>();
            pending.Push(directory);
            while (pending.TryPop(out var current))
            {
                foreach (var entry in current.EnumerateFileSystemInfos())
                {
                    if (!FilesystemPathResolver.IsRegularEntry(entry))
                    {
                        if (skipSpecialEntries)
                            continue;
                        throw new FilesystemException(HttpStatusCode.Forbidden, "Директория содержит ссылки или специальные объекты.");
                    }

                    yield return entry;
                    if (entry is DirectoryInfo child)
                        pending.Push(child);
                }
            }
        }

        private DirectoryEntry ToDirectoryEntry(DirectoryInfo directory)
            => new(directory.Name, paths.ToVirtualPath(directory.FullName), directory.LastWriteTimeUtc);
    }
}
