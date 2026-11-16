using Application.Context;
using Application.Dto;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Application.Services.Database
{
    public sealed class DatabaseHistoryService
    {
        private readonly DatabaseApiClient _api;
        private readonly SystemAuthenticationService _authentication;

        internal DatabaseHistoryService(DatabaseApiClient api, SystemAuthenticationService authentication)
        {
            _api = api;
            _authentication = authentication;
        }

        public void EnsureAuthenticated()
            => _authentication.GetTokenAsync(_authentication.ShutdownToken).GetAwaiter().GetResult();

        public void Record(HistoryEntry entry)
            => RecordAsync(entry, _authentication.ShutdownToken).GetAwaiter().GetResult();

        private async Task RecordAsync(HistoryEntry entry, CancellationToken cancellationToken)
        {
            var token = await _authentication.GetTokenAsync(cancellationToken);
            using var response = await PostAsync(entry, token, cancellationToken);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                await ConfirmAsync(response, cancellationToken);
                return;
            }

            var refreshed = await _authentication.GetTokenAsync(cancellationToken, token);
            using var retry = await PostAsync(entry, refreshed, cancellationToken);
            await ConfirmAsync(retry, cancellationToken);
        }

        private async Task<HttpResponseMessage> PostAsync(HistoryEntry entry, string token, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "database/history")
            {
                Content = JsonContent.Create(entry, ApplicationJsonContext.Default.GetTypeInfo<HistoryEntry>())
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _api.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }

        private static async Task ConfirmAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("success", out var success)
                || success.ValueKind != JsonValueKind.True || !root.TryGetProperty("data", out _))
                throw new JsonException("API не подтвердил запись истории.");
        }

        public static bool IsTransportFailure(Exception exception)
            => DatabaseApiClient.IsTransportFailure(exception);
    }
}
