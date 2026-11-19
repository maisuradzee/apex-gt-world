using UnityEngine;

namespace ApexGTWorld.VehiclePhysics
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class VehicleController : MonoBehaviour
    {
        [Header("Wheel colliders")]
        [SerializeField] private WheelCollider[] steerWheels;
        [SerializeField] private WheelCollider[] driveWheels;
        [SerializeField] private WheelCollider[] frontBrakeWheels;
        [SerializeField] private WheelCollider[] rearBrakeWheels;

        [Header("Prototype tuning — calibrate per vehicle")]
        [SerializeField, Min(0f)] private float motorTorque = 950f;
        [SerializeField, Min(0f)] private float serviceBrakeTorque = 2200f;
        [SerializeField, Min(0f)] private float handbrakeTorque = 3000f;
        [SerializeField, Range(5f, 45f)] private float maxSteerAngle = 30f;
        [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, -0.35f, 0f);

        private Rigidbody body;
        private VehicleInput input;
        private float baseMotorTorque;
        private float baseServiceBrakeTorque;
        private float baseHandbrakeTorque;
        private float baseMaxSteerAngle;

        public float SpeedKph => body == null ? 0f : body.velocity.magnitude * 3.6f;
        public Rigidbody Body => body;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.centerOfMass += centerOfMassOffset;
            baseMotorTorque = motorTorque;
            baseServiceBrakeTorque = serviceBrakeTorque;
            baseHandbrakeTorque = handbrakeTorque;
            baseMaxSteerAngle = maxSteerAngle;
        }

        private void Start() => ValidateWheelConfiguration();

        public void Configure(WheelCollider[] steering, WheelCollider[] drive,
            WheelCollider[] frontBrakes, WheelCollider[] rearBrakes)
        {
            steerWheels = steering;
            driveWheels = drive;
            frontBrakeWheels = frontBrakes;
            rearBrakeWheels = rearBrakes;
        }

        public void SetInput(VehicleInput nextInput) => input = nextInput.Clamped();

        public void ApplyGarageUpgrades(int engineLevel, int handlingLevel, int brakeLevel)
        {
            engineLevel = Mathf.Clamp(engineLevel, 0, 5);
            handlingLevel = Mathf.Clamp(handlingLevel, 0, 5);
            brakeLevel = Mathf.Clamp(brakeLevel, 0, 5);
            motorTorque = baseMotorTorque * (1f + engineLevel * 0.09f);
            maxSteerAngle = Mathf.Clamp(baseMaxSteerAngle + handlingLevel * 1.5f, 5f, 45f);
            serviceBrakeTorque = baseServiceBrakeTorque * (1f + brakeLevel * 0.10f);
            handbrakeTorque = baseHandbrakeTorque * (1f + brakeLevel * 0.08f);
        }

        private void FixedUpdate()
        {
            if (body == null) return;

            float steer = input.Steering * maxSteerAngle;
            foreach (WheelCollider wheel in steerWheels)
                if (wheel != null) wheel.steerAngle = steer;

            foreach (WheelCollider wheel in driveWheels)
                if (wheel != null) wheel.motorTorque = input.Throttle * motorTorque;

            float frontBrake = input.Brake * serviceBrakeTorque;
            float rearBrake = Mathf.Max(frontBrake, input.Handbrake * handbrakeTorque);
            ApplyBrake(frontBrakeWheels, frontBrake);
            ApplyBrake(rearBrakeWheels, rearBrake);
        }

        private static void ApplyBrake(WheelCollider[] wheels, float torque)
        {
            if (wheels == null) return;
            foreach (WheelCollider wheel in wheels)
                if (wheel != null) wheel.brakeTorque = torque;
        }

        private void ValidateWheelConfiguration()
        {
            if (steerWheels == null || steerWheels.Length == 0)
                Debug.LogWarning($"{name}: no steering WheelColliders assigned.", this);
            if (driveWheels == null || driveWheels.Length == 0)
                Debug.LogWarning($"{name}: no drive WheelColliders assigned.", this);
        }
    }
}
