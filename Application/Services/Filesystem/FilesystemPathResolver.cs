using Microsoft.Extensions.Configuration;
using System.Net;

namespace Application.Services.Filesystem
{
    internal sealed class FilesystemPathResolver
    {
        private static readonly StringComparison _comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        private const string _virtualRoot = @"root:\";
        private readonly string _root;

        public FilesystemPathResolver(IConfiguration configuration)
        {
            var root = configuration["Filesystem:Root"];
            if (string.IsNullOrWhiteSpace(root))
                throw new FilesystemException(HttpStatusCode.ServiceUnavailable, "Укажите Filesystem:Root в настройках сервера.");

            _root = Normalize(root);
        }

        public string ResolveNew(string? input)
        {
            var path = Resolve(input, mutation: true);
            if (TryGetAttributes(path, out _))
                throw new FilesystemException(HttpStatusCode.Conflict, "Файл или директория по указанному пути уже существует.");

            if (!Directory.Exists(Path.GetDirectoryName(path)))
                throw new FilesystemException(HttpStatusCode.NotFound, "Родительская директория не найдена.");
            return path;
        }

        public string Resolve(string? input, bool mutation = false)
        {
            if (input == null || !input.StartsWith(_virtualRoot, StringComparison.OrdinalIgnoreCase))
                throw new FilesystemException(HttpStatusCode.BadRequest, @"Укажите путь в формате root:\.");

            var segments = input[_virtualRoot.Length..].Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);
            if (segments.Any(segment => segment is "." or ".."))
                throw new FilesystemException(HttpStatusCode.Forbidden, "Относительные компоненты пути недоступны.");

            var relative = string.Join(Path.DirectorySeparatorChar, segments);
            var path = Normalize(Path.Join(_root, relative));
            if (!Contains(_root, path))
                throw new FilesystemException(HttpStatusCode.Forbidden, "Путь находится вне корневой директории.");

            if (mutation && string.Equals(_root, path, _comparison))
                throw new FilesystemException(HttpStatusCode.Forbidden, "Корневую директорию нельзя изменить.");

            ValidateAncestors(path);
            return path;
        }

        public string ToVirtualPath(string path)
        {
            var relative = Path.GetRelativePath(_root, path);
            return relative == "." ? _virtualRoot : _virtualRoot + relative.Replace(Path.DirectorySeparatorChar, '\\');
        }

        public static bool Contains(string parent, string path)
        {
            if (string.Equals(parent, path, _comparison))
                return true;

            var prefix = Path.EndsInDirectorySeparator(parent) ? parent : parent + Path.DirectorySeparatorChar;
            return path.StartsWith(prefix, _comparison);
        }

        public static bool TryGetAttributes(string path, out FileAttributes attributes)
        {
            try
            {
                attributes = File.GetAttributes(path);
                return true;
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                attributes = default;
                return false;
            }
        }

        public static bool IsRegularEntry(FileSystemInfo entry)
            => (entry.Attributes & FileAttributes.ReparsePoint) == 0
                && (!OperatingSystem.IsLinux() || LinuxFileStatus.Read(entry.FullName).IsRegularEntry);

        private static string Normalize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input) || !Path.IsPathFullyQualified(input))
                throw new FilesystemException(HttpStatusCode.BadRequest, "Укажите абсолютный путь.");

            if (OperatingSystem.IsWindows())
            {
                if (input.Length < 3 || !char.IsAsciiLetter(input[0]) || input[1] != ':' || input.AsSpan(2).Contains(':'))
                    throw new FilesystemException(HttpStatusCode.BadRequest, "Укажите обычный путь на локальном диске Windows.");

                var segments = input[3..].Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);
                foreach (var segment in segments)
                {
                    if (segment.EndsWith(' ') || segment.EndsWith('.') || IsWindowsDeviceName(segment))
                        throw new FilesystemException(HttpStatusCode.BadRequest, "Путь содержит недопустимый компонент Windows.");
                }
            }

            return Path.TrimEndingDirectorySeparator(Path.GetFullPath(input));
        }

        private static bool IsWindowsDeviceName(string segment)
        {
            var name = segment.Split('.', 2)[0].TrimEnd().ToUpperInvariant();
            return name is "CON" or "PRN" or "AUX" or "NUL" or "CONIN$" or "CONOUT$"
                || (name.Length == 4 && name[3] is >= '0' and <= '9' or '¹' or '²' or '³'
                    && (name.StartsWith("COM", StringComparison.Ordinal) || name.StartsWith("LPT", StringComparison.Ordinal)));
        }

        private static void ValidateAncestors(string path)
        {
            for (string? current = path; current != null; current = Path.GetDirectoryName(current))
            {
                if (!TryGetAttributes(current, out var attributes))
                    continue;

                if ((attributes & FileAttributes.ReparsePoint) != 0
                    || (OperatingSystem.IsLinux() && !LinuxFileStatus.Read(current).IsRegularEntry))
                    throw new FilesystemException(HttpStatusCode.Forbidden, "Ссылки и специальные объекты файловой системы недоступны.");
            }
        }
    }
}
