// SceneFlow.cs
// 씬 전환 유틸. 씬 이름을 한 곳에서 관리하고 BGM 페이드를 묶어서 처리.
// Unity 에디터:
//   - Build Settings에 다음 씬들을 등록:
//     0. MainMenu_Scene
//     1. Lobby_Scene
//     2. Battle_Scene  (현재 GetStarted_Scene을 이름만 바꿔도 무방)
//   - 씬 이름이 다르면 아래 상수만 수정.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SolomonCopy.Systems
{
    public static class SceneFlow
    {
        public const string MainMenu = "MainMenu_Scene";
        public const string Lobby    = "Lobby_Scene";
        public const string Battle   = "Battle_Scene";
        public const string GameOver = "GameOver_Scene"; // 별도 씬을 만들지 않으면 사용 안 함

        public static void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SoundManager.Instance?.StopBgm();
            LoadIfExists(MainMenu);
        }

        public static void GoToLobby()
        {
            Time.timeScale = 1f;
            LoadIfExists(Lobby);
        }

        public static void GoToBattle()
        {
            Time.timeScale = 1f;
            LoadIfExists(Battle);
        }

        public static void RestartCurrent()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // 씬이 빌드 세팅에 없으면 경고만 출력. 단일 씬 프로젝트에서도 안전하게 사용.
        private static void LoadIfExists(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning($"[SceneFlow] 씬 '{sceneName}'이 Build Settings에 없습니다. 현재 씬 유지.");
            }
        }
    }
}
