// LevelUpController.cs
// 레벨업 시 업그레이드 카드 3장을 UI에 띄우고 사용자가 선택하면 UpgradeApplier에 위임.
// Unity 에디터:
//   - 빈 GO "LevelUpController"에 부착.
//   - pool(UpgradePool SO), applier(Player의 UpgradeApplier), choicePanel(GameObject), cardButtons(Button[3]),
//     cardTitles(Text[3]), cardDescs(Text[3]), cardIcons(Image[3]) 인스펙터 연결.
//   - PlayerExperience.levelUpController에 이 컴포넌트 연결.
//   - choicePanel은 시작 시 비활성화 상태로 두기.

using UnityEngine;
using UnityEngine.UI;

namespace SolomonCopy.Upgrade
{
    public class LevelUpController : MonoBehaviour
    {
        [Header("데이터")]
        public UpgradePool pool;
        public UpgradeApplier applier;
        public int choicesPerLevel = 3;

        [Header("UI 패널")]
        public GameObject choicePanel;
        public Button[] cardButtons;
        public Text[] cardTitles;
        public Text[] cardDescs;
        public Image[] cardIcons;

        private SkillUpgradeSO[] _currentChoices;
        private bool _wasTimeFrozen;

        private void Awake()
        {
            if (choicePanel != null) choicePanel.SetActive(false);

            if (cardButtons != null)
            {
                for (int i = 0; i < cardButtons.Length; i++)
                {
                    int idx = i; // 클로저 캡처
                    if (cardButtons[i] != null)
                        cardButtons[i].onClick.AddListener(() => OnPick(idx));
                }
            }
        }

        public void OpenChoice(int playerLevel)
        {
            if (pool == null || applier == null) return;
            var picks = pool.RandomPick(choicesPerLevel, playerLevel, applier.StackCount);
            if (picks.Count == 0) return;

            _currentChoices = picks.ToArray();
            _wasTimeFrozen = Time.timeScale == 0f;
            Time.timeScale = 0f;

            if (choicePanel != null) choicePanel.SetActive(true);
            for (int i = 0; i < cardButtons.Length; i++)
            {
                bool active = i < _currentChoices.Length;
                cardButtons[i].gameObject.SetActive(active);
                if (!active) continue;
                var u = _currentChoices[i];
                if (cardTitles != null && i < cardTitles.Length) cardTitles[i].text = u.displayName;
                if (cardDescs != null && i < cardDescs.Length) cardDescs[i].text = u.description;
                if (cardIcons != null && i < cardIcons.Length) cardIcons[i].sprite = u.icon;
            }
        }

        private void OnPick(int idx)
        {
            if (_currentChoices == null || idx < 0 || idx >= _currentChoices.Length) return;
            applier.Apply(_currentChoices[idx]);
            if (choicePanel != null) choicePanel.SetActive(false);
            if (!_wasTimeFrozen) Time.timeScale = 1f;
            _currentChoices = null;
        }
    }
}
