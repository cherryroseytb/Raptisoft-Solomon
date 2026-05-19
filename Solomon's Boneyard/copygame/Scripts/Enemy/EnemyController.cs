// EnemyController.cs
// 적 캐릭터의 추적/피격/상태이상/사망 처리.
// Unity 에디터:
//   - Rigidbody2D(GravityScale=0, FreezeRotation Z) + CircleCollider2D 부착.
//   - Tag "Enemy". Layer로 Player/Projectile과 충돌 가능하도록 설정.
//   - 이 스크립트의 player 필드는 비워두면 Tag "Player"로 자동 탐색.

using UnityEngine;
using SolomonCopy.Magic;
using SolomonCopy.Systems;
using SolomonCopy.Meta;

namespace SolomonCopy.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [Header("스탯")]
        public int maxHp = 30;
        public float moveSpeed = 2.5f;
        public int contactDamage = 1;

        [Header("드롭")]
        public GameObject xpOrbPrefab;  // 사망 시 스폰할 XpOrb 프리팹 (선택)
        public int xpAmount = 1;
        public GameObject goldPickupPrefab;
        [Range(0f, 1f)] public float goldDropChance = 0.45f;
        public int goldMin = 1;
        public int goldMax = 4;
        public GameObject ringPickupPrefab;
        [Range(0f, 1f)] public float ringDropChance = 0.1f;
        public GameObject fieldBonusPickupPrefab;
        [Range(0f, 1f)] public float fieldBonusDropChance = 0.03f;
        [Header("보스 보상")]
        public bool isBoss;
        public GameObject bossRewardPickupPrefab;

        [Header("참조")]
        public Transform player;

        private int _hp;
        private Rigidbody2D _rb;

        // 상태이상 만료 타임스탬프 (Time.time 기준)
        private float _slowUntil;
        private float _burnUntil;
        private float _shockUntil;
        private float _poisonUntil;
        private float _nextBurnTick;
        private float _nextPoisonTick;
        private int _burnDamagePerTick = 1;
        private int _poisonDamagePerTick = 1;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _hp = maxHp;
            _slowUntil = 0f; _burnUntil = 0f; _shockUntil = 0f; _poisonUntil = 0f;
            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
            }
            ValidateDropPrefabs();
        }

        private void FixedUpdate()
        {
            if (player == null) return;

            // 행동 정지 상태이상이면 이동 스킵
            if (Time.time < _shockUntil) return;

            float speed = moveSpeed;
            if (Time.time < _slowUntil) speed *= 0.5f;

            // 도트 데미지 처리
            if (Time.time < _burnUntil && Time.time >= _nextBurnTick)
            {
                _nextBurnTick = Time.time + 0.5f;
                ApplyDamage(_burnDamagePerTick);
            }
            if (Time.time < _poisonUntil && Time.time >= _nextPoisonTick)
            {
                _nextPoisonTick = Time.time + 1f;
                ApplyDamage(_poisonDamagePerTick);
            }

            Vector2 dir = ((Vector2)player.position - _rb.position).normalized;
            _rb.MovePosition(_rb.position + dir * speed * Time.fixedDeltaTime);
        }

        public void TakeDamage(int amount, StatusEffect effect, float duration)
        {
            ApplyDamage(amount);
            if (_hp <= 0) return;
            SoundManager.Instance?.Play(SoundId.EnemyHit);

            switch (effect)
            {
                case StatusEffect.Slow:
                    _slowUntil = Mathf.Max(_slowUntil, Time.time + duration);
                    VfxManager.Instance?.Play(VfxId.HitSlow, transform.position);
                    break;
                case StatusEffect.Burn:
                    _burnUntil = Mathf.Max(_burnUntil, Time.time + duration);
                    if (_nextBurnTick < Time.time) _nextBurnTick = Time.time + 0.5f;
                    VfxManager.Instance?.Play(VfxId.HitBurn, transform.position);
                    break;
                case StatusEffect.Shock:
                    _shockUntil = Mathf.Max(_shockUntil, Time.time + Mathf.Min(duration, 0.6f));
                    VfxManager.Instance?.Play(VfxId.HitShock, transform.position);
                    break;
                case StatusEffect.Poison:
                    _poisonUntil = Mathf.Max(_poisonUntil, Time.time + duration);
                    if (_nextPoisonTick < Time.time) _nextPoisonTick = Time.time + 1f;
                    VfxManager.Instance?.Play(VfxId.HitPoison, transform.position);
                    break;
            }
        }

        private void ApplyDamage(int amount)
        {
            _hp -= amount;
            if (_hp <= 0) Die();
        }

        private void Die()
        {
            SoundManager.Instance?.Play(SoundId.EnemyDie);
            VfxManager.Instance?.Play(isBoss ? VfxId.BossDie : VfxId.EnemyDie, transform.position);
            var gm = GameManager.Instance;
            if (gm != null) { gm.AddScore(10); gm.AddKill(1); }

            var meta = MetaProgressionManager.Instance;
            float goldChanceMul = meta != null ? meta.GetGoldDropChanceMultiplier() : 1f;
            float ringChanceMul = meta != null ? meta.GetRingDropChanceMultiplier() : 1f;
            float bonusChanceMul = meta != null ? meta.GetBonusDropChanceMultiplier() : 1f;
            float goldAmountMul = meta != null ? meta.GetGoldDropAmountMultiplier() : 1f;

            if (xpOrbPrefab != null)
            {
                var orb = SpawnPickup(xpOrbPrefab).GetComponent<Pickup.XpOrb>();
                if (orb != null)
                {
                    float xpMul = meta != null ? meta.GetXpOrbAmountMultiplier() : 1f;
                    orb.xpAmount = Mathf.Max(1, Mathf.RoundToInt(xpAmount * xpMul));
                }
            }

            if (goldPickupPrefab != null && Random.value <= Mathf.Clamp01(goldDropChance * goldChanceMul))
            {
                var g = SpawnPickup(goldPickupPrefab).GetComponent<Pickup.GoldPickup>();
                if (g != null)
                {
                    int rolled = Random.Range(goldMin, goldMax + 1);
                    g.goldAmount = Mathf.Max(1, Mathf.RoundToInt(rolled * goldAmountMul));
                }
            }

            if (ringPickupPrefab != null && meta != null && Random.value <= Mathf.Clamp01(ringDropChance * ringChanceMul))
            {
                var ring = meta.CreateRandomRing();
                if (ring != null)
                {
                    var rp = SpawnPickup(ringPickupPrefab).GetComponent<Pickup.RingPickup>();
                    if (rp != null) rp.SetRingData(ring);
                }
            }

            if (fieldBonusPickupPrefab != null && Random.value <= Mathf.Clamp01(fieldBonusDropChance * bonusChanceMul))
            {
                var bp = SpawnPickup(fieldBonusPickupPrefab).GetComponent<Pickup.FieldBonusPickup>();
                if (bp != null) bp.SetBonus((Pickup.FieldBonusPickup.FieldBonusType)Random.Range(1, 4));
            }

            if (isBoss && bossRewardPickupPrefab != null)
            {
                var br = SpawnPickup(bossRewardPickupPrefab).GetComponent<Pickup.BossRewardPickup>();
                if (br != null) br.SetReward((Pickup.BossRewardPickup.BossRewardType)Random.Range(1, 4));
            }

            if (ObjectPooler.Instance != null) ObjectPooler.Instance.Return(gameObject);
            else gameObject.SetActive(false);
        }

        private GameObject SpawnPickup(GameObject prefab)
        {
            return ObjectPooler.Instance != null
                ? ObjectPooler.Instance.Spawn(prefab, transform.position, Quaternion.identity)
                : Instantiate(prefab, transform.position, Quaternion.identity);
        }

        private float _nextContactDamageAt;

        private void OnValidate()
        {
            ValidateDropPrefabs();
        }

        private void ValidateDropPrefabs()
        {
            if (fieldBonusDropChance > 0f && fieldBonusPickupPrefab == null)
            {
                Debug.LogWarning("[EnemyController] fieldBonusDropChance > 0 인데 fieldBonusPickupPrefab이 비어있습니다.", this);
            }

            if (fieldBonusPickupPrefab != null && fieldBonusPickupPrefab.GetComponent<Pickup.FieldBonusPickup>() == null)
            {
                Debug.LogWarning("[EnemyController] fieldBonusPickupPrefab에 FieldBonusPickup 컴포넌트가 없습니다. 보너스 드롭을 비활성화합니다.", this);
                fieldBonusPickupPrefab = null;
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            // 플레이어와 접촉 중 일정 주기로 데미지 (스파이크 방지)
            if (!collision.collider.CompareTag("Player")) return;
            if (Time.time < _nextContactDamageAt) return;
            _nextContactDamageAt = Time.time + 0.5f;
            int finalDamage = contactDamage;
            if (MetaProgressionManager.Instance != null && MetaProgressionManager.Instance.IsHardcoreEnabled())
                finalDamage = Mathf.Max(1, finalDamage * 2);
            if (Player.PlayerHealth.Instance != null)
                Player.PlayerHealth.Instance.TakeDamage(finalDamage);
        }
    }
}
