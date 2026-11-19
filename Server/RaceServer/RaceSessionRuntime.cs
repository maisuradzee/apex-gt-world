using System;
using System.Collections.Generic;
using System.Linq;

namespace ApexGTWorld.RaceServer
{
    public enum RaceRuntimeStatus
    {
        Created,
        Running,
        Finished,
        Cancelled
    }

    public enum RaceEntrantStatus
    {
        Waiting,
        Racing,
        Finished,
        DidNotFinish,
        Disqualified
    }

    /// <summary>
    /// Deterministic server-side race lifecycle and checkpoint validator.
    /// Timestamps and checkpoint events must originate from trusted server code,
    /// never from a player client.
    /// </summary>
    public sealed class RaceSessionRuntime
    {
        private const int MaximumEntrants = 64;
        private const long MaximumRewardCredits = 1_000_000_000;

        private readonly object _gate = new();
        private readonly string[] _checkpoints;
        private readonly Dictionary<Guid, Entrant> _entrants = new();
        private DateTimeOffset? _startedAt;
        private RaceRuntimeStatus _status = RaceRuntimeStatus.Created;
        private IReadOnlyList<RaceResultSubmission>? _finalResults;

        public Guid SessionId { get; }
        public string RegionId { get; }
        public string Discipline { get; }

        public RaceRuntimeStatus Status
        {
            get { lock (_gate) return _status; }
        }

        public int EntrantCount
        {
            get { lock (_gate) return _entrants.Count; }
        }

        public RaceSessionRuntime(
            Guid sessionId,
            string regionId,
            string discipline,
            IReadOnlyList<string> orderedCheckpointIds)
        {
            if (sessionId == Guid.Empty)
                throw new ArgumentException("Session ID is required.", nameof(sessionId));
            if (string.IsNullOrWhiteSpace(regionId) || regionId.Length > 80)
                throw new ArgumentException("A valid region ID is required.", nameof(regionId));
            if (discipline is not ("circuit" or "time_attack" or "drift" or "endurance" or "motorcycle"))
                throw new ArgumentOutOfRangeException(nameof(discipline), "Unsupported race discipline.");
            if (orderedCheckpointIds is null || orderedCheckpointIds.Count is < 1 or > 512)
                throw new ArgumentOutOfRangeException(nameof(orderedCheckpointIds), "A course must contain 1–512 ordered checkpoints.");

            var checkpoints = orderedCheckpointIds.ToArray();
            if (checkpoints.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Checkpoint IDs cannot be empty.", nameof(orderedCheckpointIds));
            if (checkpoints.Distinct(StringComparer.Ordinal).Count() != checkpoints.Length)
                throw new ArgumentException("Checkpoint IDs must be unique within a course.", nameof(orderedCheckpointIds));

            SessionId = sessionId;
            RegionId = regionId.Trim();
            Discipline = discipline;
            _checkpoints = checkpoints;
        }

        public void AddEntrant(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Player ID is required.", nameof(userId));

            lock (_gate)
            {
                EnsureStatus(RaceRuntimeStatus.Created);
                if (_entrants.Count >= MaximumEntrants)
                    throw new InvalidOperationException("Race session has reached the 64-player limit.");
                if (_entrants.ContainsKey(userId))
                    throw new InvalidOperationException("Player has already joined this race session.");

                _entrants.Add(userId, new Entrant(userId));
            }
        }

        /// <summary>
        /// Start time must be sampled by the dedicated race server after the
        /// database start-session RPC succeeds.
        /// </summary>
        public void Start(DateTimeOffset serverStartTime)
        {
            lock (_gate)
            {
                EnsureStatus(RaceRuntimeStatus.Created);
                if (_entrants.Count is < 1 or > MaximumEntrants)
                    throw new InvalidOperationException("A race requires 1–64 registered players.");
                if (serverStartTime == default)
                    throw new ArgumentException("A valid server start time is required.", nameof(serverStartTime));

                _startedAt = serverStartTime;
                foreach (var entrant in _entrants.Values)
                {
                    entrant.Status = RaceEntrantStatus.Racing;
                    entrant.LastEventAt = serverStartTime;
                }
                _status = RaceRuntimeStatus.Running;
            }
        }

        /// <summary>
        /// Accepts only the next expected course checkpoint, in order.
        /// Call this only after the authoritative server has validated the
        /// player's position/trigger against server-side world state.
        /// </summary>
        public void RecordCheckpoint(Guid userId, string checkpointId, DateTimeOffset serverEventTime)
        {
            lock (_gate)
            {
                EnsureStatus(RaceRuntimeStatus.Running);
                var entrant = GetRacingEntrant(userId);
                ValidateServerTimestamp(entrant, serverEventTime);

                if (entrant.NextCheckpointIndex >= _checkpoints.Length)
                    throw new InvalidOperationException("Player has already completed the course.");

                var expected = _checkpoints[entrant.NextCheckpointIndex];
                if (!string.Equals(expected, checkpointId, StringComparison.Ordinal))
                    throw new InvalidOperationException("Checkpoint is out of order or does not belong to this course.");

                var completesCourse = entrant.NextCheckpointIndex == _checkpoints.Length - 1;
                long? finishTimeMs = null;
                if (completesCourse)
                {
                    var elapsed = (serverEventTime - _startedAt!.Value).TotalMilliseconds;
                    if (elapsed < 1)
                        throw new InvalidOperationException("Finish timestamp must be at least 1 ms after race start.");
                    finishTimeMs = checked((long)Math.Round(elapsed, MidpointRounding.AwayFromZero));
                }

                // Validate the entire event before mutating participant progress.
                entrant.NextCheckpointIndex++;
                entrant.LastEventAt = serverEventTime;
                if (completesCourse)
                {
                    entrant.FinishTimeMs = finishTimeMs;
                    entrant.Status = RaceEntrantStatus.Finished;
                }
            }
        }

        public void MarkDidNotFinish(Guid userId, string reasonCode, DateTimeOffset serverEventTime)
        {
            MarkTerminal(userId, reasonCode, serverEventTime, RaceEntrantStatus.DidNotFinish);
        }

        public void Disqualify(Guid userId, string reasonCode, DateTimeOffset serverEventTime)
        {
            MarkTerminal(userId, reasonCode, serverEventTime, RaceEntrantStatus.Disqualified);
        }

        /// <summary>
        /// Produces stable final placements. The reward calculator is supplied
        /// by trusted server configuration; player clients never choose payouts.
        /// Identical calls after success return the cached result set.
        /// </summary>
        public IReadOnlyList<RaceResultSubmission> BuildFinalResults(
            Func<RaceStanding, long> serverRewardCalculator)
        {
            if (serverRewardCalculator is null)
                throw new ArgumentNullException(nameof(serverRewardCalculator));

            lock (_gate)
            {
                if (_finalResults is not null)
                    return _finalResults;
                EnsureStatus(RaceRuntimeStatus.Running);

                if (_entrants.Count == 0 || _entrants.Values.Any(e =>
                    e.Status is not (RaceEntrantStatus.Finished or RaceEntrantStatus.DidNotFinish or RaceEntrantStatus.Disqualified)))
                    throw new InvalidOperationException("Every entrant must finish, DNF or be disqualified before finalization.");

                var ordered = _entrants.Values
                    .OrderBy(e => e.Status == RaceEntrantStatus.Finished ? 0 : 1)
                    .ThenBy(e => e.Status == RaceEntrantStatus.Finished ? e.FinishTimeMs : null)
                    .ThenByDescending(e => e.NextCheckpointIndex)
                    .ThenBy(e => e.LastEventAt)
                    .ThenBy(e => e.UserId)
                    .ToArray();

                var results = new List<RaceResultSubmission>(ordered.Length);
                for (var i = 0; i < ordered.Length; i++)
                {
                    var entrant = ordered[i];
                    var standing = new RaceStanding(
                        entrant.UserId,
                        i + 1,
                        entrant.Status,
                        entrant.NextCheckpointIndex,
                        _checkpoints.Length,
                        entrant.FinishTimeMs,
                        0,
                        entrant.TerminalReasonCode);

                    var reward = serverRewardCalculator(standing);
                    if (reward is < 0 or > MaximumRewardCredits)
                        throw new InvalidOperationException("Server reward calculator returned an amount outside the allowed range.");

                    results.Add(new RaceResultSubmission
                    {
                        UserId = entrant.UserId,
                        Placement = standing.Placement,
                        FinishTimeMs = standing.FinishTimeMs,
                        Score = standing.Score,
                        RewardCredits = reward
                    });
                }

                _finalResults = results.AsReadOnly();
                _status = RaceRuntimeStatus.Finished;
                return _finalResults;
            }
        }

        private void MarkTerminal(
            Guid userId,
            string reasonCode,
            DateTimeOffset serverEventTime,
            RaceEntrantStatus terminalStatus)
        {
            if (string.IsNullOrWhiteSpace(reasonCode) || reasonCode.Length > 40 ||
                reasonCode.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '_' or '-')))
                throw new ArgumentException("Reason code must be a short identifier.", nameof(reasonCode));

