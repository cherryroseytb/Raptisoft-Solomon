// MagicEnums.cs
// 마법 시스템 전반에서 사용하는 enum 정의.
// - BaseMagicId: 기본 마법 종류 식별자. 새 마법 추가 시 여기에 항목만 늘리면 됨.
// - StatusEffect: 발사체가 적에게 적용하는 상태이상 종류.

namespace SolomonCopy.Magic
{
    public enum BaseMagicId
    {
        None = 0,
        Fire = 1,
        Ice = 2,
        Lightning = 3,
        Earth = 4,
    }

    public enum StatusEffect
    {
        None = 0,
        Slow = 1,   // 이동 속도 감소
        Burn = 2,   // 지속 데미지
        Shock, Poison = 3,  // 잠깐 행동 정지
    }
}
