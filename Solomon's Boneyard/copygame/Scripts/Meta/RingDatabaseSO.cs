using UnityEngine;
using System.Collections.Generic;

namespace SolomonCopy.Meta
{
    [CreateAssetMenu(menuName = "SolomonCopy/Meta/RingDatabase", fileName = "RingDatabase")]
    public class RingDatabaseSO : ScriptableObject
    {
        [System.Serializable]
        public class RingTemplate
        {
            public string ringId;
            public string displayName;
            public RingRarity rarity;
            
            [Header("Stats (Range)")]
            public float minDamageBonus;
            public float maxDamageBonus;
            public float minCooldownReduce;
            public float maxCooldownReduce;
            public float minMoveSpeedBonus;
            public float maxMoveSpeedBonus;
            public int minHpBonus;
            public int maxHpBonus;

            [Header("Skill Forcing")]
            public SolomonCopy.Magic.BaseMagicId forceMagic = SolomonCopy.Magic.BaseMagicId.None;
            public bool isPrimarySlot; 
        }

        public List<RingTemplate> templates = new List<RingTemplate>();

        public RingInstance GenerateInstance(string id)
        {
            var t = templates.Find(x => x.ringId == id);
            if (t == null) return null;

            return new RingInstance
            {
                ringId = t.ringId,
                displayName = t.displayName,
                rarity = t.rarity,
                damageMulBonus = Random.Range(t.minDamageBonus, t.maxDamageBonus),
                cooldownReduceBonus = Random.Range(t.minCooldownReduce, t.maxCooldownReduce),
                moveSpeedMulBonus = Random.Range(t.minMoveSpeedBonus, t.maxMoveSpeedBonus),
                maxHpBonus = Random.Range(t.minHpBonus, t.maxHpBonus + 1),
                forceSlotAMagic = t.isPrimarySlot ? t.forceMagic : SolomonCopy.Magic.BaseMagicId.None,
                forceSlotBMagic = !t.isPrimarySlot ? t.forceMagic : SolomonCopy.Magic.BaseMagicId.None
            };
        }
    }
}
