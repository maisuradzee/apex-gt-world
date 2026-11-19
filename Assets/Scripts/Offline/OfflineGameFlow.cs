using UnityEngine;
using ApexGTWorld.VehiclePhysics;
using ApexGTWorld.CameraSystem;
using ApexGTWorld.UI;

namespace ApexGTWorld.Offline
{
    /// <summary>
    /// Offline-first front end and playable flow for the generated starter car scene.
    /// Uses Unity IMGUI so the first scene needs no external UI packages or assets.
    /// </summary>
    public sealed class OfflineGameFlow : MonoBehaviour
    {
        private enum ScreenState { MainMenu, Career, Garage, Settings, FreeDrive, Racing, Paused, Results }

        private ScreenState state = ScreenState.MainMenu;
        private ScreenState pausedFrom = ScreenState.FreeDrive;
        private GameObject playerRoot;
        private GameObject hudRoot;
        private GameObject touchRoot;
        private VehicleController vehicle;
        private EditorVehicleInput keyboardInput;
        private CameraModeSwitcher cameraSwitcher;
        private OfflineRaceParticipant playerParticipant;
        private OfflineRaceCourse raceCourse;
        private string message = "Your progress is stored on this device.";
        private int qualityLevel = 2;

        private void Start()
        {
            playerRoot = GameObject.Find("Prototype Car");
            if (playerRoot == null)
            {
                message = "Starter vehicle was not found. Generate the Starter Car Scene.";
                Debug.LogError("[APEX OFFLINE] " + message);
                return;
            }

            vehicle = playerRoot.GetComponent<VehicleController>();
            keyboardInput = playerRoot.GetComponent<EditorVehicleInput>();
            cameraSwitcher = playerRoot.GetComponent<CameraModeSwitcher>();
            playerParticipant = playerRoot.GetComponent<OfflineRaceParticipant>();
            hudRoot = GameObject.Find("Prototype HUD");
            touchRoot = GameObject.Find("Car Mobile Controls");
            raceCourse = GetComponent<OfflineRaceCourse>();

            if (raceCourse != null)
                raceCourse.Configure(playerRoot);
            ApplyGarageTuning();

            qualityLevel = Mathf.Clamp(QualitySettings.GetQualityLevel(), 0,
                Mathf.Max(0, QualitySettings.names.Length - 1));
            SetGameplayEnabled(false);
            Time.timeScale = 0f;
        }

        private void Update()
        {
            if (state == ScreenState.FreeDrive || state == ScreenState.Racing)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    PauseGame();
            }

            if (state == ScreenState.Racing && raceCourse != null && raceCourse.PlayerFinished)
            {
                state = ScreenState.Results;
                Time.timeScale = 0f;
                SetGameplayEnabled(false);
                OfflineGameManager manager = OfflineGameManager.Instance;
                message = manager != null ? manager.Status : "Race finished.";
            }
        }

        private void OnGUI()
        {
            if (vehicle == null) return;

            if (state == ScreenState.FreeDrive || state == ScreenState.Racing)
            {
                DrawGameplayHud();
                return;
            }

            float panelWidth = Mathf.Min(520f, Screen.width - 32f);
            float panelHeight = Mathf.Min(620f, Screen.height - 32f);
            Rect panel = new Rect((Screen.width - panelWidth) * 0.5f,
                (Screen.height - panelHeight) * 0.5f, panelWidth, panelHeight);

            Color oldColor = GUI.color;
            GUI.color = new Color(0.015f, 0.025f, 0.045f, 0.94f);
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
            GUI.color = oldColor;
            GUI.Box(panel, GUIContent.none);

            float left = panel.x + 34f;
            float width = panel.width - 68f;
            float y = panel.y + 24f;
            DrawTitle(left, ref y, width);

            switch (state)
            {
                case ScreenState.MainMenu: DrawMainMenu(left, ref y, width); break;
                case ScreenState.Career: DrawCareer(left, ref y, width); break;
                case ScreenState.Garage: DrawGarage(left, ref y, width); break;
                case ScreenState.Settings: DrawSettings(left, ref y, width); break;
                case ScreenState.Paused: DrawPause(left, ref y, width); break;
                case ScreenState.Results: DrawResults(left, ref y, width); break;
            }
        }

