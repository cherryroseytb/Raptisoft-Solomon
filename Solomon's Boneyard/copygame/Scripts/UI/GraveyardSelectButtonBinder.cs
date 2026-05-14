using UnityEngine;
using UnityEngine.UI;
using SolomonCopy.Meta;

namespace SolomonCopy.UI
{
    // 맵(묘지) 선택 버튼 잠금 바인딩
    public class GraveyardSelectButtonBinder : MonoBehaviour
    {
        public int mapIndex;
        public Button selectButton;
        public GameObject lockOverlay;
        public Text stateText;

        private readonly GraveyardSelectionService _service = new GraveyardSelectionService();

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            bool unlocked = _service.IsIndexUnlocked(mapIndex);
            if (selectButton != null) selectButton.interactable = unlocked;
            if (lockOverlay != null) lockOverlay.SetActive(!unlocked);
            if (stateText != null) stateText.text = unlocked ? "Unlocked" : "Locked";
        }
    }
}
