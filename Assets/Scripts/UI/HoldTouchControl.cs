using UnityEngine;
using UnityEngine.EventSystems;

namespace ApexGTWorld.UI
{
    public sealed class HoldTouchControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private TouchVehicleInput input;
        [SerializeField] private TouchControlAction action;
        private bool held;

        public void Configure(TouchVehicleInput target, TouchControlAction controlAction)
        {
            input = target;
            action = controlAction;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            held = true;
            input?.Apply(action, true);
        }

        public void OnPointerUp(PointerEventData eventData) => Release();
        public void OnPointerExit(PointerEventData eventData) => Release();

        private void OnDisable() => Release();

        private void Release()
        {
            if (!held) return;
            held = false;
            input?.Apply(action, false);
        }
    }
}