            lock (_gate)
            {
                EnsureStatus(RaceRuntimeStatus.Running);
                var entrant = GetRacingEntrant(userId);
                ValidateServerTimestamp(entrant, serverEventTime);
                entrant.LastEventAt = serverEventTime;
                entrant.TerminalReasonCode = reasonCode;
                entrant.Status = terminalStatus;
            }
        }

        private Entrant GetRacingEntrant(Guid userId)
        {
            if (!_entrants.TryGetValue(userId, out var entrant))
                throw new InvalidOperationException("Player is not registered in this race session.");
            if (entrant.Status != RaceEntrantStatus.Racing)
                throw new InvalidOperationException("Player is not currently racing.");
            return entrant;
        }

        private static void ValidateServerTimestamp(Entrant entrant, DateTimeOffset eventTime)
        {
            if (eventTime == default || eventTime < entrant.LastEventAt)
                throw new InvalidOperationException("Server race event timestamps must be valid and monotonic.");
        }

        private void EnsureStatus(RaceRuntimeStatus expected)
        {
            if (_status != expected)
                throw new InvalidOperationException($"Race session must be {expected}, but is {_status}.");
        }

        private sealed class Entrant
        {
            public Guid UserId { get; }
            public RaceEntrantStatus Status { get; set; } = RaceEntrantStatus.Waiting;
            public int NextCheckpointIndex { get; set; }
            public DateTimeOffset LastEventAt { get; set; }
            public long? FinishTimeMs { get; set; }
            public string? TerminalReasonCode { get; set; }

            public Entrant(Guid userId) => UserId = userId;
        }
    }

    public sealed record RaceStanding(
        Guid UserId,
        int Placement,
        RaceEntrantStatus Status,
        int CompletedCheckpoints,
        int TotalCheckpoints,
        long? FinishTimeMs,
        int Score,
        string? TerminalReasonCode);
}
