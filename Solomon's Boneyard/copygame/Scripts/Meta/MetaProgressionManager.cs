using System.Collections.Generic;
using UnityEngine;
using SolomonCopy.Player;
using SolomonCopy.Systems;

namespace SolomonCopy.Meta
{
    public class MetaProgressionManager : MonoBehaviour
    {
        public static MetaProgressionManager Instance { get; private set; }
        public static event System.Action MetaStateChanged;

        [Header("메타 통화")]
        public int totalGold;

        [Header("반지 테이블")]
        public RingLootTableSO ringLootTable;

        [Header("영구 스킬 해금")]
        public bool unlockStartWithRandomRings;
        public int startRandomRingCount = 2;
        public int baseSkillPointBonusOnStart = 0;

        [Header("로비 스킬효과/기능 해금")]
        [System.Serializable]
        public class SkillUnlockCostEntry
        {
            public LobbySkillEffectId id = LobbySkillEffectId.None;
            public int cost = 1000;
        }

        [System.Serializable]
        public class FeatureUnlockCostEntry
        {
            public LobbyFeatureId id = LobbyFeatureId.None;
            public int cost = 2000;
        }

        public int fallbackPermanentSkillUnlockCost = 1200;
        public int fallbackPermanentFeatureUnlockCost = 1500;
        public List<SkillUnlockCostEntry> skillUnlockCosts = new List<SkillUnlockCostEntry>();
        public List<FeatureUnlockCostEntry> featureUnlockCosts = new List<FeatureUnlockCostEntry>();
        public List<LobbySkillEffectId> unlockedSkillEffects = new List<LobbySkillEffectId>();
        public List<LobbySkillEffectId> equippedSkillEffects = new List<LobbySkillEffectId>(); // 슬롯 장착
        public List<LobbyFeatureId> unlockedFeatures = new List<LobbyFeatureId>(); // 해금 즉시 상시 적용

        [Header("기능 슬롯")]
        public int baseFeatureSlots = 2;
        public int tempExtraFeatureSlots;
        public int maxFeatureSlots = 7;

        [Header("Tonic (한 판 한정 슬롯 확장)")]
        public int baseTonicCost = 1000;
        [Range(0f, 0.5f)] public float tonicDiscountPerPurchase = 0.05f; // Reduce Tonic Cost 1회당 할인율
        public int tonicDiscountPurchases;
        public int tonicMinCost = 100;

        [Header("무덤 반지 풀")]
        public List<RingInstance> gravePool = new List<RingInstance>();
        public List<RingInstance> carryOverSelection = new List<RingInstance>(); // 다음 판 반입(최대 2)

        [Header("무덤 반지 리스크")]
        public int shakingThreshold = 10;       // 이 횟수 이상이면 떨림 시작
        [Range(0f, 1f)] public float breakChanceWhenShaking = 0.2f;
        public bool explainedBreakRisk;

        [Header("무덤지기 세션")]
        public bool graveSessionOpened;
        public int graveSessionCost;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SaveSystem.Load(this);
            EnsureDefaultUnlockCosts();
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            totalGold += amount;
            if (GameManager.Instance != null) GameManager.Instance.SetGold(totalGold);
            NotifyMetaStateChanged(); SaveSystem.Save(this);
        }

        public bool TryBuyTemporaryFeatureSlot()
        {
            return TryBuyPerkTonic();
        }

        public int GetCurrentTonicCost()
        {
            float discounted = baseTonicCost * Mathf.Pow(1f - tonicDiscountPerPurchase, tonicDiscountPurchases);
            return Mathf.Max(tonicMinCost, Mathf.RoundToInt(discounted));
        }

        public bool TryBuyPerkTonic()
        {
            int tonicCost = GetCurrentTonicCost();
            if (totalGold < tonicCost) return false;
            int current = baseFeatureSlots + tempExtraFeatureSlots;
            if (current >= maxFeatureSlots) return false;
            totalGold -= tonicCost;
            tempExtraFeatureSlots++;
            if (GameManager.Instance != null) GameManager.Instance.SetGold(totalGold);
            NotifyMetaStateChanged(); SaveSystem.Save(this);
            return true;
        }

