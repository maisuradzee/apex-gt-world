using UnityEngine;

namespace ApexGTWorld.Offline
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class OfflineRaceCheckpoint : MonoBehaviour
    {
        [SerializeField] private int checkpointIndex;
        public int CheckpointIndex => checkpointIndex;

        public void Configure(int index)
        {
            checkpointIndex = Mathf.Max(0, index);
            BoxCollider trigger = GetComponent<BoxCollider>();
            trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            OfflineRaceParticipant participant = other.GetComponentInParent<OfflineRaceParticipant>();
            if (participant != null)
                participant.TryPassCheckpoint(checkpointIndex);
        }
    }
}
