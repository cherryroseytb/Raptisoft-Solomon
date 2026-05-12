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

        public void Apply(SkillUpgradeSO u)
        {
            if (u == null) return;
            if (!StackCount.ContainsKey(u)) StackCount[u] = 0;
            StackCount[u]++;

            switch (u.kind)
            {
                case UpgradeKind.DamageMul:
                    DamageMul *= (1f + u.magnitude);
                    break;
                case UpgradeKind.CooldownReduce:
                    CooldownMul *= (1f - u.magnitude);
                    break;
                case UpgradeKind.SpeedMul:
                    var pc = GetComponent<PlayerController>();
                    if (pc != null) pc.moveSpeed *= (1f + u.magnitude);
                    break;
                case UpgradeKind.MaxHpAdd:
                    var ph = GetComponent<PlayerHealth>();
                    if (ph != null)
                    {
                        ph.maxHp += Mathf.RoundToInt(u.magnitude);
                        ph.Heal(Mathf.RoundToInt(u.magnitude));
                    }
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
