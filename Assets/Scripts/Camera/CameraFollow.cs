using UnityEngine;

namespace ApexGTWorld.CameraSystem
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 3.2f, -7f);
        [SerializeField, Min(0.01f)] private float positionSmoothTime = 0.12f;
        [SerializeField, Min(0.01f)] private float rotationSharpness = 8f;
        private Vector3 velocity;

        public void SetTarget(Transform value) => target = value;

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desiredPosition = target.TransformPoint(localOffset);
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition,
                ref velocity, positionSmoothTime);
            Quaternion desiredRotation = Quaternion.LookRotation(
                target.position + target.forward * 5f - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation,
                1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));
        }
    }
}
