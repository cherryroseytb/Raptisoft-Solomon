using UnityEngine;
using System.Collections.Generic;

namespace SolomonCopy.Meta
{
    [System.Serializable]
    public class Achievement
    {
        public string id;
        public string title;
        public string description;
        public int targetValue;
        public int currentValue;
        public bool isUnlocked;
        public int goldReward;
    }

    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }
        public List<Achievement> achievements = new List<Achievement>();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAchievements();
        }

        public void ProgressAchievement(string id, int amount)
        {
            var ach = achievements.Find(a => a.id == id);
            if (ach == null || ach.isUnlocked) return;

            ach.currentValue += amount;
            if (ach.currentValue >= ach.targetValue)
            {
                Unlock(ach);
            }
        }

        private void Unlock(Achievement ach)
        {
            ach.isUnlocked = true;
            ach.currentValue = ach.targetValue;
            if (MetaProgressionManager.Instance != null)
                MetaProgressionManager.Instance.AddGold(ach.goldReward);
            
            Debug.Log("[Achievement] Unlocked: " + ach.title);
            SaveAchievements();
        }

        private void SaveAchievements()
        {
            string json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString("Achievements", json);
        }

        private void LoadAchievements()
        {
            if (PlayerPrefs.HasKey("Achievements"))
            {
                string json = PlayerPrefs.GetString("Achievements");
                JsonUtility.FromJsonOverwrite(json, this);
            }
        }
    }
}
