using System;
using System.Linq;

namespace ApexGTWorld.RaceServer.Tests;

internal static class RaceSessionRuntimeTests
{
    public static void Run()
    {
        var sessionId = Guid.Parse("10000000-0000-4000-8000-000000000001");
        var first = Guid.Parse("20000000-0000-4000-8000-000000000001");
        var second = Guid.Parse("20000000-0000-4000-8000-000000000002");
        var third = Guid.Parse("20000000-0000-4000-8000-000000000003");
        var start = DateTimeOffset.Parse("2030-01-01T12:00:00.000Z");

        var empty = CreateRuntime(sessionId);
        Expect<InvalidOperationException>(() => empty.Start(start), "cannot start without entrants");
        empty.AddEntrant(first);
        Expect<InvalidOperationException>(() => empty.AddEntrant(first), "duplicate entrant");
        empty.Start(start);
        Expect<InvalidOperationException>(() => empty.AddEntrant(second), "cannot join after start");
        Expect<InvalidOperationException>(() => empty.RecordCheckpoint(first, "cp-2", start.AddMilliseconds(2)),
            "out-of-order checkpoint");
        Expect<InvalidOperationException>(() => empty.RecordCheckpoint(first, "not-on-course", start.AddMilliseconds(2)),
            "unknown checkpoint");
        Expect<InvalidOperationException>(() => empty.RecordCheckpoint(first, "cp-1", start.AddMilliseconds(-1)),
            "checkpoint timestamp before race start");

        var runtime = CreateRuntime(Guid.Parse("10000000-0000-4000-8000-000000000002"));
        runtime.AddEntrant(first);
        runtime.AddEntrant(second);
        runtime.AddEntrant(third);
        runtime.Start(start);

        runtime.RecordCheckpoint(first, "cp-1", start.AddMilliseconds(5));
        runtime.RecordCheckpoint(second, "cp-1", start.AddMilliseconds(7));
        runtime.RecordCheckpoint(third, "cp-1", start.AddMilliseconds(10));
        Expect<InvalidOperationException>(
            () => runtime.BuildFinalResults(_ => 0),
            "cannot finalize while an entrant is still active");

        runtime.RecordCheckpoint(first, "cp-2", start.AddMilliseconds(20));
        runtime.RecordCheckpoint(second, "cp-2", start.AddMilliseconds(23));
        runtime.MarkDidNotFinish(third, "disconnect_timeout", start.AddMilliseconds(30));

        var rewardCalls = 0;
        var results = runtime.BuildFinalResults(standing =>
        {
            rewardCalls++;
            return standing.Status == RaceEntrantStatus.Finished
                ? (standing.Placement == 1 ? 1000L : 500L)
                : 0L;
        });

        Check(runtime.Status == RaceRuntimeStatus.Finished, "runtime enters Finished after result generation");
        Check(results.Count == 3, "all entrants receive a result row");
        Check(results[0].UserId == first && results[0].Placement == 1, "fastest finisher is first");
        Check(results[0].FinishTimeMs == 20 && results[0].RewardCredits == 1000, "first finisher timing and reward");
        Check(results[1].UserId == second && results[1].Placement == 2, "second finisher is second");
        Check(results[1].FinishTimeMs == 23 && results[1].RewardCredits == 500, "second finisher timing and reward");
        Check(results[2].UserId == third && results[2].Placement == 3, "non-finisher is placed after finishers");
        Check(results[2].FinishTimeMs is null && results[2].RewardCredits == 0, "DNF result has no finish time");
        Check(results.All(r => r.Score == 0), "unimplemented scoring is not fabricated");

        var cached = runtime.BuildFinalResults(_ => throw new InvalidOperationException("cached results should not recalculate"));
        Check(ReferenceEquals(results, cached), "final result retry returns cached immutable collection");
        Check(rewardCalls == 3, "reward calculator is called once per entrant");

        var invalidPayout = CreateRuntime(Guid.Parse("10000000-0000-4000-8000-000000000003"));
        invalidPayout.AddEntrant(first);
        invalidPayout.Start(start);
        invalidPayout.RecordCheckpoint(first, "cp-1", start.AddMilliseconds(5));
        invalidPayout.RecordCheckpoint(first, "cp-2", start.AddMilliseconds(10));
        Expect<InvalidOperationException>(() => invalidPayout.BuildFinalResults(_ => -1), "negative payout rejected");
        Check(invalidPayout.Status == RaceRuntimeStatus.Running, "failed payout calculation does not finalize the race");
        var validAfterFailure = invalidPayout.BuildFinalResults(_ => 0);
        Check(validAfterFailure.Count == 1, "valid payout can be calculated after failed payout policy");

        var capacity = CreateRuntime(Guid.Parse("10000000-0000-4000-8000-000000000004"));
        for (var i = 1; i <= 64; i++)
            capacity.AddEntrant(Guid.Parse($"30000000-0000-4000-8000-{i:000000000000}"));
        Expect<InvalidOperationException>(
            () => capacity.AddEntrant(Guid.Parse("30000000-0000-4000-8000-000000000065")),
            "65th entrant rejected");

        var disqualification = CreateRuntime(Guid.Parse("10000000-0000-4000-8000-000000000005"));
        disqualification.AddEntrant(first);
        disqualification.Start(start);
        Expect<ArgumentException>(
            () => disqualification.Disqualify(first, "invalid reason!", start.AddMilliseconds(1)),
            "reason code format");
        disqualification.Disqualify(first, "anti_cheat_flag", start.AddMilliseconds(2));
        var dqResult = disqualification.BuildFinalResults(_ => 0).Single();
        Check(dqResult.FinishTimeMs is null && dqResult.Placement == 1,
            "disqualified player receives a final non-finish result");

        Console.WriteLine("Race-session runtime tests passed: lifecycle, entrant limits, checkpoint order, monotonic server time, DNF/DQ, deterministic placements and reward-policy retries.");
    }

    private static RaceSessionRuntime CreateRuntime(Guid sessionId) =>
        new(sessionId, "metro_city", "circuit", new[] { "cp-1", "cp-2" });

    private static void Check(bool condition, string name)
    {
        if (!condition)
            throw new InvalidOperationException("TEST FAILED: " + name);
    }

    private static void Expect<TException>(Action action, string name) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }
        throw new InvalidOperationException("TEST FAILED: expected " + typeof(TException).Name + " for " + name);
    }
}
