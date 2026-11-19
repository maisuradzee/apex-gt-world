using UnityEngine;

namespace ApexGTWorld.Offline
{
    /// <summary>Deterministic lightweight waypoint driver for the first offline race prototype.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class OfflineAiDriver : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float speedMetersPerSecond = 15f;
        [SerializeField, Min(0.1f)] private float turnSpeedDegrees = 220f;
        [SerializeField, Min(0.1f)] private float reachDistance = 2.2f;

        private Rigidbody body;
        private Transform[] waypoints;
        private int waypointIndex;
        private float laneOffset;
        private bool driving;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        public void Configure(Transform[] route, float speed, float lateralOffset)
        {
            waypoints = route;
            speedMetersPerSecond = Mathf.Max(1f, speed);
            laneOffset = lateralOffset;
            waypointIndex = 0;
            driving = false;
        }

        public void BeginDriving()
        {
            waypointIndex = 0;
            driving = waypoints != null && waypoints.Length > 0;
        }

        public void StopDriving()
        {
            driving = false;
            if (body != null) body.velocity = Vector3.zero;
        }

        private void FixedUpdate()
        {
            if (!driving || waypoints == null || waypoints.Length == 0 || body == null) return;
            Transform marker = waypoints[waypointIndex];
            if (marker == null) return;

            Vector3 destination = marker.position + marker.right * laneOffset;
            Vector3 delta = destination - body.position;
            delta.y = 0f;

            if (delta.sqrMagnitude <= reachDistance * reachDistance)
            {
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
                return;
            }

            Quaternion desiredRotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            Quaternion nextRotation = Quaternion.RotateTowards(body.rotation, desiredRotation,
                turnSpeedDegrees * Time.fixedDeltaTime);
            body.MoveRotation(nextRotation);
            Vector3 nextPosition = Vector3.MoveTowards(body.position, destination,
                speedMetersPerSecond * Time.fixedDeltaTime);
            nextPosition.y = body.position.y;
            body.MovePosition(nextPosition);
        }
    }
}
