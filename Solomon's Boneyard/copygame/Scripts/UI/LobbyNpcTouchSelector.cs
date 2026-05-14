using UnityEngine;
using UnityEngine.EventSystems;

namespace SolomonCopy.UI
{
    // Boneyard 스타일: 캐릭터 이동 없이 로비에서 직접 터치/클릭하여 NPC UI 오픈.
    public class LobbyNpcTouchSelector : MonoBehaviour, IPointerClickHandler
    {
        [Header("팝업 UI")]
        public GameObject popupMenuRoot;

        private void Awake()
        {
            if (popupMenuRoot != null) popupMenuRoot.SetActive(false);
        }

        // 유니티 UI 시스템(EventSystem) 또는 Physics2D Raycaster를 통해 클릭/터치 감지
        public void OnPointerClick(PointerEventData eventData)
        {
            if (popupMenuRoot != null)
            {
                popupMenuRoot.SetActive(true);
            }
        }

        // 외부(닫기 버튼 등)에서 호출 가능
        public void ClosePopup()
        {
            if (popupMenuRoot != null) popupMenuRoot.SetActive(false);
        }
    }
}
