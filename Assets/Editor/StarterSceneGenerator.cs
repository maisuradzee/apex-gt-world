#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ApexGTWorld.Core;
using ApexGTWorld.VehiclePhysics;
using ApexGTWorld.CameraSystem;
using ApexGTWorld.UI;
using ApexGTWorld.MotorcyclePhysics;
using ApexGTWorld.Offline;

namespace ApexGTWorld.EditorTools
{
    public static class StarterSceneGenerator
    {
        private const string CarScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string BikeScenePath = "Assets/Scenes/MotorcycleTest.unity";

        [MenuItem("APEX GT WORLD/Setup/Generate Starter Car Scene")]
        public static void GenerateCarScene()
        {
            if (!EditorUtility.DisplayDialog("Generate starter car scene",
                "Create a simple test pad, placeholder car and mobile touch controls? Existing Bootstrap scene will be overwritten.",
                "Create", "Cancel")) return;

            EnsureFolder("Assets/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
            AddOfflineGameManager();
            CreateLighting();
            CreateTestPad("Car Test Pad");
            CreateCar();

            EditorSceneManager.SaveScene(scene, CarScenePath);
            RegisterSceneInBuildSettings(CarScenePath);
            EditorUtility.DisplayDialog("Car scene generated",
                "Bootstrap scene generated in offline mode. Use WASD / arrows in the Editor or touch controls on Android. Local profile progress saves on this device; vehicle tuning and device testing are still required.", "OK");
        }

        [MenuItem("APEX GT WORLD/Setup/Generate Motorcycle Test Scene")]
        public static void GenerateMotorcycleTestScene()
        {
            if (!EditorUtility.DisplayDialog("Generate motorcycle test scene",
                "Create a basic rigid-body motorcycle handling experiment with mobile controls? Existing MotorcycleTest scene will be overwritten.",
                "Create", "Cancel")) return;

            EnsureFolder("Assets/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
            AddOfflineGameManager();
            CreateLighting();
            CreateTestPad("Motorcycle Test Pad");

            GameObject bike = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bike.name = "Motorcycle Prototype";
            bike.transform.position = new Vector3(0f, 1.1f, 0f);
            bike.transform.localScale = new Vector3(0.55f, 0.8f, 1.8f);
            Rigidbody rb = bike.AddComponent<Rigidbody>();
            rb.mass = 230f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            MotorcyclePrototypeController bikeController = bike.AddComponent<MotorcyclePrototypeController>();

            GameObject thirdPersonRoot = new GameObject("Motorcycle Third Person Camera");
            thirdPersonRoot.AddComponent<Camera>();
            thirdPersonRoot.AddComponent<AudioListener>();
            CameraFollow follow = thirdPersonRoot.AddComponent<CameraFollow>();
            follow.SetTarget(bike.transform);
            thirdPersonRoot.transform.position = new Vector3(0f, 4f, -8f);

            GameObject firstPersonRoot = new GameObject("Motorcycle First Person Camera");
            firstPersonRoot.transform.SetParent(bike.transform, false);
            firstPersonRoot.transform.localPosition = new Vector3(0f, 0.55f, 0.65f);
            firstPersonRoot.AddComponent<Camera>();
            firstPersonRoot.AddComponent<AudioListener>();
            firstPersonRoot.SetActive(false);

            CameraModeSwitcher cameraSwitcher = bike.AddComponent<CameraModeSwitcher>();
            SerializedObject cameraSerialized = new SerializedObject(cameraSwitcher);
            cameraSerialized.FindProperty("firstPersonRoot").objectReferenceValue = firstPersonRoot;
            cameraSerialized.FindProperty("thirdPersonRoot").objectReferenceValue = thirdPersonRoot;
            cameraSerialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject hud = new GameObject("Motorcycle Prototype HUD");
            MotorcyclePrototypeHud motorcycleHud = hud.AddComponent<MotorcyclePrototypeHud>();
            motorcycleHud.Configure(bikeController);

            BuildMotorcycleMobileControls(bikeController, cameraSwitcher);
            EditorSceneManager.SaveScene(scene, BikeScenePath);
            RegisterSceneInBuildSettings(BikeScenePath);
            EditorUtility.DisplayDialog("Motorcycle scene generated",
                "MotorcycleTest scene generated in offline mode. This is an early rigid-body/lean experiment, not production motorcycle simulation.", "OK");
        }


        private static void AddOfflineGameManager()
        {
            GameObject offline = new GameObject("Offline Game Manager");
            offline.AddComponent<OfflineGameManager>();
        }

        private static void CreateCar()
        {
            GameObject car = new GameObject("Prototype Car");
            car.transform.position = new Vector3(0f, 0.6f, 0f);
            Rigidbody rb = car.AddComponent<Rigidbody>();
            rb.mass = 1250f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Car Body";
            body.transform.SetParent(car.transform, false);
            body.transform.localScale = new Vector3(1.85f, 0.55f, 4.0f);
            body.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.04f, 0.35f, 0.72f));

            WheelCollider[] wheels = new WheelCollider[4];
            Transform[] visuals = new Transform[4];
            Vector3[] positions = {
                new Vector3(-0.9f, -0.25f, 1.3f), new Vector3(0.9f, -0.25f, 1.3f),
                new Vector3(-0.9f, -0.25f, -1.3f), new Vector3(0.9f, -0.25f, -1.3f)
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject wheel = new GameObject("WheelCollider_" + i);
                wheel.transform.SetParent(car.transform, false);
                wheel.transform.localPosition = positions[i];
                WheelCollider collider = wheel.AddComponent<WheelCollider>();
                collider.radius = 0.34f;
                collider.suspensionDistance = 0.18f;
                collider.mass = 22f;
                wheels[i] = collider;

                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                visual.name = "WheelVisual_" + i;
                visual.transform.SetParent(car.transform, false);
                visual.transform.localPosition = positions[i];
                visual.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                visual.transform.localScale = new Vector3(0.27f, 0.34f, 0.27f);
                Object.DestroyImmediate(visual.GetComponent<Collider>());
                visuals[i] = visual.transform;
            }

            VehicleController controller = car.AddComponent<VehicleController>();
            controller.Configure(new[] { wheels[0], wheels[1] }, wheels,
                new[] { wheels[0], wheels[1] }, new[] { wheels[2], wheels[3] });
            EditorVehicleInput keyboard = car.AddComponent<EditorVehicleInput>();
            WheelVisualSync visualSync = car.AddComponent<WheelVisualSync>();
            visualSync.Configure(wheels, visuals);

            GameObject thirdPersonRoot = new GameObject("Third Person Camera");
            thirdPersonRoot.AddComponent<Camera>();
            thirdPersonRoot.AddComponent<AudioListener>();
            CameraFollow follow = thirdPersonRoot.AddComponent<CameraFollow>();
            follow.SetTarget(car.transform);
            thirdPersonRoot.transform.position = new Vector3(0f, 4f, -8f);

            GameObject firstPersonRoot = new GameObject("First Person Camera");
            firstPersonRoot.transform.SetParent(car.transform, false);
            firstPersonRoot.transform.localPosition = new Vector3(0f, 0.85f, 0.55f);
            firstPersonRoot.AddComponent<Camera>();
            firstPersonRoot.AddComponent<AudioListener>();
            firstPersonRoot.SetActive(false);

            CameraModeSwitcher switcher = car.AddComponent<CameraModeSwitcher>();
            SerializedObject cameraSerialized = new SerializedObject(switcher);
            cameraSerialized.FindProperty("firstPersonRoot").objectReferenceValue = firstPersonRoot;
            cameraSerialized.FindProperty("thirdPersonRoot").objectReferenceValue = thirdPersonRoot;
            cameraSerialized.ApplyModifiedPropertiesWithoutUndo();
            keyboard.Configure(controller, switcher);

            GameObject hud = new GameObject("Prototype HUD");
            PrototypeHud prototypeHud = hud.AddComponent<PrototypeHud>();
            SerializedObject hudSerialized = new SerializedObject(prototypeHud);
            hudSerialized.FindProperty("vehicle").objectReferenceValue = controller;
            hudSerialized.ApplyModifiedPropertiesWithoutUndo();

            BuildCarMobileControls(controller, switcher);
        }

