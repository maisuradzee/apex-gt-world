using UnityEngine;
using ApexGTWorld.CameraSystem;

namespace ApexGTWorld.VehiclePhysics
{
    /// <summary>Legacy Input Manager keyboard adapter used only for Editor tuning.</summary>
    public sealed class EditorVehicleInput : MonoBehaviour
    {
        [SerializeField] private VehicleController target;
        [SerializeField] private CameraModeSwitcher cameraSwitcher;
        [SerializeField, Range(0f, 1f)] private float steeringSensitivity = 1f;

        public void Configure(VehicleController vehicle, CameraModeSwitcher switcher)
        {
            target = vehicle;
            cameraSwitcher = switcher;
        }

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (target != null)
            {
                VehicleInput value = new VehicleInput
                {
                    Steering = Input.GetAxisRaw("Horizontal") * steeringSensitivity,
                    Throttle = Mathf.Max(0f, Input.GetAxisRaw("Vertical")),
                    Brake = Input.GetKey(KeyCode.Space) ? 1f : 0f,
                    Handbrake = Input.GetKey(KeyCode.LeftShift) ? 1f : 0f
                };
                target.SetInput(value);
            }
            if (cameraSwitcher != null && Input.GetKeyDown(KeyCode.V))
                cameraSwitcher.Toggle();
#endif
        }
    }
}
