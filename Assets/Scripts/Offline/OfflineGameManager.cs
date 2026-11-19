using System;
using System.Collections.Generic;
using UnityEngine;

namespace ApexGTWorld.Offline
{
    /// <summary>
    /// Local single-player session and progression owner.
    /// Do not treat this data as trusted for online leaderboards or payouts.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class OfflineGameManager : MonoBehaviour
    {
        private static OfflineGameManager instance;

        [SerializeField] private bool showOfflineStatus = true;
        [SerializeField, Min(1f)] private float autosaveIntervalSeconds = 30f;

        private OfflinePlayerProfile profile;
        private float autosaveTimer;
        private string status = "Loading local profile...";

        public static OfflineGameManager Instance => instance;
        public OfflinePlayerProfile Profile => profile;
        public bool IsReady => profile != null;
        public string Status => status;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            if (OfflineSaveStore.TryLoad(out profile, out string message))
            {
                status = message ?? "Offline profile ready.";
                Debug.Log("[APEX OFFLINE] " + status);
            }
            else
            {
                status = message ?? "Offline profile unavailable.";
                Debug.LogError("[APEX OFFLINE] " + status);
            }
        }

        private void Update()
        {
            if (profile == null) return;
            autosaveTimer += Time.unscaledDeltaTime;
            if (autosaveTimer < autosaveIntervalSeconds) return;
            autosaveTimer = 0f;
            SaveNow();
        }

        public bool CompleteEvent(string eventId, long rewardCredits, long earnedXp, float distanceMeters)
        {
            if (profile == null || string.IsNullOrWhiteSpace(eventId)) return false;
            if (rewardCredits < 0L || rewardCredits > 1000000000L ||
                earnedXp < 0L || earnedXp > 1000000000L ||
                float.IsNaN(distanceMeters) || float.IsInfinity(distanceMeters) || distanceMeters < 0f)
                return false;

            var completed = new HashSet<string>(profile.completedEventIds ?? new string[0], StringComparer.Ordinal);
            if (!completed.Add(eventId.Trim())) return false;

            long previousCredits = profile.credits;
            long previousXp = profile.careerXp;
            float previousDistance = profile.totalDistanceMeters;
            string[] previousEvents = profile.completedEventIds;

            profile.completedEventIds = ToArray(completed);
            profile.credits = SafeAdd(profile.credits, rewardCredits);
            profile.careerXp = SafeAdd(profile.careerXp, earnedXp);
            double totalDistance = (double)profile.totalDistanceMeters + distanceMeters;
            profile.totalDistanceMeters = (float)Math.Min(float.MaxValue, totalDistance);
            profile.Normalize();

            if (SaveNow()) return true;
            profile.credits = previousCredits;
            profile.careerXp = previousXp;
            profile.totalDistanceMeters = previousDistance;
            profile.completedEventIds = previousEvents;
            return false;
        }

        public bool TryPurchaseVehicle(string vehicleId, long price)
        {
            if (profile == null || string.IsNullOrWhiteSpace(vehicleId) || price < 0L ||
                price > 1000000000L || OwnsVehicle(vehicleId))
                return false;
            if (profile.credits < price) return false;

            long previousCredits = profile.credits;
            string[] previousOwned = profile.ownedVehicleIds;

            profile.credits -= price;
            var owned = new HashSet<string>(profile.ownedVehicleIds ?? new string[0], StringComparer.Ordinal);
            owned.Add(vehicleId.Trim());
            profile.ownedVehicleIds = ToArray(owned);
            if (SaveNow()) return true;
            profile.credits = previousCredits;
            profile.ownedVehicleIds = previousOwned;
            return false;
        }

        public bool TrySelectVehicle(string vehicleId)
        {
            if (profile == null || string.IsNullOrWhiteSpace(vehicleId)) return false;
            vehicleId = vehicleId.Trim();
            if (!OwnsVehicle(vehicleId)) return false;
            string previousSelection = profile.selectedVehicleId;
            profile.selectedVehicleId = vehicleId;
            if (SaveNow()) return true;
            profile.selectedVehicleId = previousSelection;
            return false;
        }

