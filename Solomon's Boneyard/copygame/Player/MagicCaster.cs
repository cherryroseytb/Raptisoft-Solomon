// MagicCaster.cs
// 플레이어의 마법 발사 담당. 두 슬롯(A/B)을 들고 있다가 입력 시 적절한 마법 캐스팅.
// Unity 에디터:
//   - Player GameObject에 부착.
//   - registry(MagicRegistrySO), muzzle(발사 위치 자식 Transform) 인스펙터 연결.
//   - 기본 슬롯은 인스펙터에서 BaseMagicId 선택.
// 동작:
//   - slotA == slotB → 해당 BaseMagicSO 발사
//   - slotA != slotB → MagicRegistrySO.LookupCombo → ComboMagicSO 발사
//   - 룩업 실패 → slotA 폴백

using UnityEngine;
using SolomonCopy.Magic;
using SolomonCopy.Systems;
using SolomonCopy.ProjectileSys;

namespace SolomonCopy.Player
{
    public class MagicCaster : MonoBehaviour
    {
        [Header("참조")]
        public MagicRegistrySO registry;
        public Transform muzzle;   // 발사 위치. 비우면 transform 사용

        [Header("초기 슬롯")]
        public BaseMagicId slotA = BaseMagicId.Fire;
        public BaseMagicId slotB = BaseMagicId.Fire;

        private float _cooldownUntil;
        private Upgrade.UpgradeApplier _upgrades;

        private void Awake()
        {
            _upgrades = GetComponent<Upgrade.UpgradeApplier>();
        }

        private float Mul(float v, float m) => v * m;

        public void SetSlotA(BaseMagicId id) { slotA = id; }
        public void SetSlotB(BaseMagicId id) { slotB = id; }

        // 외부(PlayerController)에서 매 프레임 호출. 쿨다운 도래 시에만 발사.
        public void TryFire(Vector2 dir)
        {
            if (registry == null || dir.sqrMagnitude < 0.0001f) return;
            if (Time.time < _cooldownUntil) return;

            if (slotA == slotB)
            {
                var bm = registry.GetBase(slotA);
                if (bm == null) return;
                FireBase(bm, dir);
            }
            else
            {
                var combo = registry.LookupCombo(slotA, slotB);
                if (combo != null) FireCombo(combo, dir);
                else
                {
                    var bm = registry.GetBase(slotA);
                    if (bm != null) FireBase(bm, dir);
                }
            }
        }

        private void FireBase(BaseMagicSO m, Vector2 dir)
        {
            if (m.projectilePrefab == null) return;
            var pos = (muzzle != null ? muzzle.position : transform.position);
            var go = ObjectPooler.Instance != null
                ? ObjectPooler.Instance.Spawn(m.projectilePrefab, pos, Quaternion.identity)
                : Instantiate(m.projectilePrefab, pos, Quaternion.identity);
            var p = go.GetComponent<Projectile>();
            float dmgMul = _upgrades != null ? _upgrades.DamageMul : 1f;
            float spdMul = _upgrades != null ? _upgrades.ProjectileSpeedMul : 1f;
            float cdMul = _upgrades != null ? _upgrades.CooldownMul : 1f;
            if (p != null)
                p.Initialize(dir, m.speed * spdMul, m.lifetime,
                             Mathf.RoundToInt(m.damage * dmgMul),
                             m.pierce, m.statusEffect, m.statusDuration);
            _cooldownUntil = Time.time + m.cooldown * cdMul;
            Debug.Log($"[MagicCaster] Fired: {m.displayName}");
        }

        private void FireCombo(ComboMagicSO c, Vector2 dir)
        {
            if (c.projectilePrefab == null) return;
            var pos = (muzzle != null ? muzzle.position : transform.position);
            var go = ObjectPooler.Instance != null
                ? ObjectPooler.Instance.Spawn(c.projectilePrefab, pos, Quaternion.identity)
                : Instantiate(c.projectilePrefab, pos, Quaternion.identity);
            var p = go.GetComponent<Projectile>();
            float dmgMul = _upgrades != null ? _upgrades.DamageMul : 1f;
            float spdMul = _upgrades != null ? _upgrades.ProjectileSpeedMul : 1f;
            float cdMul = _upgrades != null ? _upgrades.CooldownMul : 1f;
            if (p != null)
                p.Initialize(dir, c.speed * spdMul, c.lifetime,
                             Mathf.RoundToInt(c.damage * dmgMul),
                             c.pierce, c.statusEffect, c.statusDuration,
                             c.explodeOnHit, c.aoeRadius, c.chainCount, c.chainRange);
            _cooldownUntil = Time.time + c.cooldown * cdMul;
            Debug.Log($"[MagicCaster] Fired Combo: {c.displayName}");
        }
    }
}
