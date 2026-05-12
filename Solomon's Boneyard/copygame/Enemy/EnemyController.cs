// EnemyController.cs
// 적 캐릭터의 추적/피격/상태이상/사망 처리.
// Unity 에디터:
//   - Rigidbody2D(GravityScale=0, FreezeRotation Z) + CircleCollider2D 부착.
//   - Tag "Enemy". Layer로 Player/Projectile과 충돌 가능하도록 설정.
//   - 이 스크립트의 player 필드는 비워두면 Tag "Player"로 자동 탐색.

using UnityEngine;
using SolomonCopy.Magic;
using SolomonCopy.Systems;

namespace SolomonCopy.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [Header("스탯")]
        public int maxHp = 30;
        public float moveSpeed = 2.5f;
        public int contactDamage = 1;

        [Header("참조")]
        public Transform player;

        private int _hp;
        private Rigidbody2D _rb;

        // 상태이상 만료 타임스탬프 (Time.time 기준)
        private float _slowUntil;
        private float _burnUntil;
        private float _shockUntil;
        private float _nextBurnTick;
        private int _burnDamagePerTick = 1;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _hp = maxHp;
            _slowUntil = 0f; _burnUntil = 0f; _shockUntil = 0f;
            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
            }
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

            Vector2 dir = ((Vector2)player.position - _rb.position).normalized;
            _rb.MovePosition(_rb.position + dir * speed * Time.fixedDeltaTime);
        }

        public void TakeDamage(int amount, StatusEffect effect, float duration)
        {
            ApplyDamage(amount);
            if (_hp <= 0) return;

            switch (effect)
            {
                case StatusEffect.Slow:
                    _slowUntil = Mathf.Max(_slowUntil, Time.time + duration);
                    break;
                case StatusEffect.Burn:
                    _burnUntil = Mathf.Max(_burnUntil, Time.time + duration);
                    if (_nextBurnTick < Time.time) _nextBurnTick = Time.time + 0.5f;
                    break;
                case StatusEffect.Shock:
                    _shockUntil = Mathf.Max(_shockUntil, Time.time + Mathf.Min(duration, 0.6f));
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
            if (GameManager.Instance != null) GameManager.Instance.AddScore(10);
            if (ObjectPooler.Instance != null) ObjectPooler.Instance.Return(gameObject);
            else gameObject.SetActive(false);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 플레이어 접촉 데미지는 추후 PlayerHealth에서 처리 (MVP에서는 콘솔 로그만)
            if (collision.collider.CompareTag("Player"))
            {
                // TODO: PlayerHealth.TakeDamage(contactDamage);
            }
        }
    }
}
