using UnityEngine;

namespace ApexGTWorld.VehiclePhysics
{
    public sealed class WheelVisualSync : MonoBehaviour
    {
        [SerializeField] private WheelCollider[] colliders;
        [SerializeField] private Transform[] visuals;

        public void Configure(WheelCollider[] wheelColliders, Transform[] wheelVisuals)
        {
            colliders = wheelColliders;
            visuals = wheelVisuals;
        }

        private void LateUpdate()
        {
            if (colliders == null || visuals == null) return;
            int count = Mathf.Min(colliders.Length, visuals.Length);
            for (int i = 0; i < count; i++)
            {
                if (colliders[i] == null || visuals[i] == null) continue;
                colliders[i].GetWorldPose(out Vector3 position, out Quaternion rotation);
                visuals[i].SetPositionAndRotation(position, rotation);
            }
        }
    }
}
