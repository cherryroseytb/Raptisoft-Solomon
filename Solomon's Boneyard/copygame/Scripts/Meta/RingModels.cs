using System;
using SolomonCopy.Magic;
using UnityEngine;

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

        [Header("무덤 반복 반입 리스크")]
        public int carryOverUseCount;         // 무덤에서 골라 다음 판에 반입한 누적 횟수
        public bool isShaking;                // 떨림 상태 여부(파손 위험 경고)
        public bool brokeAfterLastRun;        // 마지막 판 사용 후 파손되어 풀에서 제거되었는지
    }
}
