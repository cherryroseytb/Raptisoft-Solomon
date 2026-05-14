using UnityEngine;

namespace SolomonCopy.UI
{
    // 무덤/인벤토리 UI에서 반지 떨림 상태를 시각화.
    public class RingShakeEffect : MonoBehaviour
    {
        public bool isShaking;
        public float amplitude = 3f;
        public float frequency = 24f;

        private Vector3 _baseLocalPos;

        private void OnEnable()
        {
            _baseLocalPos = transform.localPosition;
        }

        private void LateUpdate()
        {
            if (!isShaking)
            {
                transform.localPosition = _baseLocalPos;
                return;
            }

            float t = Time.unscaledTime * frequency;
            float x = Mathf.Sin(t) * amplitude;
            float y = Mathf.Cos(t * 0.7f) * amplitude * 0.4f;
            transform.localPosition = _baseLocalPos + new Vector3(x, y, 0f);
        }

        public void SetShaking(bool on)
        {
            isShaking = on;
            if (!on) transform.localPosition = _baseLocalPos;
        }
    }
}