        public bool TryBuyReduceTonicCost(int cost)
        {
            int useCost = Mathf.Max(0, cost);
            if (totalGold < useCost) return false;
            totalGold -= useCost;
            tonicDiscountPurchases++;
            if (GameManager.Instance != null) GameManager.Instance.SetGold(totalGold);
            NotifyMetaStateChanged(); SaveSystem.Save(this);
            return true;
        }

        public void ApplyRunStartState(RunRingInventory runInv, LoadoutSlotManager slotManager)
        {
            if (slotManager != null) slotManager.SetTemporaryExtraSlots(tempExtraFeatureSlots);

            SyncLegacySkillFlagsFromEquipped();

            if (runInv == null) return;
            GrantRunStartRings(runInv);
            for (int i = 0; i < carryOverSelection.Count; i++)
            {
                var carry = carryOverSelection[i];
                if (carry == null || carry.brokeAfterLastRun) continue;
                runInv.AddRing(carry);
            }
        }

        public RingInstance CreateRandomRing()
        {
            return ringLootTable != null ? ringLootTable.GenerateRing() : null;
        }

        public void GrantRunStartRings(RunRingInventory runInv)
        {
            if (runInv == null) return;
            if (!unlockStartWithRandomRings) return;
            if (runInv.inventory.Count > 0 || runInv.equipped.Count > 0) return;

            int count = Mathf.Max(0, startRandomRingCount);
            for (int i = 0; i < count; i++)
            {
                var ring = CreateRandomRing();
                if (ring != null) runInv.AddRing(ring);
            }
        }

        public bool TryUnlockSkillEffect(LobbySkillEffectId id, int cost = -1)
        {
            if (id == LobbySkillEffectId.None) return false;
            if (unlockedSkillEffects.Contains(id)) return false;
            int useCost = cost >= 0 ? cost : GetSkillUnlockCost(id);
            if (totalGold < useCost) return false;

            totalGold -= useCost;
            unlockedSkillEffects.Add(id);
            if (GameManager.Instance != null) GameManager.Instance.SetGold(totalGold);
            NotifyMetaStateChanged(); SaveSystem.Save(this);
            return true;
        }

        public bool TryEquipSkillEffect(LobbySkillEffectId id)
        {
            if (id == LobbySkillEffectId.None) return false;
            if (!unlockedSkillEffects.Contains(id)) return false;
            if (equippedSkillEffects.Contains(id)) return false;
            if (equippedSkillEffects.Count >= CurrentSkillEffectSlots) return false;
            equippedSkillEffects.Add(id);
            SyncLegacySkillFlagsFromEquipped();
            NotifyMetaStateChanged(); SaveSystem.Save(this);
            return true;
        }

        public bool TryUnequipSkillEffect(LobbySkillEffectId id)
        {
            bool removed = equippedSkillEffects.Remove(id);
            if (removed) SyncLegacySkillFlagsFromEquipped();
            if (removed) NotifyMetaStateChanged(); SaveSystem.Save(this);
            return removed;
        }

        public bool TryUnlockFeature(LobbyFeatureId id, int cost = -1)
        {
            if (id == LobbyFeatureId.None) return false;
            if (unlockedFeatures.Contains(id)) return false;
            int useCost = cost >= 0 ? cost : GetFeatureUnlockCost(id);
            if (totalGold < useCost) return false;

            totalGold -= useCost;
            unlockedFeatures.Add(id);
            ApplyUnlockedFeatureEffect(id);
            if (GameManager.Instance != null) GameManager.Instance.SetGold(totalGold);
            NotifyMetaStateChanged(); SaveSystem.Save(this);
            return true;
        }

        public bool IsFeatureUnlocked(LobbyFeatureId id)
        {
            return unlockedFeatures.Contains(id);
        }

        public bool IsScavengerUnlocked()
        {
            return IsFeatureUnlocked(LobbyFeatureId.UnlockScavenger);
        }

        public bool IsBossMonstersEnabled()
        {
            return IsFeatureUnlocked(LobbyFeatureId.BossMonsters);
        }