        private static void BuildCarMobileControls(VehicleController vehicle, CameraModeSwitcher switcher)
        {
            GameObject canvasObject = CreateMobileCanvas("Car Mobile Controls");
            TouchVehicleInput input = canvasObject.AddComponent<TouchVehicleInput>();
            input.Configure(vehicle, switcher);
            CreateCarButton(canvasObject.transform, input, "LEFT", TouchControlAction.SteerLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(105f, 105f), new Vector2(100f, 100f));
            CreateCarButton(canvasObject.transform, input, "RIGHT", TouchControlAction.SteerRight,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(225f, 105f), new Vector2(100f, 100f));
            CreateCarButton(canvasObject.transform, input, "BRAKE", TouchControlAction.Brake,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-230f, 105f), new Vector2(112f, 100f));
            CreateCarButton(canvasObject.transform, input, "GAS", TouchControlAction.Throttle,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-105f, 105f), new Vector2(112f, 100f));
            CreateCarButton(canvasObject.transform, input, "HANDBRAKE", TouchControlAction.Handbrake,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-230f, 225f), new Vector2(112f, 76f));
            CreateCarButton(canvasObject.transform, input, "CAMERA", TouchControlAction.Camera,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-100f, -70f), new Vector2(150f, 72f));
        }

        private static void BuildMotorcycleMobileControls(MotorcyclePrototypeController bike, CameraModeSwitcher switcher)
        {
            GameObject canvasObject = CreateMobileCanvas("Motorcycle Mobile Controls");
            MotorcycleTouchInput input = canvasObject.AddComponent<MotorcycleTouchInput>();
            input.Configure(bike, switcher);
            CreateMotorcycleButton(canvasObject.transform, input, "LEAN LEFT", MotorcycleTouchAction.SteerLeft, new Vector2(0f, 0f), new Vector2(90f, 100f));
            CreateMotorcycleButton(canvasObject.transform, input, "LEAN RIGHT", MotorcycleTouchAction.SteerRight, new Vector2(0f, 0f), new Vector2(210f, 100f));
            CreateMotorcycleButton(canvasObject.transform, input, "FRONT BRAKE", MotorcycleTouchAction.FrontBrake, new Vector2(1f, 0f), new Vector2(-230f, 105f));
            CreateMotorcycleButton(canvasObject.transform, input, "REAR BRAKE", MotorcycleTouchAction.RearBrake, new Vector2(1f, 0f), new Vector2(-355f, 105f));
            CreateMotorcycleButton(canvasObject.transform, input, "THROTTLE", MotorcycleTouchAction.Throttle, new Vector2(1f, 0f), new Vector2(-105f, 105f));
            CreateMotorcycleButton(canvasObject.transform, input, "CAMERA", MotorcycleTouchAction.Camera, new Vector2(1f, 1f), new Vector2(-100f, -70f));
        }

        private static GameObject CreateMobileCanvas(string name)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            GameObject canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvasObject;
        }

        private static void CreateCarButton(Transform parent, TouchVehicleInput input, string label,
            TouchControlAction action, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            GameObject buttonObject = CreateBaseButton(parent, label, anchorMin, anchorMax, position, size);
            buttonObject.AddComponent<HoldTouchControl>().Configure(input, action);
        }

        private static void CreateMotorcycleButton(Transform parent, MotorcycleTouchInput input,
            string label, MotorcycleTouchAction action, Vector2 anchor, Vector2 position)
        {
            GameObject buttonObject = CreateBaseButton(parent, label, anchor, anchor, position, new Vector2(112f, 100f));
            buttonObject.AddComponent<HoldMotorcycleTouchControl>().Configure(input, action);
        }

        private static GameObject CreateBaseButton(Transform parent, string label,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            GameObject buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.03f, 0.07f, 0.12f, 0.68f);
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;

            GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Text text = textObject.GetComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 18;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.raycastTarget = false;
            return buttonObject;
        }

        private static void CreateLighting()
        {
            GameObject light = new GameObject("Directional Light");
            Light sun = light.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateTestPad(string name)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = name;
            ground.transform.localScale = new Vector3(12f, 1f, 12f);
            ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.16f, 0.18f, 0.2f));
            CreateBarrier(new Vector3(0f, 0.5f, 35f), new Vector3(30f, 1f, 1f));
            CreateBarrier(new Vector3(0f, 0.5f, -35f), new Vector3(30f, 1f, 1f));
            CreateBarrier(new Vector3(35f, 0.5f, 0f), new Vector3(1f, 1f, 70f));
            CreateBarrier(new Vector3(-35f, 0.5f, 0f), new Vector3(1f, 1f, 70f));
        }

        private static void CreateBarrier(Vector3 position, Vector3 scale)
        {
            GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier.name = "Test Barrier";
            barrier.transform.position = position;
            barrier.transform.localScale = scale;
            barrier.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.28f, 0.3f, 0.33f));
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader);
            material.color = color;
            return material;
        }

        private static void RegisterSceneInBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            for (int i = 0; i < scenes.Count; i++)
            {
                if (string.Equals(scenes[i].path, scenePath, System.StringComparison.Ordinal))
                {
                    scenes[i] = new EditorBuildSettingsScene(scenePath, true);
                    EditorBuildSettings.scenes = scenes.ToArray();
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
