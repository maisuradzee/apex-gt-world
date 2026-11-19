using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ApexGTWorld.RaceServer
{
    /// <summary>
    /// Server-only client for the protected Supabase race control Edge Function.
    /// Never use this class in the Unity player build or ship its token to clients.
    /// </summary>
    public sealed class RaceControlClient : IDisposable
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _http;
        private readonly bool _ownsHttp;
        private readonly Uri _endpoint;
        private readonly string _serverToken;

        public RaceControlClient(Uri endpoint, string serverToken, HttpClient? httpClient = null)
        {
            if (endpoint is null || !endpoint.IsAbsoluteUri || endpoint.Scheme != Uri.UriSchemeHttps)
                throw new ArgumentException("Race control endpoint must be an absolute HTTPS URL.", nameof(endpoint));
            if (string.IsNullOrWhiteSpace(serverToken) || serverToken.Length < 32)
                throw new ArgumentException("Race server token must be at least 32 characters.", nameof(serverToken));

            _endpoint = endpoint;
            _serverToken = serverToken;
            _ownsHttp = httpClient is null;
            _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        }

        public static RaceControlClient FromEnvironment(HttpClient? httpClient = null)
        {
            var url = Environment.GetEnvironmentVariable("APEX_RACE_CONTROL_URL");
            var token = Environment.GetEnvironmentVariable("APEX_RACE_SERVER_TOKEN");

            if (!Uri.TryCreate(url, UriKind.Absolute, out var endpoint))
                throw new InvalidOperationException("APEX_RACE_CONTROL_URL must be configured as an HTTPS URL.");
            return new RaceControlClient(endpoint, token ?? string.Empty, httpClient);
        }

        public Task<RaceControlResponse> CreateSessionAsync(
            string regionId,
            string discipline,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(regionId))
                throw new ArgumentException("Region ID is required.", nameof(regionId));
            if (discipline is not ("circuit" or "time_attack" or "drift" or "endurance" or "motorcycle"))
                throw new ArgumentOutOfRangeException(nameof(discipline), "Unsupported race discipline.");

            return PostAsync(new CreateRaceSessionRequest
            {
                Operation = "create",
                RegionId = regionId,
                Discipline = discipline
            }, cancellationToken);
        }

        public Task<RaceControlResponse> StartSessionAsync(
            Guid sessionId,
            CancellationToken cancellationToken = default)
        {
            if (sessionId == Guid.Empty) throw new ArgumentException("Session ID is required.", nameof(sessionId));
            return PostAsync(new StartRaceSessionRequest
            {
                Operation = "start",
                SessionId = sessionId
            }, cancellationToken);
        }

        public Task<RaceControlResponse> FinalizeSessionAsync(
            Guid sessionId,
            IReadOnlyCollection<RaceResultSubmission> results,
            CancellationToken cancellationToken = default)
        {
            if (sessionId == Guid.Empty) throw new ArgumentException("Session ID is required.", nameof(sessionId));
            if (results is null || results.Count is < 1 or > 64)
                throw new ArgumentOutOfRangeException(nameof(results), "Race results must contain 1–64 players.");

            return PostAsync(new FinalizeRaceSessionRequest
            {
                Operation = "finalize",
                SessionId = sessionId,
                Results = results
            }, cancellationToken);
        }

        private async Task<RaceControlResponse> PostAsync<TRequest>(
            TRequest payload,
            CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
            request.Headers.TryAddWithoutValidation("x-apex-race-server-token", _serverToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                // Never include the server token in exceptions or logs.
                throw new HttpRequestException(
                    $"Race control returned HTTP {(int)response.StatusCode}: {responseJson}",
                    null,
                    response.StatusCode);
            }

            return JsonSerializer.Deserialize<RaceControlResponse>(responseJson, JsonOptions)
                ?? throw new InvalidOperationException("Race control returned an empty response.");
        }

        public void Dispose()
        {
            if (_ownsHttp) _http.Dispose();
        }
    }

    public sealed class CreateRaceSessionRequest
    {
        [JsonPropertyName("operation")] public string Operation { get; init; } = "create";
        [JsonPropertyName("region_id")] public string RegionId { get; init; } = string.Empty;
        [JsonPropertyName("discipline")] public string Discipline { get; init; } = string.Empty;
    }

    public sealed class StartRaceSessionRequest
    {
        [JsonPropertyName("operation")] public string Operation { get; init; } = "start";
        [JsonPropertyName("session_id")] public Guid SessionId { get; init; }
    }

    public sealed class FinalizeRaceSessionRequest
    {
        [JsonPropertyName("operation")] public string Operation { get; init; } = "finalize";
        [JsonPropertyName("session_id")] public Guid SessionId { get; init; }
        [JsonPropertyName("results")] public IReadOnlyCollection<RaceResultSubmission> Results { get; init; } = Array.Empty<RaceResultSubmission>();
    }

    public sealed class RaceResultSubmission
    {
        [JsonPropertyName("user_id")] public Guid UserId { get; init; }
        [JsonPropertyName("placement")] public int Placement { get; init; }
        [JsonPropertyName("finish_time_ms")] public long? FinishTimeMs { get; init; }
        [JsonPropertyName("score")] public int Score { get; init; }
        [JsonPropertyName("reward_credits")] public long RewardCredits { get; init; }
    }

    public sealed class RaceControlResponse
    {
        [JsonPropertyName("ok")] public bool Ok { get; init; }
        [JsonPropertyName("operation")] public string? Operation { get; init; }
        [JsonPropertyName("session")] public JsonElement? Session { get; init; }
        [JsonPropertyName("result")] public JsonElement? Result { get; init; }
        [JsonPropertyName("results")] public JsonElement? Results { get; init; }
    }
}
