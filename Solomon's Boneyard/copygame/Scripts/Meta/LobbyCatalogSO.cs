using System;
using System.Collections.Generic;
using UnityEngine;

namespace SolomonCopy.Meta
{
    public enum LobbyEntryType
    {
        Service = 1, // 장착형 Perk
        Fate = 2     // 비장착형 영구 해금
    }

    [CreateAssetMenu(menuName = "SolomonCopy/Meta/LobbyCatalog", fileName = "LobbyCatalog")]
    public class LobbyCatalogSO : ScriptableObject
    {
        [Serializable]
        public class ServiceEntry
        {
            public LobbySkillEffectId id = LobbySkillEffectId.None;
            public string displayName = "";
            [TextArea] public string description = "";
            public int unlockCost = 1000;
        }

        [Serializable]
        public class FateEntry
        {
            public LobbyFeatureId id = LobbyFeatureId.None;
            public string displayName = "";
            [TextArea] public string description = "";
            public int unlockCost = 2000;
            public bool repeatablePurchase; // Reduce Tonic Cost 같은 반복 구매형
        }

        public List<ServiceEntry> services = new List<ServiceEntry>();
        public List<FateEntry> fates = new List<FateEntry>();
    }
}
