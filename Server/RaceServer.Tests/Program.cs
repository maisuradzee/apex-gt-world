using System.Net;
using System.Text;
using System.Text.Json;
using ApexGTWorld.RaceServer;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException("TEST FAILED: " + message);
}

static void ExpectArgument(Action action, string name)
{
    try
    {
        action();
        throw new InvalidOperationException("TEST FAILED: expected ArgumentException for " + name);
    }
    catch (ArgumentException)
    {
        // Expected validation failure.
    }
}

const string testToken = "test-only-race-server-token-not-a-real-secret-123456789";
var endpoint = new Uri("https://example.supabase.co/functions/v1/race-session-control");
var sessionId = Guid.Parse("00000000-0000-4000-8000-000000000111");
var playerId = Guid.Parse("00000000-0000-4000-8000-000000000222");
var requestKey = Guid.Parse("00000000-0000-4000-8000-000000000333");
var handler = new RecordingHandler(sessionId, playerId);
using var http = new HttpClient(handler);
using var client = new RaceControlClient(endpoint, testToken, http);

var created = await client.CreateSessionAsync("metro_city", "circuit", requestKey);
Check(created.Ok, "create response should be ok");
Check(created.Operation == "create", "create response operation");
Check(handler.Requests.Count == 1, "one create request recorded");
Check(handler.Requests[0].Method == HttpMethod.Post, "create uses POST");
Check(handler.Requests[0].Uri == endpoint, "create endpoint URI");
Check(handler.Requests[0].Headers.TryGetValue("x-apex-race-server-token", out var sentToken) && sentToken == testToken,
    "server token header is sent");
using (var body = JsonDocument.Parse(handler.Requests[0].Body))
{
    var root = body.RootElement;
    Check(root.GetProperty("operation").GetString() == "create", "create operation body");
    Check(root.GetProperty("region_id").GetString() == "metro_city", "create region body");
    Check(root.GetProperty("discipline").GetString() == "circuit", "create discipline body");
    Check(root.GetProperty("request_key").GetGuid() == requestKey, "stable request key body");
}

var started = await client.StartSessionAsync(sessionId);
Check(started.Ok && started.Operation == "start", "start operation response");
using (var body = JsonDocument.Parse(handler.Requests[1].Body))
{
    Check(body.RootElement.GetProperty("session_id").GetGuid() == sessionId, "start session id body");
}

var results = new[]
{
    new RaceResultSubmission
    {
        UserId = playerId,
        Placement = 1,
        FinishTimeMs = 123456,
        Score = 900,
        RewardCredits = 250
    }
};
var finalized = await client.FinalizeSessionAsync(sessionId, results);
Check(finalized.Ok && finalized.Operation == "finalize", "finalize operation response");
using (var body = JsonDocument.Parse(handler.Requests[2].Body))
{
    var root = body.RootElement;
    Check(root.GetProperty("operation").GetString() == "finalize", "finalize operation body");
    Check(root.GetProperty("session_id").GetGuid() == sessionId, "finalize session id body");
    var submitted = root.GetProperty("results");
    Check(submitted.GetArrayLength() == 1, "one submitted player result");
    Check(submitted[0].GetProperty("user_id").GetGuid() == playerId, "result player id");
    Check(submitted[0].GetProperty("placement").GetInt32() == 1, "result placement");
    Check(submitted[0].GetProperty("reward_credits").GetInt64() == 250, "server-calculated payout field");
}

ExpectArgument(() => new RaceControlClient(new Uri("http://insecure.example"), testToken), "non-HTTPS endpoint");
ExpectArgument(() => new RaceControlClient(endpoint, "short"), "short server token");
ExpectArgument(() => client.CreateSessionAsync("metro_city", "circuit", Guid.Empty), "empty request key");
ExpectArgument(() => client.CreateSessionAsync("metro_city", "unknown", requestKey), "unsupported discipline");
ExpectArgument(() => client.StartSessionAsync(Guid.Empty), "empty session id");
ExpectArgument(() => client.FinalizeSessionAsync(sessionId, Array.Empty<RaceResultSubmission>()), "empty results");

Console.WriteLine("Race-server contract tests passed: create/start/finalize payloads, stable request key, server-token header and input guards.");

sealed class RecordingHandler : HttpMessageHandler
{
    private readonly Guid _sessionId;
    private readonly Guid _playerId;

    public List<RecordedRequest> Requests { get; } = new();

    public RecordingHandler(Guid sessionId, Guid playerId)
    {
        _sessionId = sessionId;
        _playerId = playerId;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content is null
            ? string.Empty
            : await request.Content.ReadAsStringAsync(cancellationToken);
        var headers = request.Headers.ToDictionary(
            pair => pair.Key,
            pair => string.Join(",", pair.Value),
            StringComparer.OrdinalIgnoreCase);
        Requests.Add(new RecordedRequest(request.Method, request.RequestUri!, body, headers));

        using var document = JsonDocument.Parse(body);
        var operation = document.RootElement.GetProperty("operation").GetString();
        var response = operation switch
        {
            "create" => $$"""{"ok":true,"operation":"create","session":{"race_session_id":"{{_sessionId}}","race_status":"created","applied":true}}""",
            "start" => $$"""{"ok":true,"operation":"start","result":{"race_session_id":"{{_sessionId}}","race_status":"running","was_started":true}}""",
            "finalize" => $$"""{"ok":true,"operation":"finalize","results":[{"result_id":"00000000-0000-4000-8000-000000000444","user_id":"{{_playerId}}","placement":1,"reward_credits":250,"applied":true}]}""",
            _ => """{"ok":false,"error":"unsupported"}"""
        };

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(response, Encoding.UTF8, "application/json")
        };
    }
}

sealed record RecordedRequest(
    HttpMethod Method,
    Uri Uri,
    string Body,
    IReadOnlyDictionary<string, string> Headers);
