using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SolomonCopy.Systems
{
    public class TopCenterMessageFeed : MonoBehaviour
    {
        public static TopCenterMessageFeed Instance { get; private set; }

        public Text messageText;
        public float showDuration = 1.6f;
        public float goldChainTimeout = 3f;

        private Coroutine _routine;
        private int _goldAccum;
        private float _goldExpireAt;
        private bool _goldMode;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (messageText != null) messageText.gameObject.SetActive(false);
        }

        public void Show(string msg)
        {
            if (string.IsNullOrEmpty(msg) || messageText == null) return;
            _goldMode = false;
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(ShowRoutine(msg));
        }

        public void ShowGoldGain(int amount)
        {
            if (amount <= 0 || messageText == null) return;
            _goldAccum += amount;
            _goldExpireAt = Time.unscaledTime + goldChainTimeout;
            _goldMode = true;
            messageText.text = $"Gold +{_goldAccum}";
            messageText.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!_goldMode || messageText == null) return;
            messageText.text = $"Gold +{_goldAccum}";
            if (Time.unscaledTime >= _goldExpireAt)
            {
                _goldMode = false;
                _goldAccum = 0;
                messageText.gameObject.SetActive(false);
            }
        }

        private IEnumerator ShowRoutine(string msg)
        {
            messageText.text = msg;
            messageText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(showDuration);
            messageText.gameObject.SetActive(false);
            _routine = null;
        }
    }
}
