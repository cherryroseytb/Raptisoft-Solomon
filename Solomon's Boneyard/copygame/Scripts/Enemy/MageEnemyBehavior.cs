using UnityEngine;
using SolomonCopy.Systems;
using SolomonCopy.Magic;

namespace SolomonCopy.Enemy
{
    // 스켈레톤 메이지: 4개 속성(얼음, 독, 번개, 불) 마법 공격
    public class MageEnemyBehavior : EnemyBehavior
    {
        public enum MageType { Ice, Poison, Lightning, Fire }
        public MageType mageType;
        public GameObject mageProjectilePrefab;
        public float attackRange = 8f;
        public float attackCooldown = 3f;

        private float _nextAttackTime;

        protected override void UpdateState()
        {
            float dist = Vector2.Distance(rb.position, player.position);
            if (dist <= attackRange)
            {
                currentState = EnemyState.Attack;
                TryAttack();
            }
            else
            {
                currentState = EnemyState.Chase;
                Vector2 dir = ((Vector2)player.position - rb.position).normalized;
                rb.MovePosition(rb.position + dir * controller.moveSpeed * Time.fixedDeltaTime);
            }
        }

        private void TryAttack()
        {
            if (Time.time < _nextAttackTime) return;
            _nextAttackTime = Time.time + attackCooldown;

            if (mageProjectilePrefab != null)
            {
                var go = ObjectPooler.Instance != null 
                    ? ObjectPooler.Instance.Spawn(mageProjectilePrefab, transform.position, Quaternion.identity)
                    : Instantiate(mageProjectilePrefab, transform.position, Quaternion.identity);
                
                var p = go.GetComponent<MageProjectile>();
                if (p != null) p.Initialize(player, mageType);
            }
        }
    }

    public class MageProjectile : MonoBehaviour
    {
        public float speed = 4f;
        public float lifetime = 5f;
        public int damage = 2;
        
        private Transform _target;
        private MageEnemyBehavior.MageType _type;
        private float _dieTime;

        public void Initialize(Transform target, MageEnemyBehavior.MageType type)
        {
            _target = target;
            _type = type;
            _dieTime = Time.time + lifetime;
        }

        private void Update()
        {
            if (Time.time > _dieTime) { Deactivate(); return; }
            if (_target == null) return;

            Vector3 dir = (_target.position - transform.position).normalized;
            
            // 유도 기능 (얼음, 독)
            if (_type == MageEnemyBehavior.MageType.Ice || _type == MageEnemyBehavior.MageType.Poison)
            {
                transform.position += dir * speed * Time.deltaTime;
            }
            else // 직선 (불, 번개는 즉발 느낌이나 코드상 투사체로 처리)
            {
                transform.position += transform.right * speed * Time.deltaTime; // 발사 시 방향 고정 가정
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                var ph = other.GetComponent<SolomonCopy.Player.PlayerHealth>();
                if (ph != null)
                {
                    StatusEffect effect = StatusEffect.None;
                    float dur = 2f;
                    switch(_type)
                    {
                        case MageEnemyBehavior.MageType.Ice: effect = StatusEffect.Slow; break;
                        case MageEnemyBehavior.MageType.Poison: effect = StatusEffect.Poison; break;
                        case MageEnemyBehavior.MageType.Lightning: effect = StatusEffect.Shock; dur = 0.3f; break;
                        case MageEnemyBehavior.MageType.Fire: effect = StatusEffect.Burn; break;
                    }
                    ph.TakeDamage(damage);
                    // 플레이어에게 상태이상 적용 로직 필요 (PlayerHealth 확장)
                }
                Deactivate();
            }
        }

        private void Deactivate()
        {
            if (ObjectPooler.Instance != null) ObjectPooler.Instance.Return(gameObject);
            else Destroy(gameObject);
        }
    }
}
