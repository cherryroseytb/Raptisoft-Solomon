// GameManager.cs
// 점수/웨이브/게임 상태(Playing/GameOver) 관리.
// Unity 에디터: 빈 GameObject "GameManager"에 부착. 씬에 1개만.
// UI 텍스트(점수/웨이브)와 GameOver 패널을 인스펙터에서 연결.

using UnityEngine;
using UnityEngine.UI;

namespace SolomonCopy.Systems
{
    public enum GameState { Playing, GameOver }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("UI 연결 (선택)")]
        public Text scoreText;
        public Text waveText;
        public GameObject gameOverPanel;

        public GameState State { get; private set; } = GameState.Playing;
        public int Score { get; private set; }
        public int Wave { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            UpdateUI();
        }

        public void AddScore(int delta)
        {
            Score += delta;
            UpdateUI();
        }

        public void SetWave(int wave)
        {
            Wave = wave;
            UpdateUI();
        }

        public void GameOver()
        {
            State = GameState.GameOver;
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        private void UpdateUI()
        {
            if (scoreText != null) scoreText.text = $"Score: {Score}";
            if (waveText != null) waveText.text = $"Wave: {Wave}";
        }
    }
}
