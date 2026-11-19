using UnityEngine;
using ApexGTWorld.VehiclePhysics;
using ApexGTWorld.CameraSystem;

namespace ApexGTWorld.UI
{
    public sealed class TouchVehicleInput : MonoBehaviour
    {
        [SerializeField] private VehicleController target;
        [SerializeField] private CameraModeSwitcher cameraSwitcher;
        private VehicleInput state;
        private bool leftPressed;
        private bool rightPressed;

        public void Configure(VehicleController vehicle, CameraModeSwitcher switcher)
        {
            target = vehicle;
            cameraSwitcher = switcher;
        }

        public void ResetInput()
        {
            leftPressed = false;
            rightPressed = false;
            state = new VehicleInput();
            Publish();
        }

        public void Apply(TouchControlAction action, bool pressed)
        {
            switch (action)
            {
                case TouchControlAction.SteerLeft: leftPressed = pressed; break;
                case TouchControlAction.SteerRight: rightPressed = pressed; break;
                case TouchControlAction.Throttle: state.Throttle = pressed ? 1f : 0f; break;
                case TouchControlAction.Brake: state.Brake = pressed ? 1f : 0f; break;
                case TouchControlAction.Handbrake: state.Handbrake = pressed ? 1f : 0f; break;
                case TouchControlAction.Camera:
                    if (pressed && cameraSwitcher != null) cameraSwitcher.Toggle();
                    break;
            }
            state.Steering = leftPressed == rightPressed ? 0f : (leftPressed ? -1f : 1f);
            Publish();
        }

        private void Publish()
        {
            if (target != null) target.SetInput(state);
        }
    }

    public enum TouchControlAction
    {
        SteerLeft, SteerRight, Throttle, Brake, Handbrake, Camera
    }
}
