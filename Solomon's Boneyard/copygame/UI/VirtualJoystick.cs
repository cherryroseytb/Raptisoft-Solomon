// VirtualJoystick.cs
// UGUI 기반 가상 조이스틱. 외부 라이브러리 없음.
// Unity 에디터:
//   1) Canvas(Screen Space - Overlay) 아래 Image(배경) 생성 후 이 스크립트 부착.
//   2) 자식 Image(핸들)을 만들고 handle 필드에 연결. 배경 RectTransform이 자동 입력 영역.
//   3) 좌하/우하에 2개 배치(이동용/조준용). 좌/우 멀티터치 동시 지원됨(각 EventSystem 핸들러가 독립).
// 사용:
//   VirtualJoystick js = ...; Vector2 v = js.Direction; // -1~1, 정규화 안 됨(가장자리에서 1)

using UnityEngine;
using UnityEngine.EventSystems;

namespace SolomonCopy.UI
{
    public class VirtualJoystick : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform handle;
        [SerializeField] private float handleRange = 80f;  // 배경 반경(px)
        [SerializeField] private float deadZone = 0.1f;

        public Vector2 Direction { get; private set; }   // -1~1 범위
        public bool IsPressed { get; private set; }

        private RectTransform _bgRect;

        private void Awake()
        {
            _bgRect = GetComponent<RectTransform>();
            if (handle != null) handle.anchoredPosition = Vector2.zero;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsPressed = true;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_bgRect == null) return;
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _bgRect, eventData.position, eventData.pressEventCamera, out local);

            // -1~1 정규화
            Vector2 norm = local / handleRange;
            if (norm.magnitude > 1f) norm = norm.normalized;
            if (norm.magnitude < deadZone) norm = Vector2.zero;

            Direction = norm;
            if (handle != null) handle.anchoredPosition = norm * handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsPressed = false;
            Direction = Vector2.zero;
            if (handle != null) handle.anchoredPosition = Vector2.zero;
        }
    }
}
