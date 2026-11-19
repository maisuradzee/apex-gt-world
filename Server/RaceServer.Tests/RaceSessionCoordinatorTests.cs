using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ApexGTWorld.RaceServer;

namespace ApexGTWorld.RaceServer.Tests;

internal static class RaceSessionCoordinatorTests
{
    public static async Task RunAsync()
    {
        var sessionId = Guid.Parse("40000000-0000-4000-8000-000000000001");
        var first = Guid.Parse("50000000-0000-4000-8000-000000000001");
        var second = Guid.Parse("50000000-0000-4000-8000-000000000002");
        var requestKey = Guid.Parse("60000000-0000-4000-8000-000000000001");
        var start = DateTimeOffset.Parse("2030-02-01T09:00:00.000Z");
        var endpoint = new Uri("https://example.supabase.co/functions/v1/race-session-control");
        var handler = new CoordinatorHandler(sessionId) { FailFirstFinalize = true };

        using var http = new HttpClient(handler);
        using var control = new RaceControlClient(endpoint, "test-only-token-for-coordinator-123456789", http);

        var coordinator = await RaceSessionCoordinator.CreateAsync(
            control,
            "metro_city",
            "circuit",
            requestKey,
            new[] { "start_finish", "sector_1", "finish" });

        Check(coordinator.SessionId == sessionId, "coordinator uses the database session ID");
        Check(coordinator.Status == RaceRuntimeStatus.Created, "created coordinator begins in Created state");
        await ExpectAsync<InvalidOperationException>(
            () => coordinator.StartAsync(start),
            "race cannot start before an entrant joins");
        Check(handler.Requests.Count == 1, "invalid local start sends no remote start request");

        coordinator.AddEntrant(first);
        coordinator.AddEntrant(second);
        await coordinator.StartAsync(start);
        Check(coordinator.Status == RaceRuntimeStatus.Running, "coordinator starts local runtime after remote start");

        coordinator.RecordCheckpoint(first, "start_finish", start.AddMilliseconds(10));
        coordinator.RecordCheckpoint(second, "start_finish", start.AddMilliseconds(11));
        coordinator.RecordCheckpoint(first, "sector_1", start.AddMilliseconds(20));
        coordinator.RecordCheckpoint(first, "finish", start.AddMilliseconds(30));
        coordinator.MarkDidNotFinish(second, "disconnect_timeout", start.AddMilliseconds(40));

        await ExpectAsync<HttpRequestException>(
            () => coordinator.FinalizeAsync(_ => 0),
            "first finalize network attempt fails");
        Check(coordinator.Status == RaceRuntimeStatus.Finished,
            "runtime caches final state before network finalization");
        Check(handler.Requests.Count(r => r.Operation == "finalize") == 1,
            "first finalize attempt was sent once");

        var finalResults = await coordinator.FinalizeAsync(_ =>
            throw new InvalidOperationException("retry must reuse cached reward calculation"));
        Check(finalResults.Count == 2, "coordinator returns both final participant results");
        Check(handler.Requests.Select(r => r.Operation).SequenceEqual(new[] { "create", "start", "finalize", "finalize" }),
            "coordinator sends lifecycle operations in order and retries finalization");

        using (var createBody = JsonDocument.Parse(handler.Requests[0].Body))
        {
            Check(createBody.RootElement.GetProperty("request_key").GetGuid() == requestKey,
                "coordinator sends caller's stable create request key");
        }
        using (var finalizeBody = JsonDocument.Parse(handler.Requests[3].Body))
        {
            var root = finalizeBody.RootElement;
            Check(root.GetProperty("session_id").GetGuid() == sessionId, "finalize uses database session ID");
            var rows = root.GetProperty("results");
            Check(rows.GetArrayLength() == 2, "finalize retries the full result set");
            Check(rows[0].GetProperty("user_id").GetGuid() == first, "winner is first in retry payload");
            Check(rows[0].GetProperty("finish_time_ms").GetInt64() == 30, "finish time is server-derived");
            Check(rows[1].GetProperty("finish_time_ms").ValueKind == JsonValueKind.Null,
                "DNF is represented without a finish time");
        }

        var invalidCourseHandler = new CoordinatorHandler(sessionId);
        using var invalidHttp = new HttpClient(invalidCourseHandler);
        using var invalidControl = new RaceControlClient(endpoint,
            "test-only-token-for-coordinator-123456789", invalidHttp);
        await ExpectAsync<ArgumentException>(
            () => RaceSessionCoordinator.CreateAsync(
                invalidControl,
                "metro_city",
                "circuit",
                requestKey,
                new[] { "duplicate", "duplicate" }),
            "invalid course is rejected before remote session creation");
        Check(invalidCourseHandler.Requests.Count == 0,
            "invalid course configuration never creates a database session");

        Console.WriteLine("Race-session coordinator tests passed: remote/local lifecycle ordering, stable create key, server-timed results and identical finalization retry.");
    }

    private static void Check(bool condition, string name)
    {
        if (!condition)
            throw new InvalidOperationException("TEST FAILED: " + name);
    }

    private static async Task ExpectAsync<TException>(Func<Task> action, string name) where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException)
        {
            return;
        }
        throw new InvalidOperationException("TEST FAILED: expected " + typeof(TException).Name + " for " + name);
    }

    private sealed class CoordinatorHandler : HttpMessageHandler
    {
        private readonly Guid _sessionId;
        private bool _finalizeFailed;

        public bool FailFirstFinalize { get; init; }
        public List<RecordedRequest> Requests { get; } = new();

        public CoordinatorHandler(Guid sessionId) => _sessionId = sessionId;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            var operation = document.RootElement.GetProperty("operation").GetString() ?? string.Empty;
            Requests.Add(new RecordedRequest(operation, body));

            if (operation == "finalize" && FailFirstFinalize && !_finalizeFailed)
            {
                _finalizeFailed = true;
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("{\"error\":\"temporary failure\"}", Encoding.UTF8, "application/json")
                };
            }

            var response = operation switch
            {
                "create" => $"{{\"ok\":true,\"operation\":\"create\",\"session\":{{\"race_session_id\":\"{_sessionId}\",\"race_status\":\"created\",\"applied\":true}}}}",
                "start" => $"{{\"ok\":true,\"operation\":\"start\",\"result\":{{\"race_session_id\":\"{_sessionId}\",\"race_status\":\"running\",\"was_started\":true}}}}",
                "finalize" => $"{{\"ok\":true,\"operation\":\"finalize\",\"results\":[]}}",
                _ => "{\"ok\":false,\"error\":\"unsupported\"}"
            };
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed record RecordedRequest(string Operation, string Body);
}
