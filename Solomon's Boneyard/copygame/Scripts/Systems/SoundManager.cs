// SoundManager.cs
// SFX 풀 기반 재생 + BGM 크로스페이드.
// Unity 에디터:
//   - 빈 GO "SoundManager" 생성 후 부착. DontDestroyOnLoad 동작.
//   - library 필드에 SoundLibrarySO 연결.
//   - bgmBattle/bgmLobby/bgmGameOver에 AudioClip 연결 (선택).

using System.Collections;
using UnityEngine;

namespace SolomonCopy.Systems
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("라이브러리")]
        public SoundLibrarySO library;

        [Header("SFX 풀")]
        public int sfxPoolSize = 10;

        [Header("BGM")]
        public AudioClip bgmBattle;
        public AudioClip bgmLobby;
        public AudioClip bgmGameOver;
        [Range(0f, 3f)] public float bgmFadeDuration = 1f;

        private AudioSource[] _sfxPool;
        private int _sfxIndex;
        private AudioSource _bgmSource;
        private Coroutine _bgmFadeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitPool();
        }

        private void InitPool()
        {
            _sfxPool = new AudioSource[sfxPoolSize];
            for (int i = 0; i < sfxPoolSize; i++)
            {
                var go = new GameObject($"SFX_{i}");
                go.transform.SetParent(transform);
                _sfxPool[i] = go.AddComponent<AudioSource>();
                _sfxPool[i].playOnAwake = false;
            }

            var bgmGo = new GameObject("BGM");
            bgmGo.transform.SetParent(transform);
            _bgmSource = bgmGo.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
        }

        // ─── SFX ────────────────────────────────────────────────
        public void Play(SoundId id)
        {
            if (library == null || id == SoundId.None) return;
            var entry = library.Get(id);
            if (entry == null || entry.clips == null || entry.clips.Length == 0) return;

            var clip = entry.clips[Random.Range(0, entry.clips.Length)];
            if (clip == null) return;

            float sfxVol = SettingsManager.Instance != null
                ? SettingsManager.Instance.sfxVolume
                : 1f;

            var src = NextSfxSource();
            src.pitch = Random.Range(entry.pitchMin, entry.pitchMax);
            src.PlayOneShot(clip, entry.volume * sfxVol);
        }

        private AudioSource NextSfxSource()
        {
            // 재생 중이지 않은 소스 우선, 없으면 라운드로빈
            for (int i = 0; i < _sfxPool.Length; i++)
            {
                int idx = (_sfxIndex + i) % _sfxPool.Length;
                if (!_sfxPool[idx].isPlaying) { _sfxIndex = (idx + 1) % _sfxPool.Length; return _sfxPool[idx]; }
            }
            var result = _sfxPool[_sfxIndex];
            _sfxIndex = (_sfxIndex + 1) % _sfxPool.Length;
            return result;
        }

        // ─── BGM ────────────────────────────────────────────────
        public void PlayBgm(AudioClip clip)
        {
            if (clip == null) return;
            if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;

            if (_bgmFadeCoroutine != null) StopCoroutine(_bgmFadeCoroutine);
            _bgmFadeCoroutine = StartCoroutine(CrossFadeBgm(clip));
        }

        public void PlayBattleBgm() => PlayBgm(bgmBattle);
        public void PlayLobbyBgm()  => PlayBgm(bgmLobby);
        public void PlayGameOverBgm() => PlayBgm(bgmGameOver);

        public void StopBgm()
        {
            if (_bgmFadeCoroutine != null) StopCoroutine(_bgmFadeCoroutine);
            _bgmFadeCoroutine = StartCoroutine(FadeOutBgm());
        }

        private IEnumerator CrossFadeBgm(AudioClip next)
        {
            float bgmVol = SettingsManager.Instance != null
                ? SettingsManager.Instance.bgmVolume : 0.7f;

            // 페이드 아웃
            float elapsed = 0f;
            float startVol = _bgmSource.volume;
            while (elapsed < bgmFadeDuration * 0.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                _bgmSource.volume = Mathf.Lerp(startVol, 0f, elapsed / (bgmFadeDuration * 0.5f));
                yield return null;
            }

            _bgmSource.Stop();
            _bgmSource.clip = next;
            _bgmSource.volume = 0f;
            _bgmSource.Play();

            // 페이드 인
            elapsed = 0f;
            while (elapsed < bgmFadeDuration * 0.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                _bgmSource.volume = Mathf.Lerp(0f, bgmVol, elapsed / (bgmFadeDuration * 0.5f));
                yield return null;
            }
            _bgmSource.volume = bgmVol;
        }

        private IEnumerator FadeOutBgm()
        {
            float elapsed = 0f;
            float startVol = _bgmSource.volume;
            while (elapsed < bgmFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _bgmSource.volume = Mathf.Lerp(startVol, 0f, elapsed / bgmFadeDuration);
                yield return null;
            }
            _bgmSource.Stop();
        }

        // BGM 볼륨 실시간 반영 (SettingsManager에서 호출)
        public void RefreshBgmVolume()
        {
            if (!_bgmSource.isPlaying) return;
            _bgmSource.volume = SettingsManager.Instance != null
                ? SettingsManager.Instance.bgmVolume : 0.7f;
        }
    }
}
