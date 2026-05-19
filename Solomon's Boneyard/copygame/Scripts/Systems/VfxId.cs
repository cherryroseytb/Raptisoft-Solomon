// VfxId.cs
// 파티클/이펙트 식별자. VfxLibrarySO에서 프리팹과 매핑됨.

namespace SolomonCopy.Systems
{
    public enum VfxId
    {
        None = 0,

        // 피격/사망
        EnemyHit,
        EnemyDie,
        BossDie,
        PlayerHit,
        PlayerDie,

        // 상태이상 이펙트 (피격 시 추가로 재생)
        HitBurn,
        HitSlow,
        HitShock,
        HitPoison,

        // 발사체 충격
        ImpactFire,
        ImpactIce,
        ImpactLightning,
        ImpactEarth,

        // 콤보 이펙트
        Explosion,
        ChainLightning,

        // 픽업
        XpCollect,
        GoldCollect,
        RingCollect,

        // 시스템
        LevelUp,
        WaveStart,
        BossAppear,
        AutoPotion,
        BlazeOfGlory,
    }
}
