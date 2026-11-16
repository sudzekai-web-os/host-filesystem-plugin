using Application.Dto;
using System.Net;
using System.Text;

namespace Application.Services.Filesystem
{
    internal sealed class FileService(FilesystemPathResolver paths)
    {
        private const int _transferLimit = 8 * 1024 * 1024;

        public string ExportFile(string path)
        {
            var file = GetFile(path);
            using var stream = file.OpenRead();
            var size = stream.Length;
            if (size > _transferLimit)
                throw new FilesystemException(HttpStatusCode.RequestEntityTooLarge, "Файл превышает 8 МиБ.");

            var bytes = new byte[(int)size];
            stream.ReadExactly(bytes);
            return Convert.ToBase64String(bytes);
        }

        public FileDetails ImportFile(FileTransferRequest request)
        {
            if (request.Content == null)
                throw new FilesystemException(HttpStatusCode.BadRequest, "Укажите содержимое файла в Base64.");

            byte[] bytes;
            try { bytes = Convert.FromBase64String(request.Content); }
            catch (FormatException)
            {
                throw new FilesystemException(HttpStatusCode.BadRequest, "Некорректное содержимое Base64.");
            }

            if (bytes.Length > _transferLimit)
                throw new FilesystemException(HttpStatusCode.RequestEntityTooLarge, "Файл превышает 8 МиБ.");

            var file = paths.ResolveNew(request.Path);
            var temporary = Path.Combine(Path.GetDirectoryName(file)!, ".linctrl-" + Guid.NewGuid().ToString("N"));
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    stream.Write(bytes);
                File.Move(temporary, file);
            }
            finally
            {
                File.Delete(temporary);
            }

            return ToFileDetails(paths, new FileInfo(file));
        }

        public FileDetails GetFileDetails(string? path)
            => ToFileDetails(paths, GetFile(path));

        public object GetFileSecurity(string? path)
            => FilesystemSecurity.Read(GetFile(path));

        public FileDetails CreateFile(TextRequest request)
        {
            var path = paths.ResolveNew(request.Path);
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(request.Content);

            return ToFileDetails(paths, new FileInfo(path));
        }

        public FileDetails CopyFile(TransferRequest request)
        {
            var source = GetFile(request.Source);
            var destination = paths.ResolveNew(request.Destination);
            return ToFileDetails(paths, source.CopyTo(destination));
        }

        public FileDetails MoveFile(TransferRequest request)
        {
            var source = GetFile(request.Source, mutation: true);
            var destination = paths.ResolveNew(request.Destination);
            source.MoveTo(destination);
            return ToFileDetails(paths, source);
        }

        public void DeleteFile(PathRequest request)
            => GetFile(request.Path, mutation: true).Delete();

        public string ReadText(string? path)
            => File.ReadAllText(GetFile(path).FullName);

        public void WriteText(TextRequest request)
        {
            if (request.Content == null)
                throw new FilesystemException(HttpStatusCode.BadRequest, "Укажите содержимое файла.");

            File.WriteAllText(GetFile(request.Path, mutation: true).FullName, request.Content);
        }

        public void ReplaceLines(ReplaceLinesRequest request)
        {
            if (request.Lines == null)
                throw new FilesystemException(HttpStatusCode.BadRequest, "Укажите словарь lines.");

            var file = GetFile(request.Path, mutation: true);
            if (request.Lines.Count == 0)
                return;

            string content;
            Encoding encoding;
            using (var reader = new StreamReader(file.FullName, new UTF8Encoding(false, true), detectEncodingFromByteOrderMarks: true))
            {
                content = reader.ReadToEnd();
                encoding = reader.CurrentEncoding;
            }

            var updated = TextLineReplacer.Replace(content, request.Lines);
            File.WriteAllText(file.FullName, updated, encoding);
        }

        private FileInfo GetFile(string? input, bool mutation = false)
        {
            var path = paths.Resolve(input, mutation);
            if (!FilesystemPathResolver.TryGetAttributes(path, out var attributes) || (attributes & FileAttributes.Directory) != 0)
                throw new FilesystemException(HttpStatusCode.NotFound, "Файл не найден.");
            return new FileInfo(path);
        }

        internal static FileDetails ToFileDetails(FilesystemPathResolver paths, FileInfo file)
            => new(file.Name, paths.ToVirtualPath(file.FullName), file.Extension, file.Length, file.Attributes.ToString(), file.IsReadOnly,
                file.CreationTimeUtc, file.LastAccessTimeUtc, file.LastWriteTimeUtc);
    }
}
