// UpgradeApplier.cs
// 선택된 SkillUpgradeSO를 실제 플레이어 스탯에 반영하는 단일 진입점.
// Player에 부착하고 LevelUpController가 Apply()를 호출.

using System.Collections.Generic;
using UnityEngine;
using SolomonCopy.Player;

namespace SolomonCopy.Upgrade
{
    public class UpgradeApplier : MonoBehaviour
    {
        // 어떤 업그레이드를 몇 번 가져갔는지 추적
        public Dictionary<SkillUpgradeSO, int> StackCount { get; } = new Dictionary<SkillUpgradeSO, int>();

        // 누적 배율(스냅샷용) — 다른 컴포넌트가 필요 시 참조
        public float DamageMul { get; private set; } = 1f;
        public float CooldownMul { get; private set; } = 1f;
        public float ProjectileSpeedMul { get; private set; } = 1f;

        // 업그레이드 카드 외 시스템(반지/메타 보너스 등)에서 공용으로 사용.
        public void ApplyExternalMultipliers(
            float damageMulBonus,
            float cooldownReduceBonus,
            float moveSpeedMulBonus,
            int maxHpBonus)
        {
            if (damageMulBonus > 0f) DamageMul *= (1f + damageMulBonus);
            if (cooldownReduceBonus > 0f) CooldownMul *= (1f - cooldownReduceBonus);
            if (moveSpeedMulBonus > 0f)
            {
                var pc = GetComponent<PlayerController>();
                if (pc != null) pc.moveSpeed *= (1f + moveSpeedMulBonus);
            }

            if (maxHpBonus > 0)
            {
                var ph = GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.maxHp += maxHpBonus;
                    ph.Heal(maxHpBonus);
                }
            }
        }

        public void Apply(SkillUpgradeSO u)
        {
            if (u == null) return;
            if (!StackCount.ContainsKey(u)) StackCount[u] = 0;
            StackCount[u]++;

            switch (u.kind)
            {
                case UpgradeKind.DamageMul:
                    ApplyExternalMultipliers(u.magnitude, 0f, 0f, 0);
                    break;
                case UpgradeKind.CooldownReduce:
                    ApplyExternalMultipliers(0f, u.magnitude, 0f, 0);
                    break;
                case UpgradeKind.SpeedMul:
                    ApplyExternalMultipliers(0f, 0f, u.magnitude, 0);
                    break;
                case UpgradeKind.MaxHpAdd:
                    ApplyExternalMultipliers(0f, 0f, 0f, Mathf.RoundToInt(u.magnitude));
                    break;
                case UpgradeKind.HealNow:
                    var ph2 = GetComponent<PlayerHealth>();
                    if (ph2 != null) ph2.Heal(Mathf.RoundToInt(u.magnitude));
                    break;
                case UpgradeKind.UnlockSlotA:
                    var c = GetComponent<MagicCaster>();
                    if (c != null) c.SetSlotA(u.targetMagic);
                    break;
                case UpgradeKind.UnlockSlotB:
                    var c2 = GetComponent<MagicCaster>();
                    if (c2 != null) c2.SetSlotB(u.targetMagic);
                    break;
                case UpgradeKind.ProjectileSpeed:
                    ProjectileSpeedMul *= (1f + u.magnitude);
                    break;
            }

            Debug.Log($"[Upgrade] Applied: {u.displayName} (stack {StackCount[u]})");
        }
    }
}
