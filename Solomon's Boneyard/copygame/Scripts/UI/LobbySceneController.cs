// LobbySceneController.cs
// 로비 씬의 루트 컨트롤러. BGM 재생, 골드 표시 동기화, 전투 진입/메인 복귀 버튼 처리.
// Unity 에디터:
//   - 로비 씬의 빈 GO에 부착. 같은 GO에 LobbyStateMarker도 부착.
//   - goldText에 골드 표시용 Text 연결.
//   - 진입/복귀 버튼 OnClick에 OnEnterBattlePressed / OnBackToMainMenuPressed 바인딩.
//   - MetaProgressionManager가 DontDestroyOnLoad로 살아있다는 전제.

using UnityEngine;
using UnityEngine.UI;
using SolomonCopy.Meta;
using SolomonCopy.Systems;

namespace SolomonCopy.UI
{
    public class LobbySceneController : MonoBehaviour
    {
        [Header("UI 연결")]
        public Text goldText;
        public Text totalKillsText;
        public GameObject pendingGraveSessionBadge; // 무덤지기 세션 열려있을 때 표시 (선택)

        private void OnEnable()
        {
            MetaProgressionManager.MetaStateChanged += RefreshUI;
        }

        private void OnDisable()
        {
            MetaProgressionManager.MetaStateChanged -= RefreshUI;
        }

        private void Start()
        {
            SoundManager.Instance?.PlayLobbyBgm();
            RefreshUI();
        }

        private void RefreshUI()
        {
            var meta = MetaProgressionManager.Instance;
            if (goldText != null)
                goldText.text = $"Gold: {(meta != null ? meta.totalGold : 0)}";
            if (pendingGraveSessionBadge != null)
                pendingGraveSessionBadge.SetActive(meta != null && meta.graveSessionOpened);
        }

        // 버튼: 전투 시작.
        public void OnEnterBattlePressed()
        {
            SceneFlow.GoToBattle();
        }

        // 버튼: 메인 메뉴로.
        public void OnBackToMainMenuPressed()
        {
            SceneFlow.GoToMainMenu();
        }
    }
}
