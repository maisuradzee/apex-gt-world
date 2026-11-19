using System;
using UnityEngine;

namespace ApexGTWorld.Offline
{
    /// <summary>
    /// Device-local single-player progress. It is deliberately separate from
    /// authenticated/cloud profile DTOs; local data is not an authoritative online reward.
    /// </summary>
    [Serializable]
    public sealed class OfflinePlayerProfile
    {
        public int schemaVersion = 1;
        public string playerId = "offline-player";
        public string displayName = "Driver";
        public long credits;
        public long careerXp;
        public int careerLevel = 1;
        public string selectedVehicleId = "starter_car";
        public string[] ownedVehicleIds = { "starter_car" };
        public string[] unlockedRegionIds = { "metro_city" };
        public string[] completedEventIds = new string[0];
        public float totalDistanceMeters;
        public string lastSavedUtc;

        public void Normalize()
        {
            if (schemaVersion <= 0) schemaVersion = 1;
            if (string.IsNullOrWhiteSpace(playerId)) playerId = "offline-player";
            if (string.IsNullOrWhiteSpace(displayName)) displayName = "Driver";
            displayName = displayName.Trim();
            if (displayName.Length > 24) displayName = displayName.Substring(0, 24);
            credits = Math.Max(0L, credits);
            careerXp = Math.Max(0L, careerXp);
            careerLevel = Mathf.Max(1, 1 + (int)Math.Min(int.MaxValue - 1L, careerXp / 1000L));
            if (string.IsNullOrWhiteSpace(selectedVehicleId)) selectedVehicleId = "starter_car";
            ownedVehicleIds = ownedVehicleIds ?? new string[0];
            unlockedRegionIds = unlockedRegionIds ?? new string[0];
            completedEventIds = completedEventIds ?? new string[0];
            totalDistanceMeters = Mathf.Max(0f, totalDistanceMeters);
        }
    }
}