        public int GetAvailableGraveyardCount()
        {
            // 기본 1개 + MoreGraveyards 해금 시 +3개
            return IsFeatureUnlocked(LobbyFeatureId.MoreGraveyards) ? 4 : 1;
        }

        public bool IsCharacterUnlocked(LobbyFeatureId characterFeature)
        {
            return IsFeatureUnlocked(characterFeature);
        }

        public bool IsSkillUnlockedByFate(LobbyFeatureId skillFeature)
        {
            return IsFeatureUnlocked(skillFeature);
        }

        public bool HasEquippedService(LobbySkillEffectId id)
        {
            return equippedSkillEffects.Contains(id);
        }

        public bool IsServiceUnlocked(LobbySkillEffectId id)
        {
            return unlockedSkillEffects.Contains(id);
        }

        public bool CanEquipService(LobbySkillEffectId id)
        {
            if (!IsServiceUnlocked(id)) return false;
            if (HasEquippedService(id)) return false;
            return equippedSkillEffects.Count < CurrentSkillEffectSlots;
        }

        public float GetGoldDropAmountMultiplier()
        {
            return HasEquippedService(LobbySkillEffectId.MoreGoldDrops) ? 2f : 1f;
        }

        public float GetGoldDropChanceMultiplier()
        {
            return HasEquippedService(LobbySkillEffectId.MoreGoldDrops) ? 1.25f : 1f;
        }

        public float GetRingDropChanceMultiplier()
        {
            return HasEquippedService(LobbySkillEffectId.MoreDrops) ? 1.8f : 1f;
        }

        public float GetBonusDropChanceMultiplier()
        {
            return HasEquippedService(LobbySkillEffectId.MoreDrops) ? 1.8f : 1f;
        }

        public bool IsHardcoreEnabled()
        {
            return HasEquippedService(LobbySkillEffectId.Hardcore);
        }

        public bool IsAutoPotionEnabled()
        {
            return HasEquippedService(LobbySkillEffectId.AutoPotion);
        }

        public bool IsBlazeOfGloryEnabled()
        {
            return HasEquippedService(LobbySkillEffectId.BlazeOfGlory);
        }

        public bool IsMagicScavengerEnabled()
        {
            return HasEquippedService(LobbySkillEffectId.MagicScavenger);
        }

        public bool IsCreativeCastingEnabled()
        {
            return HasEquippedService(LobbySkillEffectId.CreativeCasting);
        }

        public bool IsSecondSecondaryEnabled()
        {
            return HasEquippedService(LobbySkillEffectId.SecondSecondary);
        }

        public float GetXpOrbAmountMultiplier()
        {
            return IsMagicScavengerEnabled() ? 1.5f : 1f;
        }

        public float GetXpOrbAttractRadiusMultiplier()
        {
            return IsMagicScavengerEnabled() ? 1.5f : 1f;
        }

        public int GetLevelUpChoicesBonus()
        {
            return IsCreativeCastingEnabled() ? 1 : 0;
        }

        public float GetMasterOfOffenseDamageMultiplier()
        {
            return HasEquippedService(LobbySkillEffectId.MasterOfOffense) ? 1.25f : 1f;
        }

        public float GetMasterOfOffenseCooldownMultiplier()
        {
            return HasEquippedService(LobbySkillEffectId.MasterOfOffense) ? 0.85f : 1f;
        }

        public int CurrentSkillEffectSlots => Mathf.Clamp(baseFeatureSlots + tempExtraFeatureSlots, 0, maxFeatureSlots);

        public int GetSkillUnlockCost(LobbySkillEffectId id)
        {
            for (int i = 0; i < skillUnlockCosts.Count; i++)
            {
                if (skillUnlockCosts[i] != null && skillUnlockCosts[i].id == id)
                    return Mathf.Max(0, skillUnlockCosts[i].cost);
            }
            return Mathf.Max(0, fallbackPermanentSkillUnlockCost);
        }

        public int GetFeatureUnlockCost(LobbyFeatureId id)
        {
            for (int i = 0; i < featureUnlockCosts.Count; i++)
            {
                if (featureUnlockCosts[i] != null && featureUnlockCosts[i].id == id)
                    return Mathf.Max(0, featureUnlockCosts[i].cost);
            }
            return Mathf.Max(0, fallbackPermanentFeatureUnlockCost);
        }

