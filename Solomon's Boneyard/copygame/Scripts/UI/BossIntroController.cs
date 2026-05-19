// BossIntroController.cs
// 보스 등장 연출: 경고 UI 표시 + 화면 흔들림 + 보스 BGM 전환.
// Unity 에디터:
//   - 전투 씬의 캔버스에 부착하거나 GameManager 옆에 빈 GO로 배치.
//   - warningRoot: 경고 메시지(텍스트+배경) 루트. 평소엔 비활성.
//   - warningText: "BOSS APPROACHING" 같은 텍스트 (선택, 미설정 시 그대로 사용).
//   - shakeCamera: 흔들 카메라 (보통 Camera.main).
//   - bossBgm: 보스 등장 시 재생할 BGM 클립 (선택).
//   - WaveSpawner.SpawnBoss에서 BossIntroController.Instance?.Play()로 호출.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SolomonCopy.Systems;

namespace SolomonCopy.UI
{
    public class BossIntroController : MonoBehaviour
    {
        public static BossIntroController Instance { get; private set; }

        [Header("경고 UI")]
        public GameObject warningRoot;
        public Text warningText;
        public string warningMessage = "BOSS APPROACHING";
        public float warningDuration = 2.0f;

        [Header("카메라 흔들림")]
        public Transform shakeCamera;
        public float shakeAmplitude = 0.15f;
        public float shakeDuration = 0.6f;

        [Header("BGM")]
        public AudioClip bossBgm;

        private bool _isPlaying;

        private void Awake()
        {
            Instance = this;
            if (warningRoot != null) warningRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // WaveSpawner.SpawnBoss에서 호출.
        public void Play()
        {
            if (_isPlaying) return;
            StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            _isPlaying = true;

            if (warningText != null && !string.IsNullOrEmpty(warningMessage))
                warningText.text = warningMessage;
            if (warningRoot != null) warningRoot.SetActive(true);

            if (bossBgm != null) SoundManager.Instance?.PlayBgm(bossBgm);

            // 카메라 흔들림은 경고 표시와 병렬로.
            if (shakeCamera != null) StartCoroutine(ShakeRoutine());

            VfxManager.Instance?.Play(VfxId.BossAppear, Camera.main != null ? Camera.main.transform.position : transform.position);

            yield return new WaitForSeconds(warningDuration);

            if (warningRoot != null) warningRoot.SetActive(false);
            _isPlaying = false;
        }

        private IEnumerator ShakeRoutine()
        {
            Vector3 origin = shakeCamera.localPosition;
            float elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float damp = 1f - (elapsed / shakeDuration);
                Vector2 r = Random.insideUnitCircle * shakeAmplitude * damp;
                shakeCamera.localPosition = origin + new Vector3(r.x, r.y, 0f);
                yield return null;
            }
            shakeCamera.localPosition = origin;
        }
    }
}
