using UnityEngine;
using SolomonCopy.Meta;

namespace SolomonCopy.UI
{
    // 인벤토리 반지 선택 -> 중앙 모루 노출 -> 모루 클릭 시 분해.
    public class InventoryAnvilDismantleController : MonoBehaviour
    {
        [Header("참조")]
        public RunRingInventory runInventory;

        [Header("중앙 표시 전환")]
        public GameObject characterVisual;
        public GameObject anvilVisual;

        private int _selectedInventoryIndex = -1;

        private void Start()
        {
            if (runInventory == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) runInventory = player.GetComponent<RunRingInventory>();
            }
            SetAnvilMode(false);
        }

        // 인벤토리 UI 버튼 OnClick에 index를 전달해 호출.
        public void SelectInventoryRing(int inventoryIndex)
        {
            if (runInventory == null) return;
            if (inventoryIndex < 0 || inventoryIndex >= runInventory.inventory.Count) return;
            _selectedInventoryIndex = inventoryIndex;
            SetAnvilMode(true);
        }

        // 중앙 모루 클릭 OnClick에 연결.
        public void OnAnvilPressed()
        {
            if (runInventory == null || _selectedInventoryIndex < 0) return;
            bool ok = runInventory.DismantleRingFromInventory(_selectedInventoryIndex);
            _selectedInventoryIndex = -1;
            SetAnvilMode(false);

            if (!ok) return;
        }

        public void CancelSelection()
        {
            _selectedInventoryIndex = -1;
            SetAnvilMode(false);
        }

        private void SetAnvilMode(bool on)
        {
            if (characterVisual != null) characterVisual.SetActive(!on);
            if (anvilVisual != null) anvilVisual.SetActive(on);
        }
    }
}
