using UnityEngine;
using SolomonCopy.Systems;

namespace SolomonCopy.UI
{
    // 로비 NPC 근접 상호작용: 플레이어가 가까이 오면 팝업 메뉴 표시.
    [RequireComponent(typeof(Collider2D))]
    public class LobbyNpcInteractable : MonoBehaviour
    {
        [Header("팝업 UI")]
        public GameObject popupMenuRoot;

        [Header("입력")]
        public bool requirePressToOpen = false;
        public KeyCode openKey = KeyCode.E;

        private bool _playerInside;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
            if (popupMenuRoot != null) popupMenuRoot.SetActive(false);
        }

        private void OnEnable()
        {
            _playerInside = false;
            if (popupMenuRoot != null) popupMenuRoot.SetActive(false);
        }

        private void Update()
        {
            if (!LobbyStateMarker.InLobby)
            {
                if (popupMenuRoot != null && popupMenuRoot.activeSelf) popupMenuRoot.SetActive(false);
                return;
            }

            if (!_playerInside) return;
            if (requirePressToOpen)
            {
                if (Input.GetKeyDown(openKey)) SetPopup(true);
            }
            else
            {
                SetPopup(true);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!LobbyStateMarker.InLobby) return;
            if (!other.CompareTag("Player")) return;
            _playerInside = true;
            if (!requirePressToOpen) SetPopup(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInside = false;
            SetPopup(false);
        }

        public void ClosePopup()
        {
            SetPopup(false);
        }

        private void SetPopup(bool on)
        {
            if (popupMenuRoot == null) return;
            popupMenuRoot.SetActive(on && LobbyStateMarker.InLobby);
        }
    }
}
