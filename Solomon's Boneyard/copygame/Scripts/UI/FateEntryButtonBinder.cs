using UnityEngine;
using UnityEngine.UI;
using SolomonCopy.Meta;

namespace SolomonCopy.UI
{
    // Fates 한 항목 버튼 바인딩
    public class FateEntryButtonBinder : MonoBehaviour
    {
        public LobbyShopService shop;
        public LobbyFeatureId fateId = LobbyFeatureId.None;
        public bool repeatable;

        [Header("UI")]
        public Text titleText;
        public Text descriptionText;
        public Text costText;
        public Text stateText;
        public Button buyButton;

        private void OnEnable()
        {
            MetaProgressionManager.MetaStateChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            MetaProgressionManager.MetaStateChanged -= Refresh;
        }

        public void Configure(LobbyShopService shopService, LobbyCatalogSO.FateEntry entry)
        {
            shop = shopService;
            if (entry != null)
            {
                fateId = entry.id;
                repeatable = entry.repeatablePurchase;
                if (titleText != null) titleText.text = string.IsNullOrEmpty(entry.displayName) ? fateId.ToString() : entry.displayName;
                if (descriptionText != null) descriptionText.text = entry.description ?? "";
            }
            Refresh();
        }

        public void OnClickBuy()
        {
            if (shop != null) shop.TryUnlockFate(fateId);
            Refresh();
        }

        public void Refresh()
        {
            if (titleText != null && string.IsNullOrEmpty(titleText.text)) titleText.text = fateId.ToString();
            if (MetaProgressionManager.Instance == null) return;

            int cost = MetaProgressionManager.Instance.GetFeatureUnlockCost(fateId);
            bool unlocked = MetaProgressionManager.Instance.IsFeatureUnlocked(fateId);

            if (costText != null) costText.text = $"{cost}g";
            if (stateText != null) stateText.text = unlocked ? "Unlocked" : "Locked";
            if (buyButton != null) buyButton.interactable = repeatable || !unlocked;
        }
    }
}
