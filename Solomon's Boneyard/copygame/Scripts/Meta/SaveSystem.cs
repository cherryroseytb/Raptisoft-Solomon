using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace SolomonCopy.Meta
{
    // Boneyard 메타 진행도 저장 및 로드
    public static class SaveSystem
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "meta_save.json");

        [System.Serializable]
        public class SaveData
        {
            public int totalGold;
            public List<LobbySkillEffectId> unlockedSkillEffects = new List<LobbySkillEffectId>();
            public List<LobbySkillEffectId> equippedSkillEffects = new List<LobbySkillEffectId>();
            public List<LobbyFeatureId> unlockedFeatures = new List<LobbyFeatureId>();
            public int tonicDiscountPurchases;
            public List<RingInstance> gravePool = new List<RingInstance>();
        }

        public static void Save(MetaProgressionManager meta)
        {
            if (meta == null) return;

            SaveData data = new SaveData
            {
                totalGold = meta.totalGold,
                unlockedSkillEffects = new List<LobbySkillEffectId>(meta.unlockedSkillEffects),
                equippedSkillEffects = new List<LobbySkillEffectId>(meta.equippedSkillEffects),
                unlockedFeatures = new List<LobbyFeatureId>(meta.unlockedFeatures),
                tonicDiscountPurchases = meta.tonicDiscountPurchases,
                gravePool = new List<RingInstance>(meta.gravePool)
            };

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log("[SaveSystem] Meta progress saved to: " + SavePath);
        }

        public static void Load(MetaProgressionManager meta)
        {
            if (meta == null) return;
            if (!File.Exists(SavePath)) return;

            try
            {
                string json = File.ReadAllText(SavePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                meta.totalGold = data.totalGold;
                meta.unlockedSkillEffects = data.unlockedSkillEffects ?? new List<LobbySkillEffectId>();
                meta.equippedSkillEffects = data.equippedSkillEffects ?? new List<LobbySkillEffectId>();
                meta.unlockedFeatures = data.unlockedFeatures ?? new List<LobbyFeatureId>();
                meta.tonicDiscountPurchases = data.tonicDiscountPurchases;
                meta.gravePool = data.gravePool ?? new List<RingInstance>();

                meta.ReinitializeAfterLoad();

                Debug.Log("[SaveSystem] Meta progress loaded successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[SaveSystem] Failed to load save: " + e.Message);
            }
        }
    }
}
