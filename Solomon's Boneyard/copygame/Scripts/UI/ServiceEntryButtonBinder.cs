using UnityEngine;
using UnityEngine.UI;
using SolomonCopy.Meta;

namespace SolomonCopy.UI
{
    // Services(Perk) 한 항목 버튼 바인딩
    public class ServiceEntryButtonBinder : MonoBehaviour
    {
        public LobbyShopService shop;
        public LobbySkillEffectId serviceId = LobbySkillEffectId.None;

        [Header("UI")]
        public Text titleText;
        public Text descriptionText;
        public Text costText;
        public Text stateText;
        public Button unlockButton;
        public Button equipButton;
        public Button unequipButton;

        private void OnEnable()
        {
            MetaProgressionManager.MetaStateChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            MetaProgressionManager.MetaStateChanged -= Refresh;
        }

        public void Configure(LobbyShopService shopService, LobbyCatalogSO.ServiceEntry entry)
        {
            shop = shopService;
            if (entry != null)
            {
                serviceId = entry.id;
                if (titleText != null) titleText.text = string.IsNullOrEmpty(entry.displayName) ? serviceId.ToString() : entry.displayName;
                if (descriptionText != null) descriptionText.text = entry.description ?? "";
            }
            Refresh();
        }

        public void OnClickUnlock()
        {
            if (shop != null) shop.TryUnlockService(serviceId);
            Refresh();
        }

        public void OnClickEquip()
        {
            if (shop != null) shop.TryEquipService(serviceId);
            Refresh();
        }

        public void OnClickUnequip()
        {
            if (shop != null) shop.TryUnequipService(serviceId);
            Refresh();
        }

        public void Refresh()
        {
            if (titleText != null && string.IsNullOrEmpty(titleText.text)) titleText.text = serviceId.ToString();
            if (MetaProgressionManager.Instance == null) return;

            int cost = MetaProgressionManager.Instance.GetSkillUnlockCost(serviceId);
            bool unlocked = MetaProgressionManager.Instance.IsServiceUnlocked(serviceId);
            bool equipped = MetaProgressionManager.Instance.HasEquippedService(serviceId);
            bool canEquip = MetaProgressionManager.Instance.CanEquipService(serviceId);

            if (costText != null) costText.text = $"{cost}g";
            if (stateText != null)
            {
                stateText.text = !unlocked ? "Locked" : (equipped ? "Equipped" : "Unlocked");
            }

            if (unlockButton != null) unlockButton.interactable = !unlocked;
            if (equipButton != null) equipButton.interactable = unlocked && !equipped && canEquip;
            if (unequipButton != null) unequipButton.interactable = equipped;
        }
    }
}
