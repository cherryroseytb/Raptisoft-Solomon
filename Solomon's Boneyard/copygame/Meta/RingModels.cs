using System;
using SolomonCopy.Magic;

namespace SolomonCopy.Meta
{
    public enum RingRarity
    {
        Magic = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
    }

    [Serializable]
    public class RingInstance
    {
        public string ringId;
        public string displayName;
        public RingRarity rarity = RingRarity.Magic;
        public float damageMulBonus;          // 0.1 = +10%
        public float cooldownReduceBonus;     // 0.1 = -10%
        public float moveSpeedMulBonus;       // 0.1 = +10%
        public int maxHpBonus;                // +N
        public BaseMagicId forceSlotAMagic = BaseMagicId.None;
        public BaseMagicId forceSlotBMagic = BaseMagicId.None;
    }
}
