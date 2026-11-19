using UnityEngine;
using UnityEngine.EventSystems;
using ApexGTWorld.MotorcyclePhysics;

namespace ApexGTWorld.UI
{
    public sealed class HoldMotorcycleTouchControl : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private MotorcycleTouchInput input;
        [SerializeField] private MotorcycleTouchAction action;
        private bool held;

        public void Configure(MotorcycleTouchInput target, MotorcycleTouchAction controlAction)
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
