using UnityEngine;

namespace ApexGTWorld.MotorcyclePhysics
{
    /// <summary>
    /// Early motorcycle handling experiment. This is NOT MotoGP-grade tire physics.
    /// It provides throttle/brake forces and speed-dependent lean stabilization so
    /// the basic input/scene pipeline can be tested before the full bike model.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class MotorcyclePrototypeController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float driveForce = 1100f;
        [SerializeField, Min(0f)] private float brakeForce = 1400f;
        [SerializeField, Min(0f)] private float leanTorque = 75f;
        [SerializeField, Min(0f)] private float uprightAssist = 28f;
        [SerializeField, Range(5f, 55f)] private float maxLeanAngle = 38f;
        [SerializeField, Min(0f)] private float steeringTorque = 18f;
        [SerializeField] private Transform centerOfMass;

        private Rigidbody body;
        private MotorcycleInput input;

        public float SpeedKph => body == null ? 0f : body.velocity.magnitude * 3.6f;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            if (centerOfMass != null) body.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
            else body.centerOfMass += Vector3.down * 0.45f;
        }

        public void SetInput(MotorcycleInput value) => input = value.Clamped();

        private void FixedUpdate()
        {
            if (body == null) return;
            float speedFactor = Mathf.Clamp01(SpeedKph / 35f);
            body.AddForce(transform.forward * (input.Throttle * driveForce), ForceMode.Force);
            float totalBrake = Mathf.Max(input.FrontBrake, input.RearBrake) * brakeForce;
            if (totalBrake > 0f && body.velocity.sqrMagnitude > 0.05f)
                body.AddForce(-body.velocity.normalized * totalBrake, ForceMode.Force);

            float desiredLean = -input.Steering * maxLeanAngle * speedFactor;
            float currentLean = Mathf.DeltaAngle(0f, transform.localEulerAngles.z);
            float leanError = Mathf.DeltaAngle(currentLean, desiredLean);
            body.AddRelativeTorque(Vector3.forward * (leanError * leanTorque), ForceMode.Acceleration);
            body.AddRelativeTorque(Vector3.forward * (-body.angularVelocity.z * uprightAssist), ForceMode.Acceleration);
            body.AddRelativeTorque(Vector3.up * (input.Steering * steeringTorque * speedFactor), ForceMode.Acceleration);
        }
    }
}
