// PlayerExperience.cs
// 경험치/레벨 관리. XP 누적 → 임계치 도달 시 레벨업 → LevelUpController에 선택지 제시 요청.
// Unity 에디터:
//   - Player에 부착.
//   - levelUpController, xpBar(Slider), levelText(Text) 인스펙터 연결.

using UnityEngine;
using UnityEngine.UI;
using SolomonCopy.Systems;

namespace SolomonCopy.Player
{
    public class PlayerExperience : MonoBehaviour
    {
        public static PlayerExperience Instance { get; private set; }

        [Header("레벨링")]
        public int level = 1;
        public int xp = 0;
        public int xpToNextBase = 5;     // 1→2 레벨에 필요한 XP
        public float xpCurvePower = 1.4f; // 다음 레벨 = base * level^power

        [Header("연결")]
        public LevelUpController levelUpController;
        public Slider xpBar;
        public Text levelText;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            UpdateUI();
        }

        public int XpNeeded =>
            Mathf.CeilToInt(xpToNextBase * Mathf.Pow(level, xpCurvePower));

        public void AddXp(int amount)
        {
            xp += amount;
            while (xp >= XpNeeded)
            {
                xp -= XpNeeded;
                level++;
                SoundManager.Instance?.Play(SoundId.LevelUp);
                VfxManager.Instance?.Play(VfxId.LevelUp, transform.position);
                if (levelUpController != null) levelUpController.OpenChoice(level);
            }
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (xpBar != null) { xpBar.maxValue = XpNeeded; xpBar.value = xp; }
            if (levelText != null) levelText.text = $"Lv. {level}";
        }
    }
}
