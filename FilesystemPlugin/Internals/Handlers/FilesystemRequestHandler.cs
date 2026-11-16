using Application.Dispatchering.Dispatchers;
using Application.Services.Filesystem;
using Application.Services.Database;
using Application.Dto;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using System.Web;
using Sudzekai.LinCtrl.Abi.Managed;
using Sudzekai.LinCtrl.Assembly.Extensions;
using Sudzekai.LinCtrl.Core;
using System.Net;
using System.Text;
using System.Text.Json;

namespace FilesystemPlugin
{
    internal abstract class FilesystemRequestHandler(
        IDispatcher dispatcher, DatabaseHistoryService history, HttpStatusCode successStatus = HttpStatusCode.OK) : IRequestHandler
    {
        protected const int TransferRequestLimit = 12 * 1024 * 1024;

        protected virtual int BodySizeLimit => 1024 * 1024;

        public abstract string Pattern { get; }

        public abstract HttpRequestMethod HttpMethod { get; }

        public abstract HandlerMetadata Metadata { get; }

        public HttpResponseDto Handle(HttpRequestDto request)
        {
            try
            {
                var userId = Authorize(request);
                var entry = HttpMethod == HttpRequestMethod.Get ? null : PrepareHistory(request, userId);
                var result = Execute(dispatcher, request);
                if (entry != null)
                    RecordHistory(entry with { DateTime = DateTime.UtcNow });
                return result == null
                    ? HttpResponseDto.Create(successStatus).As(ResponseContentType.ApplicationJson)
                    : HttpResponseDto.Create(successStatus).AsJson(result);
            }
            catch (FilesystemException ex)
            {
                return Error(ex.StatusCode, ex.Message);
            }
            catch (JsonException)
            {
                return Error(HttpStatusCode.BadRequest, "Некорректное тело JSON.");
            }
            catch (DecoderFallbackException)
            {
                return Error(HttpStatusCode.BadRequest, "Файл содержит некорректный текст.");
            }
            catch (ArgumentException)
            {
                return Error(HttpStatusCode.BadRequest, "Некорректный путь или параметры операции.");
            }
            catch (UnauthorizedAccessException)
            {
                return Error(HttpStatusCode.Forbidden, "Доступ к объекту запрещён операционной системой.");
            }
            catch (IOException ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                return Error(HttpStatusCode.NotFound, "Объект файловой системы не найден.");
            }
            catch (IOException)
            {
                return Error(HttpStatusCode.Conflict, "Операция с файловой системой не выполнена.");
            }
        }

        protected abstract object? Execute(IDispatcher dispatcher, HttpRequestDto request);

        protected static string GetSecuritySchema()
            => new JsonObject
            {
                ["oneOf"] = new JsonArray(GetSchema(typeof(WindowsSecurityInfo)), GetSchema(typeof(LinuxSecurityInfo)))
            }.ToJsonString();

        private static JsonTypeInfo GetTypeInfo(Type type)
            => FilesystemJsonContext.Default.GetTypeInfo(type)
                ?? throw new InvalidOperationException($"Missing JSON metadata for {type}");

        private static JsonNode GetSchema(Type type)
            => GetTypeInfo(type).GetJsonSchemaAsNode(new JsonSchemaExporterOptions { TreatNullObliviousAsNonNullable = true });

        protected static string ReadPath(HttpRequestDto request)
        {
            var values = HttpUtility.ParseQueryString(request.QueryString).GetValues("path");
            if (values is not [var path])
                throw new FilesystemException(HttpStatusCode.BadRequest, "Укажите один параметр path.");
            return path;
        }

        protected static T ReadBody<T>(HttpRequestDto request, JsonTypeInfo<T> typeInfo, int sizeLimit = 1024 * 1024) where T : class
        {
            if (request.ContentType != ResponseContentType.ApplicationJson)
                throw new FilesystemException(HttpStatusCode.UnsupportedMediaType, "Требуется Content-Type application/json.");
            if (request.Body.Length > sizeLimit)
                throw new FilesystemException(HttpStatusCode.RequestEntityTooLarge, "Тело запроса превышает допустимый размер.");

            return JsonSerializer.Deserialize(request.Body, typeInfo)
                ?? throw new FilesystemException(HttpStatusCode.BadRequest, "Требуется объект JSON.");
        }

        private HistoryEntry PrepareHistory(HttpRequestDto request, long userId)
        {
            if (request.ContentType != ResponseContentType.ApplicationJson)
                throw new FilesystemException(HttpStatusCode.UnsupportedMediaType, "Требуется Content-Type application/json.");
            if (request.Body.Length > BodySizeLimit)
                throw new FilesystemException(HttpStatusCode.RequestEntityTooLarge, "Тело запроса превышает допустимый размер.");

            using var json = JsonDocument.Parse(request.Body);
            var root = json.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                throw new FilesystemException(HttpStatusCode.BadRequest, "Требуется объект JSON.");

            var paths = new List<string>();
            foreach (var key in new[] { "path", "source", "destination" })
            {
                if (root.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
                    paths.Add($"{key}: {value.GetString()}");
            }

            var summary = paths.Count == 0 ? Pattern : string.Join(", ", paths);
            if (summary.Length > 4096)
                summary = summary[..4096];

            try
            {
                history.EnsureAuthenticated();
            }
            catch (Exception ex) when (DatabaseHistoryService.IsTransportFailure(ex))
            {
                throw new FilesystemException(HttpStatusCode.ServiceUnavailable, "Системная авторизация для записи истории недоступна.");
            }

            return new(userId, default, $"{HttpMethod.ToString().ToUpperInvariant()} /{Pattern}", summary);
        }

        private void RecordHistory(HistoryEntry entry)
        {
            try
            {
                history.Record(entry);
            }
            catch (Exception ex) when (DatabaseHistoryService.IsTransportFailure(ex))
            {
                throw new FilesystemException(HttpStatusCode.BadGateway,
                    "Изменение выполнено, но запись в историю не подтверждена.");
            }
        }

        private long Authorize(HttpRequestDto request)
        {
            if (!request.Claims.TryGetValue("sub", out var subjects) || subjects.Length != 1 || !long.TryParse(subjects[0], out var id) || id <= 0)
                throw new FilesystemException(HttpStatusCode.Unauthorized, "Требуется действующий токен.");

            var roles = request.Claims.GetValueOrDefault("role") ?? [];
            if (roles.Contains(nameof(ClientRole.Administrator), StringComparer.Ordinal))
                return id;
            if (HttpMethod == HttpRequestMethod.Get && roles.Any(role => role is nameof(ClientRole.Viewer) or nameof(ClientRole.User)))
                return id;

            throw new FilesystemException(HttpStatusCode.Forbidden, "Недостаточно прав.");
        }

        private static HttpResponseDto Error(HttpStatusCode statusCode, string message)
            => HttpResponseDto.CreateError(statusCode, (int)statusCode, message);
    }
}
