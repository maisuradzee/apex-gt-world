using UnityEngine;

namespace ApexGTWorld.CameraSystem
{
    public enum CameraMode { FirstPerson, ThirdPerson }

    public sealed class CameraModeSwitcher : MonoBehaviour
    {
        [SerializeField] private GameObject firstPersonRoot;
        [SerializeField] private GameObject thirdPersonRoot;
        [SerializeField] private CameraMode initialMode = CameraMode.ThirdPerson;
        public CameraMode CurrentMode { get; private set; }

        private void Start() => SetMode(initialMode);
        public void Toggle() => SetMode(CurrentMode == CameraMode.FirstPerson
            ? CameraMode.ThirdPerson : CameraMode.FirstPerson);

        public void SetMode(CameraMode mode)
        {
            CurrentMode = mode;
            if (firstPersonRoot != null) firstPersonRoot.SetActive(mode == CameraMode.FirstPerson);
            if (thirdPersonRoot != null) thirdPersonRoot.SetActive(mode == CameraMode.ThirdPerson);
        }
    }
}
