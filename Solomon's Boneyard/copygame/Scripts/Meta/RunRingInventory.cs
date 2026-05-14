using System.Collections.Generic;
using UnityEngine;
using SolomonCopy.Upgrade;
using SolomonCopy.Player;
using SolomonCopy.Systems;

namespace SolomonCopy.Meta
{
    public class RunRingInventory : MonoBehaviour
    {
        [Header("런 인벤토리")]
        public int maxEquippedRings = 2;
        public List<RingInstance> inventory = new List<RingInstance>();
        public List<RingInstance> equipped = new List<RingInstance>();

        private UpgradeApplier _applier;
        private MagicCaster _caster;
        private PlayerController _controller;
        private PlayerHealth _health;

        private void Awake()
        {
            _applier = GetComponent<UpgradeApplier>();
            _caster = GetComponent<MagicCaster>();
            _controller = GetComponent<PlayerController>();
            _health = GetComponent<PlayerHealth>();
        }

        public void AddRing(RingInstance ring)
        {
            if (ring == null) return;
            inventory.Add(CloneRing(ring));
        }

        public bool EquipRingFromInventory(int index)
        {
            if (index < 0 || index >= inventory.Count) return false;
            if (equipped.Count >= maxEquippedRings) return false;
            var ring = inventory[index];
            inventory.RemoveAt(index);
            equipped.Add(ring);
            ApplyRingStats(ring);
            return true;
        }

        public bool DismantleRingFromInventory(int index)
        {
            if (index < 0 || index >= inventory.Count) return false;
            inventory.RemoveAt(index);
            ApplyRandomDismantleBuff();
            return true;
        }

        public List<RingInstance> SnapshotRingsOnDeath()
        {
            var outList = new List<RingInstance>(inventory.Count + equipped.Count);
            for (int i = 0; i < inventory.Count; i++) outList.Add(CloneRing(inventory[i]));
            for (int i = 0; i < equipped.Count; i++) outList.Add(CloneRing(equipped[i]));
            return outList;
        }

        private void ApplyRingStats(RingInstance ring)
        {
            if (ring == null) return;
            if (_applier != null)
            {
                _applier.ApplyExternalMultipliers(
                    ring.damageMulBonus,
                    ring.cooldownReduceBonus,
                    ring.moveSpeedMulBonus,
                    ring.maxHpBonus);
            }

            if (_caster != null && ring.forceSlotAMagic != Magic.BaseMagicId.None)
                _caster.SetSlotA(ring.forceSlotAMagic);
            if (_caster != null && ring.forceSlotBMagic != Magic.BaseMagicId.None)
                _caster.SetSlotB(ring.forceSlotBMagic);
        }

        private void ApplyRandomDismantleBuff()
        {
            // 단판(run) 한정 버프: 4개 중 하나를 균등 랜덤 적용.
            int roll = Random.Range(0, 4);
            switch (roll)
            {
                case 0:
                    if (_applier != null) _applier.ApplyExternalMultipliers(0.08f, 0f, 0f, 0);
                    if (TopCenterMessageFeed.Instance != null) TopCenterMessageFeed.Instance.Show("분해 효과: 데미지 증가");
                    break;
                case 1:
                    if (_applier != null) _applier.ApplyExternalMultipliers(0f, 0.08f, 0f, 0);
                    if (TopCenterMessageFeed.Instance != null) TopCenterMessageFeed.Instance.Show("분해 효과: 쿨다운 감소");
                    break;
                case 2:
                    if (_controller != null) _controller.moveSpeed *= 1.1f;
                    if (TopCenterMessageFeed.Instance != null) TopCenterMessageFeed.Instance.Show("분해 효과: 이동속도 증가");
                    break;
                case 3:
                    if (_health != null) _health.Heal(20);
                    if (TopCenterMessageFeed.Instance != null) TopCenterMessageFeed.Instance.Show("분해 효과: 체력 회복");
                    break;
            }
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
    }
}