        public void OnRunEndedByDeath(RunRingInventory runInv)
        {
            if (runInv != null)
            {
                var rings = runInv.SnapshotRingsOnDeath();
                for (int i = 0; i < rings.Count; i++) gravePool.Add(rings[i]);
            }

            ResolveCarryOverBreakRisk();

            // Boneyard 규칙: 판 종료(사망) 시 임시 슬롯 확장 종료
            tempExtraFeatureSlots = 0;
            carryOverSelection.Clear();
            graveSessionOpened = false;
            graveSessionCost = 0;
            NotifyMetaStateChanged(); SaveSystem.Save(this);
        }

        public bool TryEnterGraveSession(int randomCost)
        {
            if (randomCost < 0) randomCost = 0;
            if (totalGold < randomCost) return false;

            totalGold -= randomCost;
            if (GameManager.Instance != null) GameManager.Instance.SetGold(totalGold);
            graveSessionOpened = true;
            graveSessionCost = randomCost;
            NotifyMetaStateChanged(); SaveSystem.Save(this);
            return true;
        }

        // 무덤지기 입장 후에는 반지 선택 추가 비용 없음.
        public bool TrySelectCarryOverInOpenedSession(int indexA, int indexB)
        {
            if (!graveSessionOpened) return false;
            carryOverSelection.Clear();
            TryAddCarry(indexA);
            if (indexB != indexA) TryAddCarry(indexB);
            graveSessionOpened = false;
            NotifyMetaStateChanged(); SaveSystem.Save(this);
            return true;
        }

        public int RollGravekeeperCost(int minCost = 100, int maxCost = 400)
        {
            if (minCost < 0) minCost = 0;
            if (maxCost < minCost) maxCost = minCost;
            return Random.Range(minCost, maxCost + 1);
        }

        private void TryAddCarry(int idx)
        {
            if (idx < 0 || idx >= gravePool.Count) return;
            if (carryOverSelection.Count >= 2) return;

            var selected = CloneRing(gravePool[idx]);
            if (selected == null) return;

            selected.carryOverUseCount++;
            selected.isShaking = selected.carryOverUseCount >= shakingThreshold;
            selected.brokeAfterLastRun = false;
            carryOverSelection.Add(selected);
        }

        private void ResolveCarryOverBreakRisk()
        {
            for (int i = 0; i < carryOverSelection.Count; i++)
            {
                var ring = carryOverSelection[i];
                if (ring == null || !ring.isShaking) continue;
                if (Random.value <= breakChanceWhenShaking) ring.brokeAfterLastRun = true;
            }
        }

        public bool HasShakingRingInGravePool()
        {
            for (int i = 0; i < gravePool.Count; i++)
            {
                var ring = gravePool[i];
                if (ring != null && ring.isShaking) return true;
            }
            return false;
        }

        private void SyncLegacySkillFlagsFromEquipped()
        {
            // 기존 구현 호환을 위해 bool/수치 필드로 반영.
            unlockStartWithRandomRings = equippedSkillEffects.Contains(LobbySkillEffectId.TwoRingfingers)
                || equippedSkillEffects.Contains(LobbySkillEffectId.Ringfinger);
            startRandomRingCount = equippedSkillEffects.Contains(LobbySkillEffectId.TwoRingfingers) ? 2 : 1;
            baseSkillPointBonusOnStart = equippedSkillEffects.Contains(LobbySkillEffectId.Plus2ToAllSkills) ? 2 :
                (equippedSkillEffects.Contains(LobbySkillEffectId.Plus1ToAllSkills) ? 1 : 0);
        }

        private void ApplyUnlockedFeatureEffect(LobbyFeatureId id)
        {
            switch (id)
            {
                case LobbyFeatureId.FeelingPerky:
                    baseFeatureSlots = Mathf.Min(maxFeatureSlots, baseFeatureSlots + 1);
                    break;
                case LobbyFeatureId.ThePerkiest:
                    baseFeatureSlots = Mathf.Min(maxFeatureSlots, baseFeatureSlots + 1);
                    break;
            }
        }

