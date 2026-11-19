using System;
using UnityEngine;

namespace ApexGTWorld.World
{
    [Serializable]
    public sealed class RegionManifest
    {
        public RegionId id;
        public string displayName;
        public string addressableSceneKey;
        [Min(0)] public int recommendedNpcBudget = 30;
    }
}
