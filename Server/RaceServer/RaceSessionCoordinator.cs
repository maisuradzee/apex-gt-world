using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ApexGTWorld.RaceServer
{
    /// <summary>
    /// Coordinates the local authoritative race runtime with the protected
    /// Supabase session lifecycle API. Run one coordinator per live race session.
    /// </summary>
    public sealed class RaceSessionCoordinator
    {
        private readonly RaceControlClient _control;
        private readonly RaceSessionRuntime _runtime;

        private RaceSessionCoordinator(RaceControlClient control, RaceSessionRuntime runtime)
        {
            _control = control;
            _runtime = runtime;
        }

        public Guid SessionId => _runtime.SessionId;
        public RaceRuntimeStatus Status => _runtime.Status;
        public int EntrantCount => _runtime.EntrantCount;

        public static async Task<RaceSessionCoordinator> CreateAsync(
            RaceControlClient control,
            string regionId,
            string discipline,
            Guid requestKey,
            IReadOnlyList<string> orderedCheckpointIds,
            CancellationToken cancellationToken = default)
        {
            if (control is null)
                throw new ArgumentNullException(nameof(control));

            // Validate local course configuration before creating a remote row.
            _ = new RaceSessionRuntime(Guid.NewGuid(), regionId, discipline, orderedCheckpointIds);

            var response = await control.CreateSessionAsync(
                regionId,
                discipline,
                requestKey,
                cancellationToken).ConfigureAwait(false);

            if (!response.Ok || response.Session is not JsonElement session ||
                session.ValueKind != JsonValueKind.Object ||
                !session.TryGetProperty("race_session_id", out var idElement) ||
                idElement.ValueKind != JsonValueKind.String ||
                !Guid.TryParse(idElement.GetString(), out var sessionId) ||
                sessionId == Guid.Empty ||
                !session.TryGetProperty("race_status", out var statusElement) ||
                statusElement.GetString() != "created" ||
                !session.TryGetProperty("region_id", out var regionElement) ||
                regionElement.GetString() != regionId ||
                !session.TryGetProperty("discipline", out var disciplineElement) ||
                disciplineElement.GetString() != discipline)
            {
                throw new InvalidOperationException("Race control did not return a valid created session ID.");
            }

            var runtime = new RaceSessionRuntime(sessionId, regionId, discipline, orderedCheckpointIds);
            return new RaceSessionCoordinator(control, runtime);
        }

        public void AddEntrant(Guid userId) => _runtime.AddEntrant(userId);

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_runtime.Status != RaceRuntimeStatus.Created || _runtime.EntrantCount < 1)
                throw new InvalidOperationException("Register at least one entrant before starting the race.");

            var response = await _control.StartSessionAsync(SessionId, cancellationToken).ConfigureAwait(false);
            if (!response.Ok ||
                response.Result is not JsonElement result ||
                result.ValueKind != JsonValueKind.Object ||
                !result.TryGetProperty("race_started_at", out var startedAtElement) ||
                startedAtElement.ValueKind != JsonValueKind.String ||
                !DateTimeOffset.TryParse(
                    startedAtElement.GetString(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var databaseStartedAt) ||
                databaseStartedAt == default)
            {
                throw new InvalidOperationException("Race control did not return a valid authoritative start timestamp.");
            }

            // Use the timestamp stored by PostgreSQL, not the client's request time
            // or a pre-request local clock sample.
            _runtime.Start(databaseStartedAt);
        }

        public void RecordCheckpoint(Guid userId, string checkpointId, DateTimeOffset serverEventTime) =>
            _runtime.RecordCheckpoint(userId, checkpointId, serverEventTime);

        public void MarkDidNotFinish(Guid userId, string reasonCode, DateTimeOffset serverEventTime) =>
            _runtime.MarkDidNotFinish(userId, reasonCode, serverEventTime);

        public void Disqualify(Guid userId, string reasonCode, DateTimeOffset serverEventTime) =>
            _runtime.Disqualify(userId, reasonCode, serverEventTime);

        public async Task<IReadOnlyList<RaceResultSubmission>> FinalizeAsync(
            Func<RaceStanding, long> serverRewardCalculator,
            CancellationToken cancellationToken = default)
        {
            // The runtime caches this payload before network I/O. A transient HTTP
            // failure can therefore be retried with the exact same result set.
            var results = _runtime.BuildFinalResults(serverRewardCalculator);
            var response = await _control.FinalizeSessionAsync(
                SessionId,
                results,
                cancellationToken).ConfigureAwait(false);

            if (!response.Ok)
                throw new InvalidOperationException("Race control rejected race finalization.");

            return results;
        }
    }
}
