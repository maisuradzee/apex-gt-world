using System;
using System.Collections.Generic;
using UnityEngine;

namespace ApexGTWorld.Offline
{
    [Serializable]
    public sealed class OfflineRaceDefinition
    {
        public string eventId = "metro_circuit_club_cup";
        public string displayName = "Metro Circuit Club Cup";
        [Min(1)] public int laps = 2;
        [Min(2)] public int checkpointCount = 4;
        [Min(0)] public long rewardCredits = 500;
        [Min(0)] public long rewardXp = 250;
        [Min(0f)] public float rewardDistanceMeters = 400f;
    }

    /// <summary>
    /// Local race rules. This is for offline progression only and is not an
    /// authoritative result source for online leaderboards or cloud payouts.
    /// </summary>
    public sealed class OfflineRaceSession
    {
        private readonly OfflineRaceDefinition definition;
        private readonly float startedAt;
        private readonly List<OfflineRaceParticipant> participants;
        private bool playerRewardApplied;

        public OfflineRaceDefinition Definition => definition;
        public bool IsPlayerFinished { get; private set; }
        public float PlayerFinishTimeSeconds { get; private set; }
        public float ElapsedSeconds => IsPlayerFinished ? PlayerFinishTimeSeconds : Mathf.Max(0f, Time.time - startedAt);
        public bool PlayerRewardApplied { get; private set; }
        public string Status { get; private set; } = "RUNNING";
        public IReadOnlyList<OfflineRaceParticipant> Participants => participants;

        public OfflineRaceSession(OfflineRaceDefinition raceDefinition,
            IEnumerable<OfflineRaceParticipant> entrants)
        {
            if (raceDefinition == null) throw new ArgumentNullException(nameof(raceDefinition));
            if (string.IsNullOrWhiteSpace(raceDefinition.eventId))
                throw new ArgumentException("Race event ID is required.", nameof(raceDefinition));
            if (raceDefinition.laps < 1 || raceDefinition.laps > 20)
                throw new ArgumentOutOfRangeException(nameof(raceDefinition), "Offline race laps must be between 1 and 20.");
            if (raceDefinition.checkpointCount < 2 || raceDefinition.checkpointCount > 64)
                throw new ArgumentOutOfRangeException(nameof(raceDefinition), "Checkpoint count must be between 2 and 64.");

            definition = raceDefinition;
            participants = new List<OfflineRaceParticipant>();
            if (entrants != null)
            {
                foreach (OfflineRaceParticipant entrant in entrants)
                    if (entrant != null && !participants.Contains(entrant))
                        participants.Add(entrant);
            }

            if (participants.Count == 0)
                throw new ArgumentException("At least one race participant is required.", nameof(entrants));

            startedAt = Time.time;
            for (int i = 0; i < participants.Count; i++)
            {
                OfflineRaceParticipant participant = participants[i];
                participant.Configure(this, participant.DisplayName, participant.IsPlayer);
            }
        }

        public bool TryRecordCheckpoint(OfflineRaceParticipant participant, int checkpointIndex)
        {
            if (participant == null || !participants.Contains(participant) ||
                !participant.CanPass(checkpointIndex) || IsPlayerFinished)
                return false;

            participant.Advance(definition.checkpointCount);
            if (participant.CompletedLaps < definition.laps) return true;

            float finishTime = Mathf.Max(0f, Time.time - startedAt);
            participant.MarkFinished(finishTime);

            if (participant.IsPlayer)
            {
                IsPlayerFinished = true;
                PlayerFinishTimeSeconds = finishTime;
                if (!playerRewardApplied)
                {
                    playerRewardApplied = true;
                    OfflineGameManager manager = OfflineGameManager.Instance;
                    if (manager != null)
                        PlayerRewardApplied = manager.CompleteEvent(definition.eventId, definition.rewardCredits,
                            definition.rewardXp, definition.rewardDistanceMeters);
                }
                Status = "PLAYER FINISHED";
            }

            return true;
        }
    }
}