        private void DrawTitle(float x, ref float y, float width)
        {
            GUIStyle title = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Screen.width / 42, 24, 38),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            title.normal.textColor = Color.white;
            GUI.Label(new Rect(x, y, width, 52f), "APEX GT WORLD", title);
            y += 54f;

            GUIStyle subtitle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter
            };
            subtitle.normal.textColor = new Color(0.35f, 0.78f, 1f);
            GUI.Label(new Rect(x, y, width, 24f), "OFFLINE SINGLE PLAYER", subtitle);
            y += 38f;

            OfflinePlayerProfile profile = OfflineGameManager.Instance != null
                ? OfflineGameManager.Instance.Profile : null;
            string stats = profile == null ? "Local profile unavailable" :
                profile.displayName + "   |   LEVEL " + profile.careerLevel +
                "   |   CREDITS " + profile.credits;
            GUI.Label(new Rect(x, y, width, 26f), stats);
            y += 40f;
        }

        private void DrawMainMenu(float x, ref float y, float width)
        {
            AddButton(x, ref y, width, "START FREE DRIVE", StartFreeDrive);
            AddButton(x, ref y, width, "CAREER", () => state = ScreenState.Career);
            AddButton(x, ref y, width, "GARAGE", () => state = ScreenState.Garage);
            AddButton(x, ref y, width, "SETTINGS", () => state = ScreenState.Settings);
            AddButton(x, ref y, width, "EXIT", () => Application.Quit());
            DrawMessage(x, ref y, width);
            DrawFooter(x, panelSafeWidth(width), ref y);
        }

        private void DrawCareer(float x, ref float y, float width)
        {
            OfflineRaceDefinition race = raceCourse != null ? raceCourse.DefaultEvent : null;
            GUI.Label(new Rect(x, y, width, 28f), "AVAILABLE EVENT");
            y += 34f;
            if (race == null)
            {
                DrawMessage(x, ref y, width);
            }
            else
            {
                GUI.Label(new Rect(x, y, width, 28f), race.displayName);
                y += 30f;
                GUI.Label(new Rect(x, y, width, 26f), race.laps + " laps  |  4 checkpoints  |  3 AI rivals");
                y += 30f;
                GUI.Label(new Rect(x, y, width, 26f), "Prize: " + race.rewardCredits + " Credits  +  " + race.rewardXp + " XP");
                y += 42f;
                AddButton(x, ref y, width, "START CAREER RACE", StartCareerRace);
            }
            AddButton(x, ref y, width, "BACK", () => state = ScreenState.MainMenu);
        }

        private void DrawGarage(float x, ref float y, float width)
        {
            OfflinePlayerProfile profile = OfflineGameManager.Instance != null
                ? OfflineGameManager.Instance.Profile : null;
            GUI.Label(new Rect(x, y, width, 28f), "YOUR VEHICLES");
            y += 38f;
            GUI.Label(new Rect(x, y, width, 28f), "STARTER CAR  |  OWNED");
            y += 30f;
            GUI.Label(new Rect(x, y, width, 28f), "Selected: " +
                (profile != null ? profile.selectedVehicleId : "Unavailable"));
            y += 30f;
            GUI.Label(new Rect(x, y, width, 28f), "Owned vehicles: " +
                (profile != null && profile.ownedVehicleIds != null ? profile.ownedVehicleIds.Length : 0));
            y += 36f;
            if (profile != null)
            {
                AddButton(x, ref y, width, "ENGINE LVL " + profile.engineUpgradeLevel + "  |  UPGRADE 250 CR", () => PurchaseUpgrade("engine", 250));
                AddButton(x, ref y, width, "HANDLING LVL " + profile.handlingUpgradeLevel + "  |  UPGRADE 250 CR", () => PurchaseUpgrade("handling", 250));
                AddButton(x, ref y, width, "BRAKES LVL " + profile.brakeUpgradeLevel + "  |  UPGRADE 250 CR", () => PurchaseUpgrade("brakes", 250));
            }
            DrawMessage(x, ref y, width);
            AddButton(x, ref y, width, "BACK", () => state = ScreenState.MainMenu);
        }

        private void DrawSettings(float x, ref float y, float width)
        {
            GUI.Label(new Rect(x, y, width, 28f), "GRAPHICS QUALITY");
            y += 34f;
            string qualityName = QualitySettings.names.Length > 0
                ? QualitySettings.names[Mathf.Clamp(qualityLevel, 0, QualitySettings.names.Length - 1)]
                : "Default";
            AddButton(x, ref y, width, "QUALITY: " + qualityName.ToUpperInvariant(), CycleQuality);
            AddButton(x, ref y, width, "FRAME RATE: 60 FPS", () =>
            {
                Application.targetFrameRate = Application.targetFrameRate >= 60 ? 30 : 60;
                message = "Frame-rate cap set to " + Application.targetFrameRate + " FPS.";
            });
            AddButton(x, ref y, width, "BACK", () => state = ScreenState.MainMenu);
            DrawMessage(x, ref y, width);
        }

        private void DrawPause(float x, ref float y, float width)
        {
            AddButton(x, ref y, width, "RESUME", ResumeGame);
            AddButton(x, ref y, width, "MAIN MENU", ReturnToMainMenu);
        }

        private void DrawResults(float x, ref float y, float width)
        {
            GUIStyle resultStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            resultStyle.normal.textColor = new Color(0.35f, 0.95f, 0.55f);
            GUI.Label(new Rect(x, y, width, 44f), "RACE FINISHED", resultStyle);
            y += 54f;
            GUI.Label(new Rect(x, y, width, 30f), "Finish time: " +
                (raceCourse != null ? raceCourse.PlayerFinishTime.ToString("0.000") : "0.000") + " sec");
            y += 34f;
            DrawMessage(x, ref y, width);
            AddButton(x, ref y, width, "CONTINUE DRIVING", StartFreeDrive);
            AddButton(x, ref y, width, "MAIN MENU", ReturnToMainMenu);
        }

        private void DrawGameplayHud()
        {
            GUIStyle speedStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
            speedStyle.normal.textColor = Color.white;
            GUI.Box(new Rect(14f, 14f, 220f, 62f), GUIContent.none);
            GUI.Label(new Rect(26f, 24f, 200f, 42f), vehicle.SpeedKph.ToString("000") + " KM/H", speedStyle);

            if (state == ScreenState.Racing && raceCourse != null && raceCourse.CurrentSession != null)
            {
                OfflineRaceSession race = raceCourse.CurrentSession;
                if (playerParticipant == null)
                    playerParticipant = playerRoot.GetComponent<OfflineRaceParticipant>();
                int lap = playerParticipant != null ? Mathf.Min(race.Definition.laps, playerParticipant.CompletedLaps + 1) : 1;
                int nextGate = playerParticipant != null ? playerParticipant.NextCheckpointIndex + 1 : 1;
                GUI.Box(new Rect(14f, 84f, 290f, 72f), GUIContent.none);
                GUI.Label(new Rect(26f, 92f, 270f, 24f), "LAP " + lap + " / " + race.Definition.laps);
                GUI.Label(new Rect(26f, 119f, 270f, 24f), "NEXT CHECKPOINT " + nextGate + " / " + race.Definition.checkpointCount);
                GUI.Label(new Rect(320f, 92f, 240f, 24f), "TIME " + race.ElapsedSeconds.ToString("0.000") + " SEC");
            }

            string controls = Application.isMobilePlatform
                ? "TOUCH TO DRIVE  |  CAMERA BUTTON TO SWITCH VIEW"
                : "WASD / ARROWS DRIVE   |   SPACE BRAKE   |   LEFT SHIFT HANDBRAKE   |   V CAMERA";
            GUI.Label(new Rect(20f, Screen.height - 34f, 660f, 24f), controls);
            if (GUI.Button(new Rect(Screen.width - 150f, 18f, 128f, 48f), "PAUSE"))
                PauseGame();
        }

        private void AddButton(float x, ref float y, float width, string label, System.Action action)
        {
            if (GUI.Button(new Rect(x, y, width, 48f), label))
                action?.Invoke();
            y += 58f;
        }

        private void DrawMessage(float x, ref float y, float width)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label) { wordWrap = true };
            style.normal.textColor = new Color(0.65f, 0.75f, 0.85f);
            GUI.Label(new Rect(x, y, width, 42f), message, style);
            y += 48f;
        }

        private void DrawFooter(float x, float width, ref float y)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            style.normal.textColor = new Color(0.4f, 0.5f, 0.6f);
            GUI.Label(new Rect(x, y, width, 26f), "No account or internet connection required.", style);
        }

        private void StartFreeDrive()
        {
            if (raceCourse != null) raceCourse.StopRace();
            state = ScreenState.FreeDrive;
            Time.timeScale = 1f;
            SetGameplayEnabled(true);
            message = "Free drive started.";
        }

        private void StartCareerRace()
        {
            if (raceCourse == null || raceCourse.DefaultEvent == null)
            {
                message = "The offline race course is not available in this scene.";
                return;
            }

            Time.timeScale = 1f;
            SetGameplayEnabled(true);
            if (!raceCourse.BeginRace(raceCourse.DefaultEvent))
            {
                Time.timeScale = 0f;
                SetGameplayEnabled(false);
                message = "Could not start the local race. Check the course setup.";
                return;
            }

            playerParticipant = playerRoot.GetComponent<OfflineRaceParticipant>();
            state = ScreenState.Racing;
            message = "Race started. Complete every checkpoint in order.";
        }

        private void PauseGame()
        {
            if (state != ScreenState.FreeDrive && state != ScreenState.Racing) return;
            pausedFrom = state;
            state = ScreenState.Paused;
            Time.timeScale = 0f;
            SetGameplayEnabled(false);
        }

        private void ResumeGame()
        {
            state = pausedFrom;
            Time.timeScale = 1f;
            SetGameplayEnabled(true);
        }

        private void ReturnToMainMenu()
        {
            if (raceCourse != null) raceCourse.StopRace();
            state = ScreenState.MainMenu;
            Time.timeScale = 0f;
            SetGameplayEnabled(false);
        }

        private void SetGameplayEnabled(bool enabled)
        {
            if (!enabled && vehicle != null)
                vehicle.SetInput(new VehicleInput());
            if (vehicle != null) vehicle.enabled = enabled;
            if (keyboardInput != null) keyboardInput.enabled = enabled;
            if (hudRoot != null) hudRoot.SetActive(false);
            if (touchRoot != null) touchRoot.SetActive(enabled);
        }

        private void CycleQuality()
        {
            int count = QualitySettings.names.Length;
            if (count == 0) return;
            qualityLevel = (qualityLevel + 1) % count;
            QualitySettings.SetQualityLevel(qualityLevel, true);
            message = "Graphics quality: " + QualitySettings.names[qualityLevel];
        }

        private void PurchaseUpgrade(string upgradeId, long price)
        {
            OfflineGameManager manager = OfflineGameManager.Instance;
            if (manager == null)
            {
                message = "Local profile is unavailable.";
                return;
            }

            if (!manager.TryPurchaseUpgrade(upgradeId, price))
            {
                message = "Upgrade not purchased. Check Credits, maximum level, or local save status.";
                return;
            }

            ApplyGarageTuning();
            message = upgradeId.ToUpperInvariant() + " upgrade purchased and saved locally.";
        }

        private void ApplyGarageTuning()
        {
            OfflineGameManager manager = OfflineGameManager.Instance;
            if (vehicle == null || manager == null || manager.Profile == null) return;
            OfflinePlayerProfile profile = manager.Profile;
            vehicle.ApplyGarageUpgrades(profile.engineUpgradeLevel,
                profile.handlingUpgradeLevel, profile.brakeUpgradeLevel);
        }

        private static float panelSafeWidth(float width) => Mathf.Max(180f, width);

        private void OnDestroy()
        {
            if (raceCourse != null) raceCourse.StopRace();
            Time.timeScale = 1f;
        }
    }
}
