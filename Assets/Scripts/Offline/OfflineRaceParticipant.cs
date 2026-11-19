using UnityEngine;

namespace ApexGTWorld.Offline
{
    /// <summary>Checkpoint progress for one human or AI entrant in a local race.</summary>
    public sealed class OfflineRaceParticipant : MonoBehaviour
    {
        public string DisplayName { get; private set; }
        public bool IsPlayer { get; private set; }
        public bool IsFinished { get; private set; }
        public int CompletedLaps { get; private set; }
        public int NextCheckpointIndex { get; private set; }
        public float FinishTimeSeconds { get; private set; }
        public OfflineRaceSession Session { get; private set; }

        public void Configure(OfflineRaceSession session, string displayName, bool isPlayer)
        {
            Session = session;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName;
            IsPlayer = isPlayer;
            IsFinished = false;
            CompletedLaps = 0;
            NextCheckpointIndex = 0;
            FinishTimeSeconds = 0f;
        }

        public bool TryPassCheckpoint(int checkpointIndex)
        {
            if (Session == null || IsFinished) return false;
            return Session.TryRecordCheckpoint(this, checkpointIndex);
        }

        internal bool CanPass(int checkpointIndex) =>
            Session != null && !IsFinished && checkpointIndex == NextCheckpointIndex;

        internal void DetachSession()
        {
            Session = null;
            IsFinished = false;
            CompletedLaps = 0;
            NextCheckpointIndex = 0;
            FinishTimeSeconds = 0f;
        }

        internal void Advance(int checkpointCount)
        {
            NextCheckpointIndex++;
            if (NextCheckpointIndex >= checkpointCount)
            {
                NextCheckpointIndex = 0;
                CompletedLaps++;
            }
        }

        internal void MarkFinished(float finishTime)
        {
            IsFinished = true;
            FinishTimeSeconds = Mathf.Max(0f, finishTime);
        }
    }
}
