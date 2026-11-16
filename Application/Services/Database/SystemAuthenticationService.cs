using Application.Context;
using Application.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Application.Services.Database
{
    internal sealed class SystemAuthenticationService : IAsyncDisposable
    {
        private readonly HttpClient _client;
        private readonly LoginRequest _loginRequest;
        private readonly ILogger<SystemAuthenticationService> _logger;
        private readonly SemaphoreSlim _tokenLock = new(1);
        private readonly CancellationTokenSource _shutdown = new();
        private readonly Task _initialLogin;
        private string? _token;
        private DateTimeOffset _refreshAfter;

        public SystemAuthenticationService(DatabaseApiClient api, IConfiguration configuration, ILogger<SystemAuthenticationService> logger)
        {
            _logger = logger;

            _loginRequest = new(
                Required(configuration, "Username"),
                Required(configuration, "Password")
            );

            _client = api.Client;
            _initialLogin = WarmUpAsync(_shutdown.Token);
        }

        public CancellationToken ShutdownToken => _shutdown.Token;

        private async Task WarmUpAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    try
                    {
                        await GetTokenAsync(cancellationToken);
                        return;
                    }
                    catch (Exception ex) when (DatabaseApiClient.IsTransportFailure(ex) && !cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogDebug("Database history login will be retried ({errorType})", ex.GetType().Name);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
            {
            }
        }

        public async Task<string> GetTokenAsync(CancellationToken cancellationToken, string? rejectedToken = null)
        {
            await _tokenLock.WaitAsync(cancellationToken);
            try
            {
                if (_token != null && _token != rejectedToken && DateTimeOffset.UtcNow < _refreshAfter)
                    return _token;

                _token = null;
                var issuedAfter = DateTimeOffset.UtcNow;
                using var body = JsonContent.Create(_loginRequest, ApplicationJsonContext.Default.GetTypeInfo<LoginRequest>());
                using var response = await _client.PostAsync("auth/login", body, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                var root = document.RootElement;
                var token = root.ValueKind == JsonValueKind.Object
                    && root.TryGetProperty("success", out var success) && success.ValueKind == JsonValueKind.True
                    && root.TryGetProperty("data", out var data)
                    ? data.Deserialize(ApplicationJsonContext.Default.GetTypeInfo<LoginResponse>())
                    : null;

                if (token is null
                    || token.ExpiresIn <= 0
                    || string.IsNullOrWhiteSpace(token.Token)
                    || !string.Equals(token.TokenType, "Bearer", StringComparison.OrdinalIgnoreCase))
                    throw new JsonException("Invalid token response from LinCtrl");

                var refreshIn = token.ExpiresIn - Math.Min(30, token.ExpiresIn / 2);
                _refreshAfter = issuedAfter.AddSeconds(refreshIn);
                return _token = token.Token;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _shutdown.CancelAsync();
            try
            {
                await _initialLogin;
            }
            finally
            {
                _tokenLock.Dispose();
                _shutdown.Dispose();
            }
        }

        private static string Required(IConfiguration configuration, string key)
            => configuration[$"LinCtrlDatabase:{key}"] is { Length: > 0 } value
                ? value
                : throw new InvalidOperationException($"Missing configuration: LinCtrlDatabase:{key}");
    }
}
