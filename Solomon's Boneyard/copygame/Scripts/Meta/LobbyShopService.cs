using System.Collections.Generic;
using UnityEngine;

namespace SolomonCopy.Meta
{
    // 로비 우측 NPC(마녀) 상점 코어 로직. UI는 이 서비스만 호출.
    public class LobbyShopService : MonoBehaviour
    {
        public LobbyCatalogSO catalog;

        [ContextMenu("Fill Catalog From Meta Defaults")]
        public void FillCatalogFromMetaDefaults()
        {
            if (catalog == null || MetaProgressionManager.Instance == null) return;
            catalog.services.Clear();
            catalog.fates.Clear();

            foreach (LobbySkillEffectId id in System.Enum.GetValues(typeof(LobbySkillEffectId)))
            {
                if (id == LobbySkillEffectId.None) continue;
                catalog.services.Add(new LobbyCatalogSO.ServiceEntry
                {
                    id = id,
                    displayName = id.ToString(),
                    description = "",
                    unlockCost = MetaProgressionManager.Instance.GetSkillUnlockCost(id)
                });
            }

            foreach (LobbyFeatureId id in System.Enum.GetValues(typeof(LobbyFeatureId)))
            {
                if (id == LobbyFeatureId.None) continue;
                catalog.fates.Add(new LobbyCatalogSO.FateEntry
                {
                    id = id,
                    displayName = id.ToString(),
                    description = "",
                    unlockCost = MetaProgressionManager.Instance.GetFeatureUnlockCost(id),
                    repeatablePurchase = id == LobbyFeatureId.ReduceTonicCost
                });
            }
        }

        public List<LobbyCatalogSO.ServiceEntry> GetServices()
        {
            if (catalog == null) return new List<LobbyCatalogSO.ServiceEntry>();
            return catalog.services;
        }

        public List<LobbyCatalogSO.FateEntry> GetFates()
        {
            if (catalog == null) return new List<LobbyCatalogSO.FateEntry>();
            return catalog.fates;
        }

        public bool TryUnlockService(LobbySkillEffectId id)
        {
            if (MetaProgressionManager.Instance == null) return false;
            int cost = ResolveServiceCost(id);
            return MetaProgressionManager.Instance.TryUnlockSkillEffect(id, cost);
        }

        public bool TryEquipService(LobbySkillEffectId id)
        {
            if (MetaProgressionManager.Instance == null) return false;
            return MetaProgressionManager.Instance.TryEquipSkillEffect(id);
        }

        public bool TryUnequipService(LobbySkillEffectId id)
        {
            if (MetaProgressionManager.Instance == null) return false;
            return MetaProgressionManager.Instance.TryUnequipSkillEffect(id);
        }

        public bool TryUnlockFate(LobbyFeatureId id)
        {
            if (MetaProgressionManager.Instance == null) return false;
            int cost = ResolveFateCost(id);
            bool repeatable = IsRepeatableFate(id);

            if (repeatable && id == LobbyFeatureId.ReduceTonicCost)
            {
                return MetaProgressionManager.Instance.TryBuyReduceTonicCost(cost);
            }

            return MetaProgressionManager.Instance.TryUnlockFeature(id, cost);
        }

        private int ResolveServiceCost(LobbySkillEffectId id)
        {
            if (catalog != null)
            {
                for (int i = 0; i < catalog.services.Count; i++)
                {
                    var e = catalog.services[i];
                    if (e != null && e.id == id) return Mathf.Max(0, e.unlockCost);
                }
            }
            return MetaProgressionManager.Instance != null
                ? MetaProgressionManager.Instance.GetSkillUnlockCost(id) : 0;
        }

        private int ResolveFateCost(LobbyFeatureId id)
        {
            if (catalog != null)
            {
                for (int i = 0; i < catalog.fates.Count; i++)
                {
                    var e = catalog.fates[i];
                    if (e != null && e.id == id) return Mathf.Max(0, e.unlockCost);
                }
            }
            return MetaProgressionManager.Instance != null
                ? MetaProgressionManager.Instance.GetFeatureUnlockCost(id) : 0;
        }

        private bool IsRepeatableFate(LobbyFeatureId id)
        {
            if (catalog == null) return false;
            for (int i = 0; i < catalog.fates.Count; i++)
            {
                var e = catalog.fates[i];
                if (e != null && e.id == id) return e.repeatablePurchase;
            }
            return false;
        }
    }
}
