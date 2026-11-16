using System.Text.Json;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace Application.Services.Database
{
    internal sealed class DatabaseApiClient : IDisposable
    {
        public HttpClient Client { get; }

        public DatabaseApiClient(IConfiguration configuration)
        {
            if (!int.TryParse(configuration["LinCtrl:Port"], out var port) || port is < 1 or > 65535)
                throw new InvalidOperationException("Для связи с API необходим корректный LinCtrl:Port.");

            var listenAddress = configuration["LinCtrl:ListenAddress"];
            var address = IPAddress.TryParse(listenAddress, out var ip) && !ip.Equals(IPAddress.Any) && !ip.Equals(IPAddress.IPv6Any)
                ? ip.ToString() : IPAddress.Loopback.ToString();

            Client = new(new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false })
            {
                BaseAddress = new UriBuilder(Uri.UriSchemeHttp, address, port).Uri,
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        public static bool IsTransportFailure(Exception exception)
            => exception is HttpRequestException or OperationCanceledException or JsonException or IOException;

        public void Dispose() => Client.Dispose();
    }
}
