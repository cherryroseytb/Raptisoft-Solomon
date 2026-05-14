using System.Collections.Generic;
using UnityEngine;
using SolomonCopy.Player;
using SolomonCopy.Systems;

namespace SolomonCopy.Meta
{
    public class MetaProgressionManager : MonoBehaviour
    {
        public static MetaProgressionManager Instance { get; private set; }

        [Header("메타 통화")]
        public int totalGold;

        [Header("기능 슬롯")]
        public int baseFeatureSlots = 2;
        public int tempExtraFeatureSlots;
        public int maxFeatureSlots = 6;
        public int temporarySlotCost = 1000;

        [Header("무덤 반지 풀")]
        public List<RingInstance> gravePool = new List<RingInstance>();
        public List<RingInstance> carryOverSelection = new List<RingInstance>(); // 다음 판 반입(최대 2)

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            totalGold += amount;
            if (GameManager.Instance != null) GameManager.Instance.SetGold(totalGold);
        }

        public bool TryBuyTemporaryFeatureSlot()
        {
            if (totalGold < temporarySlotCost) return false;
            int current = baseFeatureSlots + tempExtraFeatureSlots;
            if (current >= maxFeatureSlots) return false;
            totalGold -= temporarySlotCost;
            tempExtraFeatureSlots++;
            if (GameManager.Instance != null) GameManager.Instance.SetGold(totalGold);
            return true;
        }

        public void ApplyRunStartState(RunRingInventory runInv, LoadoutSlotManager slotManager)
        {
            if (slotManager != null) slotManager.SetTemporaryExtraSlots(tempExtraFeatureSlots);

            if (runInv == null) return;
            for (int i = 0; i < carryOverSelection.Count; i++)
                runInv.AddRing(carryOverSelection[i]);
        }

        public void OnRunEndedByDeath(RunRingInventory runInv)
        {
            if (runInv != null)
            {
                var rings = runInv.SnapshotRingsOnDeath();
                for (int i = 0; i < rings.Count; i++) gravePool.Add(rings[i]);
            }

            // Bornyard 규칙: 판 종료(사망) 시 임시 슬롯 확장 종료
            tempExtraFeatureSlots = 0;
            carryOverSelection.Clear();
        }

        // 무덤지기 로직: 비용을 내고 무덤 풀에서 최대 2개를 다음 판으로 선택.
        public bool TrySelectCarryOverFromGrave(int indexA, int indexB, int randomCost)
        {
            if (randomCost < 0) randomCost = 0;
            if (totalGold < randomCost) return false;

            totalGold -= randomCost;
            if (GameManager.Instance != null) GameManager.Instance.SetGold(totalGold);

            carryOverSelection.Clear();
            TryAddCarry(indexA);
            if (indexB != indexA) TryAddCarry(indexB);
            return true;
        }

        private void TryAddCarry(int idx)
        {
            if (idx < 0 || idx >= gravePool.Count) return;
            if (carryOverSelection.Count >= 2) return;
            carryOverSelection.Add(CloneRing(gravePool[idx]));
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
            };
        }
    }
}
