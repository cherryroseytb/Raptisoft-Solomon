// MainMenuController.cs
// 타이틀 씬의 루트 컨트롤러. Start/Continue/Settings/Quit 버튼 처리.
// Unity 에디터:
//   - 타이틀 캔버스 루트에 부착.
//   - 각 Button의 OnClick에서 다음 메서드 연결: OnStartPressed/OnSettingsPressed/OnQuitPressed.
//   - settingsPanel은 비활성 상태로 인스펙터 연결.
//   - versionText에 빌드 버전 표시 (선택).

using UnityEngine;
using UnityEngine.UI;
using SolomonCopy.Systems;

namespace SolomonCopy.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI 패널")]
        public GameObject settingsPanel;
        public Text versionText;

        [Header("BGM")]
        public AudioClip mainMenuBgm;

        private void Start()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (versionText != null) versionText.text = $"v{Application.version}";

            // 메인 메뉴 BGM (SoundManager에 동적으로 지정).
            if (SoundManager.Instance != null && mainMenuBgm != null)
                SoundManager.Instance.PlayBgm(mainMenuBgm);
        }

        // 버튼: Start (또는 Continue) — 로비로 진입.
        public void OnStartPressed()
        {
            SceneFlow.GoToLobby();
        }

        // 버튼: Settings — 설정 패널 토글.
        public void OnSettingsPressed()
        {
            if (settingsPanel != null) settingsPanel.SetActive(!settingsPanel.activeSelf);
        }

        public void OnSettingsClosePressed()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        // 버튼: Quit — 에디터에서는 플레이 종료, 빌드에선 앱 종료.
        public void OnQuitPressed()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
