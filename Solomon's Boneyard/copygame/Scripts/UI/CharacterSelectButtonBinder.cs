using UnityEngine;
using UnityEngine.UI;
using SolomonCopy.Meta;

namespace SolomonCopy.UI
{
    // 캐릭터 선택 버튼 잠금 바인딩
    public class CharacterSelectButtonBinder : MonoBehaviour
    {
        public CharacterSelectionService selectionService;
        public string characterId = "Sirmin";
        public Button selectButton;
        public GameObject lockOverlay;
        public Text stateText;

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (selectionService == null)
            {
                selectionService = FindObjectOfType<CharacterSelectionService>();
            }
            bool unlocked = selectionService != null && selectionService.IsUnlocked(characterId);
            if (selectButton != null) selectButton.interactable = unlocked;
            if (lockOverlay != null) lockOverlay.SetActive(!unlocked);
            if (stateText != null) stateText.text = unlocked ? "Unlocked" : "Locked";
        }
    }
}
