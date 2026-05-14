using UnityEngine;
using SolomonCopy.Meta;

namespace SolomonCopy.UI
{
    // LobbyCatalogSO를 기준으로 Services/Fates 목록을 런타임에 자동 생성.
    public class LobbyCatalogListBuilder : MonoBehaviour
    {
        public LobbyShopService shop;
        public LobbyTextTableSO textTable;

        [Header("Services")]
        public Transform serviceListRoot;
        public ServiceEntryButtonBinder serviceEntryPrefab;

        [Header("Fates")]
        public Transform fateListRoot;
        public FateEntryButtonBinder fateEntryPrefab;

        public bool rebuildOnEnable = true;

        private void OnEnable()
        {
            if (rebuildOnEnable) Rebuild();
        }

        [ContextMenu("Rebuild")]
        public void Rebuild()
        {
            if (shop == null) return;
            BuildServices();
            BuildFates();
        }

        private void BuildServices()
        {
            if (serviceListRoot == null || serviceEntryPrefab == null) return;
            ClearChildren(serviceListRoot);
            var entries = shop.GetServices();
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || entry.id == LobbySkillEffectId.None) continue;
                var binder = Instantiate(serviceEntryPrefab, serviceListRoot);
                binder.Configure(shop, entry);
                if (textTable != null && textTable.TryGetServiceText(entry.id, out var dName, out var desc))
                {
                    if (binder.titleText != null && !string.IsNullOrEmpty(dName)) binder.titleText.text = dName;
                    if (binder.descriptionText != null && !string.IsNullOrEmpty(desc)) binder.descriptionText.text = desc;
                }
            }
        }

        private void BuildFates()
        {
            if (fateListRoot == null || fateEntryPrefab == null) return;
            ClearChildren(fateListRoot);
            var entries = shop.GetFates();
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || entry.id == LobbyFeatureId.None) continue;
                var binder = Instantiate(fateEntryPrefab, fateListRoot);
                binder.Configure(shop, entry);
                if (textTable != null && textTable.TryGetFateText(entry.id, out var dName, out var desc))
                {
                    if (binder.titleText != null && !string.IsNullOrEmpty(dName)) binder.titleText.text = dName;
                    if (binder.descriptionText != null && !string.IsNullOrEmpty(desc)) binder.descriptionText.text = desc;
                }
            }
        }

        private static void ClearChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
        }
    }
}