        [ContextMenu("Reset Unlock Costs To Boneyard Defaults")]
        public void ResetUnlockCostsToBoneyardDefaults()
        {
            skillUnlockCosts.Clear();
            featureUnlockCosts.Clear();

            AddSkillCost(LobbySkillEffectId.Ringfinger, 600);
            AddSkillCost(LobbySkillEffectId.TwoRingfingers, 2000);
            AddSkillCost(LobbySkillEffectId.MoreDrops, 10000);
            AddSkillCost(LobbySkillEffectId.MoreGoldDrops, 2500);
            AddSkillCost(LobbySkillEffectId.Plus1ToAllSkills, 2500);
            AddSkillCost(LobbySkillEffectId.Plus2ToAllSkills, 5000);
            AddSkillCost(LobbySkillEffectId.MasterOfOffense, 10000);
            AddSkillCost(LobbySkillEffectId.MagicScavenger, 1000);
            AddSkillCost(LobbySkillEffectId.BattleMageStart, 1000);
            AddSkillCost(LobbySkillEffectId.TelekineticStart, 750);
            AddSkillCost(LobbySkillEffectId.SecondSecondary, 750);
            AddSkillCost(LobbySkillEffectId.CreativeCasting, 300);
            AddSkillCost(LobbySkillEffectId.AutoPotion, 5000);
            AddSkillCost(LobbySkillEffectId.FasterCasterStart, 1500);
            AddSkillCost(LobbySkillEffectId.MagicShieldInstead, 4000);
            AddSkillCost(LobbySkillEffectId.MagicShieldToo, 8000);
            AddSkillCost(LobbySkillEffectId.MagicInventory, 10000);
            AddSkillCost(LobbySkillEffectId.BlazeOfGlory, 1000);
            AddSkillCost(LobbySkillEffectId.Hardcore, 10000);

            AddFeatureCost(LobbyFeatureId.FeelingPerky, 2500);
            AddFeatureCost(LobbyFeatureId.ThePerkiest, 20000);
            AddFeatureCost(LobbyFeatureId.MoreGraveyards, 2500);
            AddFeatureCost(LobbyFeatureId.BossMonsters, 5000);
            AddFeatureCost(LobbyFeatureId.ReduceTonicCost, 2500);
        }

        private void EnsureDefaultUnlockCosts()
        {
            if (skillUnlockCosts.Count == 0 && featureUnlockCosts.Count == 0)
                ResetUnlockCostsToBoneyardDefaults();
        }

        private void AddSkillCost(LobbySkillEffectId id, int cost)
        {
            skillUnlockCosts.Add(new SkillUnlockCostEntry { id = id, cost = cost });
        }

        private void AddFeatureCost(LobbyFeatureId id, int cost)
        {
            featureUnlockCosts.Add(new FeatureUnlockCostEntry { id = id, cost = cost });
        }

        private RingInstance CloneRing(RingInstance src)
        {
            if (src == null) return null;
            return new RingInstance
            {
                ringId = src.ringId,
                displayName = src.displayName,
                rarity = src.rarity,
                damageMulBonus = src.damageMulBonus,
                cooldownReduceBonus = src.cooldownReduceBonus,
                moveSpeedMulBonus = src.moveSpeedMulBonus,
                maxHpBonus = src.maxHpBonus,
                forceSlotAMagic = src.forceSlotAMagic,
                forceSlotBMagic = src.forceSlotBMagic,
                carryOverUseCount = src.carryOverUseCount,
                isShaking = src.isShaking,
                brokeAfterLastRun = src.brokeAfterLastRun,
            };
        }

        private static void NotifyMetaStateChanged()
        {
            MetaStateChanged?.Invoke();
        }
    
        public void ReinitializeAfterLoad()
        {
            SyncLegacySkillFlagsFromEquipped();
            // 필요 시 추가적인 상시 효과 적용 로직
            foreach (var f in unlockedFeatures) ApplyUnlockedFeatureEffect(f);
            NotifyMetaStateChanged();
        }
}
}
