// ComboMagicSO.cs
// 두 BaseMagic의 조합으로 발현되는 마법.
// 예: Fire + Ice = 충돌 시 폭발 / Fire + Lightning = 연쇄 / Ice + Lightning = 광역 슬로우
// Unity 에디터: Create > SolomonCopy > Magic > ComboMagic.
// inputA/inputB 순서는 무관 (Registry에서 정렬 키로 룩업).

using UnityEngine;

namespace SolomonCopy.Magic
{
    [CreateAssetMenu(menuName = "SolomonCopy/Magic/ComboMagic", fileName = "ComboMagic_New")]
    public class ComboMagicSO : ScriptableObject
    {
        [Header("조합 입력")]
        public BaseMagicId inputA = BaseMagicId.None;
        public BaseMagicId inputB = BaseMagicId.None;

        [Header("결과 발사체")]
        public string displayName = "Unnamed Combo";
        public GameObject projectilePrefab;
        public float speed = 12f;
        public float lifetime = 2.5f;
        public int damage = 18;
        public bool pierce = false;
        public float cooldown = 0.5f;

        [Header("상태이상")]
        public StatusEffect statusEffect = StatusEffect.None;
        public float statusDuration = 2f;

        [Header("특수 효과")]
        public bool explodeOnHit = false;   // 충돌 시 AOE 폭발
        public float aoeRadius = 0f;
        public int chainCount = 0;          // 연쇄 횟수 (번개 계열)
        public float chainRange = 0f;
    }
}
