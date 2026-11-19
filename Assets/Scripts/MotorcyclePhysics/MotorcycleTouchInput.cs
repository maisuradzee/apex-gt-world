using UnityEngine;
using ApexGTWorld.CameraSystem;

namespace ApexGTWorld.MotorcyclePhysics
{
    public enum MotorcycleTouchAction { SteerLeft, SteerRight, Throttle, FrontBrake, RearBrake, Camera }

    public sealed class MotorcycleTouchInput : MonoBehaviour
    {
        [SerializeField] private MotorcyclePrototypeController target;
        [SerializeField] private CameraModeSwitcher cameraSwitcher;
        private MotorcycleInput state;
        private bool leftPressed;
        private bool rightPressed;

        public void Configure(MotorcyclePrototypeController controller, CameraModeSwitcher switcher)
        {
            target = controller;
            cameraSwitcher = switcher;
        }

        public void Apply(MotorcycleTouchAction action, bool pressed)
        {
            switch (action)
            {
                case MotorcycleTouchAction.SteerLeft: leftPressed = pressed; break;
                case MotorcycleTouchAction.SteerRight: rightPressed = pressed; break;
                case MotorcycleTouchAction.Throttle: state.Throttle = pressed ? 1f : 0f; break;
                case MotorcycleTouchAction.FrontBrake: state.FrontBrake = pressed ? 1f : 0f; break;
                case MotorcycleTouchAction.RearBrake: state.RearBrake = pressed ? 1f : 0f; break;
                case MotorcycleTouchAction.Camera:
                    if (pressed && cameraSwitcher != null) cameraSwitcher.Toggle();
                    break;
            }
            state.Steering = leftPressed == rightPressed ? 0f : (leftPressed ? -1f : 1f);
            if (target != null) target.SetInput(state);
        }
    }
}
