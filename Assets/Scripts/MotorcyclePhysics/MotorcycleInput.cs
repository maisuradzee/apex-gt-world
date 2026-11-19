using UnityEngine;

namespace ApexGTWorld.MotorcyclePhysics
{
    public struct MotorcycleInput
    {
        public float Steering;
        public float Throttle;
        public float FrontBrake;
        public float RearBrake;

        public MotorcycleInput Clamped() => new MotorcycleInput
        {
            Steering = Mathf.Clamp(Steering, -1f, 1f),
            Throttle = Mathf.Clamp01(Throttle),
            FrontBrake = Mathf.Clamp01(FrontBrake),
            RearBrake = Mathf.Clamp01(RearBrake)
        };
    }
}
