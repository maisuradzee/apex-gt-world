using UnityEngine;
using ApexGTWorld.VehiclePhysics;

namespace ApexGTWorld.UI
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        [SerializeField] private VehicleController vehicle;
        private GUIStyle labelStyle;

        private void OnGUI()
        {
            if (vehicle == null) return;
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
                labelStyle.normal.textColor = Color.white;
            }
            GUI.Box(new Rect(14, 52, 190, 52), GUIContent.none);
            GUI.Label(new Rect(24, 62, 170, 36), $"{vehicle.SpeedKph:000} km/h", labelStyle);
            GUI.Label(new Rect(18, Screen.height - 38, 620, 26), "EDITOR: WASD / Arrows to drive • SPACE brake • LEFT SHIFT handbrake • V camera");
        }
    }
}

namespace ApexGTWorld.UI
{
    public sealed class MotorcyclePrototypeHud : MonoBehaviour
    {
        [SerializeField] private ApexGTWorld.MotorcyclePhysics.MotorcyclePrototypeController motorcycle;
        private GUIStyle style;
        public void Configure(ApexGTWorld.MotorcyclePhysics.MotorcyclePrototypeController value) => motorcycle = value;
        private void OnGUI()
        {
            if (motorcycle == null) return;
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
                style.normal.textColor = Color.white;
            }
            GUI.Box(new Rect(14, 14, 210, 52), GUIContent.none);
            GUI.Label(new Rect(24, 24, 190, 36), $"{motorcycle.SpeedKph:000} km/h", style);
        }
    }
}