        public bool TryPurchaseUpgrade(string upgradeId, long price, int maximumLevel = 5)
        {
            if (profile == null || string.IsNullOrWhiteSpace(upgradeId) ||
                price < 0L || price > 1000000000L || maximumLevel < 1 || maximumLevel > 10)
                return false;
            if (profile.credits < price) return false;

            int previousLevel;
            switch (upgradeId.Trim().ToLowerInvariant())
            {
                case "engine": previousLevel = profile.engineUpgradeLevel; break;
                case "handling": previousLevel = profile.handlingUpgradeLevel; break;
                case "brakes": previousLevel = profile.brakeUpgradeLevel; break;
                default: return false;
            }
            if (previousLevel >= maximumLevel) return false;

            long previousCredits = profile.credits;
            profile.credits -= price;
            switch (upgradeId.Trim().ToLowerInvariant())
            {
                case "engine": profile.engineUpgradeLevel++; break;
                case "handling": profile.handlingUpgradeLevel++; break;
                case "brakes": profile.brakeUpgradeLevel++; break;
            }

            if (SaveNow()) return true;
            profile.credits = previousCredits;
            switch (upgradeId.Trim().ToLowerInvariant())
            {
                case "engine": profile.engineUpgradeLevel = previousLevel; break;
                case "handling": profile.handlingUpgradeLevel = previousLevel; break;
                case "brakes": profile.brakeUpgradeLevel = previousLevel; break;
            }
            return false;
        }

        public bool UnlockRegion(string regionId)
        {
            if (profile == null || string.IsNullOrWhiteSpace(regionId)) return false;
            var regions = new HashSet<string>(profile.unlockedRegionIds ?? new string[0], StringComparer.Ordinal);
            if (!regions.Add(regionId.Trim())) return false;
            string[] previousRegions = profile.unlockedRegionIds;
            profile.unlockedRegionIds = ToArray(regions);
            if (SaveNow()) return true;
            profile.unlockedRegionIds = previousRegions;
            return false;
        }

        public bool OwnsVehicle(string vehicleId)
        {
            if (profile == null || string.IsNullOrWhiteSpace(vehicleId)) return false;
            var owned = profile.ownedVehicleIds ?? new string[0];
            for (int i = 0; i < owned.Length; i++)
                if (string.Equals(owned[i], vehicleId, StringComparison.Ordinal)) return true;
            return false;
        }

        public bool SaveNow()
        {
            if (profile == null) return false;
            if (OfflineSaveStore.TrySave(profile, out string error))
            {
                status = "Offline progress saved.";
                return true;
            }

            status = error ?? "Offline save failed.";
            Debug.LogError("[APEX OFFLINE] " + status);
            return false;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) SaveNow();
        }

        private void OnApplicationQuit() => SaveNow();

        private void OnDestroy()
        {
            if (instance != this) return;
            SaveNow();
            instance = null;
        }

        private void OnGUI()
        {
            if (!showOfflineStatus || profile == null) return;
            const float width = 260f;
            GUI.Box(new Rect(12f, Screen.height - 90f, width, 76f), "OFFLINE CAREER");
            GUI.Label(new Rect(24f, Screen.height - 62f, width - 24f, 20f),
                profile.displayName + "  |  Level " + profile.careerLevel);
            GUI.Label(new Rect(24f, Screen.height - 40f, width - 24f, 20f),
                "Credits: " + profile.credits + "  |  Local save");
        }

        private static long SafeAdd(long current, long addition)
        {
            if (addition > long.MaxValue - current) return long.MaxValue;
            return current + addition;
        }

        private static string[] ToArray(HashSet<string> values)
        {
            var result = new string[values.Count];
            values.CopyTo(result);
            Array.Sort(result, StringComparer.Ordinal);
            return result;
        }
    }
}
