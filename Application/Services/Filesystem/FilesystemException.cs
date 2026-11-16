using System.Net;

namespace Application.Services.Filesystem
{
    public sealed class FilesystemException(HttpStatusCode statusCode, string message) : Exception(message)
    {
        public HttpStatusCode StatusCode { get; } = statusCode;
    }
}
