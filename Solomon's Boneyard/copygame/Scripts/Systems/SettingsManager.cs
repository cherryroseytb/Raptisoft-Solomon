using UnityEngine;

namespace SolomonCopy.Systems
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public float bgmVolume = 0.7f;
        public float joystickSensitivity = 1f;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }

        public void SetMasterVolume(float v)
        {
            masterVolume = v;
            PlayerPrefs.SetFloat("MasterVolume", v);
            ApplyVolume();
        }

        public void SetSFXVolume(float v)
        {
            sfxVolume = v;
            PlayerPrefs.SetFloat("SFXVolume", v);
        }

        public void SetBGMVolume(float v)
        {
            bgmVolume = v;
            PlayerPrefs.SetFloat("BGMVolume", v);
            ApplyVolume();
            if (SoundManager.Instance != null) SoundManager.Instance.RefreshBgmVolume();
        }

        private void ApplyVolume()
        {
            AudioListener.volume = masterVolume;
        }

        public void LoadSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.7f);
            joystickSensitivity = PlayerPrefs.GetFloat("JoystickSensitivity", 1f);
            ApplyVolume();
        }
    }
}
