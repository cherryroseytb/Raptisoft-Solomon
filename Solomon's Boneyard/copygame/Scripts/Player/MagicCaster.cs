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
using SolomonCopy.Meta;

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
        private RunCombatBuffs _runBuffs;
        private int _allSkillStartBonus;
        private int _masterOffensePrimaryBonus;

        [Header("시작 스킬 보너스 환산식")]
        [Range(0f, 1f)] public float startBonusDamagePerLevel = 0.12f;
        [Range(0f, 1f)] public float startBonusCooldownReducePerLevel = 0.06f;
        [Range(0.1f, 1f)] public float minCooldownMultiplier = 0.4f;

        private void Awake()
        {
            _upgrades = GetComponent<Upgrade.UpgradeApplier>();
            _runBuffs = GetComponent<RunCombatBuffs>();
        }

        private float Mul(float v, float m) => v * m;

        private void GetMultipliers(int startBonusLevel, out float dmgMul, out float spdMul, out float cdMul)
        {
            dmgMul = _upgrades != null ? _upgrades.DamageMul : 1f;
            spdMul = _upgrades != null ? _upgrades.ProjectileSpeedMul : 1f;
            cdMul  = _upgrades != null ? _upgrades.CooldownMul : 1f;
            if (MetaProgressionManager.Instance != null && MetaProgressionManager.Instance.IsHardcoreEnabled())
                dmgMul *= 2f;
            if (startBonusLevel > 0)
            {
                dmgMul *= 1f + (startBonusLevel * startBonusDamagePerLevel);
                cdMul  *= Mathf.Max(minCooldownMultiplier, 1f - (startBonusLevel * startBonusCooldownReducePerLevel));
            }
            if (_runBuffs != null) dmgMul *= _runBuffs.damageMultiplier;
        }

        public void SetSlotA(BaseMagicId id) { slotA = id; }
        public void SetSlotB(BaseMagicId id) { slotB = id; }
        public void ConfigureStartSkillBonuses(int allSkillBonus, int masterOffensePrimaryBonus)
        {
            _allSkillStartBonus = Mathf.Max(0, allSkillBonus);
            _masterOffensePrimaryBonus = Mathf.Max(0, masterOffensePrimaryBonus);
        }

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
            var pos = muzzle != null ? muzzle.position : transform.position;
            var go = ObjectPooler.Instance != null
                ? ObjectPooler.Instance.Spawn(m.projectilePrefab, pos, Quaternion.identity)
                : Instantiate(m.projectilePrefab, pos, Quaternion.identity);
            var p = go.GetComponent<Projectile>();
            GetMultipliers(GetStartBonusLevelForBase(slotA), out float dmgMul, out float spdMul, out float cdMul);
            if (p != null)
                p.Initialize(dir, m.speed * spdMul, m.lifetime,
                             Mathf.RoundToInt(m.damage * dmgMul),
                             m.pierce, m.statusEffect, m.statusDuration);
            _cooldownUntil = Time.time + m.cooldown * cdMul;
            SoundManager.Instance?.Play(SoundId.MagicFireBase);
            Debug.Log($"[MagicCaster] Fired: {m.displayName}");
        }

        private void FireCombo(ComboMagicSO c, Vector2 dir)
        {
            if (c.projectilePrefab == null) return;
            var pos = muzzle != null ? muzzle.position : transform.position;
            var go = ObjectPooler.Instance != null
                ? ObjectPooler.Instance.Spawn(c.projectilePrefab, pos, Quaternion.identity)
                : Instantiate(c.projectilePrefab, pos, Quaternion.identity);
            var p = go.GetComponent<Projectile>();
            int startBonusLevel = Mathf.RoundToInt((GetStartBonusLevelForBase(slotA) + GetStartBonusLevelForBase(slotB)) * 0.5f);
            GetMultipliers(startBonusLevel, out float dmgMul, out float spdMul, out float cdMul);
            if (p != null)
                p.Initialize(dir, c.speed * spdMul, c.lifetime,
                             Mathf.RoundToInt(c.damage * dmgMul),
                             c.pierce, c.statusEffect, c.statusDuration,
                             c.explodeOnHit, c.aoeRadius, c.chainCount, c.chainRange);
            _cooldownUntil = Time.time + c.cooldown * cdMul;
            SoundManager.Instance?.Play(SoundId.MagicFireCombo);
            Debug.Log($"[MagicCaster] Fired Combo: {c.displayName}");
        }

        private int GetStartBonusLevelForBase(BaseMagicId id)
        {
            if (id == BaseMagicId.None) return 0;
            int level = _allSkillStartBonus;
            if (id == slotA) level += _masterOffensePrimaryBonus; // slotA를 주공격으로 간주
            return Mathf.Max(0, level);
        }
    }
}
