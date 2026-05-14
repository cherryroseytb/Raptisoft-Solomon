// SkillUpgradeSO.cs
// 레벨업 시 선택 가능한 업그레이드 카드 데이터.
// Unity 에디터: Create > SolomonCopy > Upgrade > SkillUpgrade.
// LevelUpController가 풀에서 N개 랜덤 뽑아 제시.

using UnityEngine;
using SolomonCopy.Magic;

namespace SolomonCopy.Upgrade
{
    public enum UpgradeKind
    {
        DamageMul,        // 데미지 % 증가
        CooldownReduce,   // 쿨다운 % 감소
        SpeedMul,         // 이동속도 % 증가
        MaxHpAdd,         // 최대 HP +N
        HealNow,          // 즉시 회복
        UnlockSlotA,      // 슬롯A 마법 변경 (targetMagic)
        UnlockSlotB,      // 슬롯B 마법 변경 (targetMagic)
        ProjectileSpeed,  // 발사체 속도 %
    }

    [CreateAssetMenu(menuName = "SolomonCopy/Upgrade/SkillUpgrade", fileName = "Upgrade_New")]
    public class SkillUpgradeSO : ScriptableObject
    {
        public string displayName = "Unnamed";
        [TextArea] public string description = "";
        public Sprite icon;

        public UpgradeKind kind = UpgradeKind.DamageMul;
        public float magnitude = 0.1f;          // 예: 0.1 = +10%
        public BaseMagicId targetMagic = BaseMagicId.None;

        [Header("등장 제약")]
        public int minPlayerLevel = 1;
        [Tooltip("같은 업그레이드를 몇 번까지 가져갈 수 있는가. 0 = 무제한")]
        public int maxStacks = 0;
    }
}
