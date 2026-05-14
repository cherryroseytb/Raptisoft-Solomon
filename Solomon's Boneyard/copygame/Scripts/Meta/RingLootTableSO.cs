using System;
using UnityEngine;
using SolomonCopy.Magic;

namespace SolomonCopy.Meta
{
    [CreateAssetMenu(menuName = "SolomonCopy/Meta/RingLootTable", fileName = "RingLootTable")]
    public class RingLootTableSO : ScriptableObject
    {
        [Serializable]
        public class RarityWeight
        {
            public RingRarity rarity = RingRarity.Magic;
            public int weight = 100;
        }

        [Header("등급 가중치")]
        public RarityWeight[] rarityWeights =
        {
            new RarityWeight { rarity = RingRarity.Magic, weight = 70 },
            new RarityWeight { rarity = RingRarity.Rare, weight = 20 },
            new RarityWeight { rarity = RingRarity.Epic, weight = 9 },
            new RarityWeight { rarity = RingRarity.Legendary, weight = 1 },
        };

        [Header("기본 옵션 범위")]
        public Vector2 damageMulRange = new Vector2(0.04f, 0.12f);
        public Vector2 cooldownReduceRange = new Vector2(0.03f, 0.1f);
        public Vector2 moveSpeedMulRange = new Vector2(0.03f, 0.1f);
        public Vector2Int maxHpRange = new Vector2Int(5, 25);

        [Header("등급별 스케일")]
        public float rareScale = 1.3f;
        public float epicScale = 1.7f;
        public float legendaryScale = 2.2f;

        [Header("슬롯 강제 변경 부여 확률")]
        [Range(0f, 1f)] public float forceSlotMagicChance = 0.2f;

        [Header("이름 조합")]
        public string[] prefixes = { "Ancient", "Burning", "Frozen", "Storm", "Grave", "Cursed" };
        public string[] cores = { "Band", "Ring", "Seal", "Loop" };

        public RingRarity RollRarity()
        {
            if (rarityWeights == null || rarityWeights.Length == 0) return RingRarity.Magic;
            int total = 0;
            for (int i = 0; i < rarityWeights.Length; i++) total += Mathf.Max(0, rarityWeights[i].weight);
            if (total <= 0) return RingRarity.Magic;

            int roll = UnityEngine.Random.Range(1, total + 1);
            int acc = 0;
            for (int i = 0; i < rarityWeights.Length; i++)
            {
                acc += Mathf.Max(0, rarityWeights[i].weight);
                if (roll <= acc) return rarityWeights[i].rarity;
            }
            return RingRarity.Magic;
        }

        public RingInstance GenerateRing()
        {
            var rarity = RollRarity();
            float scale = GetScale(rarity);
            var ring = new RingInstance();
            ring.ringId = Guid.NewGuid().ToString("N");
            ring.rarity = rarity;
            ring.displayName = BuildName(rarity);

            int statCount = RollStatCountByRarity(rarity);
            for (int i = 0; i < statCount; i++) ApplyOneRandomStat(ring, scale);

            if (UnityEngine.Random.value <= forceSlotMagicChance)
            {
                ring.forceSlotAMagic = RollBaseMagic();
            }
            if (UnityEngine.Random.value <= forceSlotMagicChance * 0.6f)
            {
                ring.forceSlotBMagic = RollBaseMagic();
            }
            return ring;
        }

        private string BuildName(RingRarity rarity)
        {
            string p = prefixes != null && prefixes.Length > 0 ? prefixes[UnityEngine.Random.Range(0, prefixes.Length)] : "Mystic";
            string c = cores != null && cores.Length > 0 ? cores[UnityEngine.Random.Range(0, cores.Length)] : "Ring";
            return $"{p} {c} [{rarity}]";
        }

        private float GetScale(RingRarity rarity)
        {
            switch (rarity)
            {
                case RingRarity.Rare: return rareScale;
                case RingRarity.Epic: return epicScale;
                case RingRarity.Legendary: return legendaryScale;
                default: return 1f;
            }
        }

        private int RollStatCountByRarity(RingRarity rarity)
        {
            // 관측 기반 가정:
            // - Magic: 1~2
            // - Rare: 1~2
            // - Epic: 2 또는 4
            // - Legendary(가정): 3~4
            switch (rarity)
            {
                case RingRarity.Magic:
                case RingRarity.Rare:
                    return UnityEngine.Random.value < 0.6f ? 1 : 2;
                case RingRarity.Epic:
                    return UnityEngine.Random.value < 0.7f ? 2 : 4;
                case RingRarity.Legendary:
                    return UnityEngine.Random.value < 0.5f ? 3 : 4;
                default:
                    return 1;
            }
        }

        private void ApplyOneRandomStat(RingInstance ring, float scale)
        {
            int roll = UnityEngine.Random.Range(0, 4);
            switch (roll)
            {
                case 0:
                    ring.damageMulBonus += UnityEngine.Random.Range(damageMulRange.x, damageMulRange.y) * scale;
                    break;
                case 1:
                    ring.cooldownReduceBonus += UnityEngine.Random.Range(cooldownReduceRange.x, cooldownReduceRange.y) * scale;
                    break;
                case 2:
                    ring.moveSpeedMulBonus += UnityEngine.Random.Range(moveSpeedMulRange.x, moveSpeedMulRange.y) * scale;
                    break;
                case 3:
                    ring.maxHpBonus += Mathf.RoundToInt(UnityEngine.Random.Range(maxHpRange.x, maxHpRange.y + 1) * scale);
                    break;
            }
        }

        private BaseMagicId RollBaseMagic()
        {
            int roll = UnityEngine.Random.Range(1, 5);
            return (BaseMagicId)roll;
        }
    }
}
