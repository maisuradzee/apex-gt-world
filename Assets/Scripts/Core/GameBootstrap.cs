using UnityEngine;

namespace ApexGTWorld.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool showDevelopmentHud = true;

        private void Awake()
        {
            Application.targetFrameRate = Mathf.Clamp(targetFrameRate, 30, 120);
            QualitySettings.vSyncCount = 0;
        }

        private void OnGUI()
        {
            if (!showDevelopmentHud) return;
            GUI.Label(new Rect(16, 14, 560, 28), "APEX GT WORLD | LOCAL DEVELOPMENT BUILD");
            GUI.Label(new Rect(16, 38, 300, 24), $"FPS: {1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f):0}");
        }
    }
}
