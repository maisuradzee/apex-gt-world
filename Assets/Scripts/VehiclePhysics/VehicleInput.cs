using UnityEngine;

namespace ApexGTWorld.VehiclePhysics
{
    public struct VehicleInput
    {
        public float Steering;
        public float Throttle;
        public float Brake;
        public float Handbrake;

        public VehicleInput Clamped() => new VehicleInput
        {
            Steering = Mathf.Clamp(Steering, -1f, 1f),
            Throttle = Mathf.Clamp01(Throttle),
            Brake = Mathf.Clamp01(Brake),
            Handbrake = Mathf.Clamp01(Handbrake)
        };
    }
}
