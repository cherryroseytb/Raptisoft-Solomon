// Projectile.cs
// 마법 발사체 동작.
// Unity 에디터:
//   - 빈 SpriteRenderer 자식 + CircleCollider2D(IsTrigger=true) + Rigidbody2D(Kinematic) 구성으로 프리팹화.
//   - 이 스크립트 부착. BaseMagicSO/ComboMagicSO의 projectilePrefab 필드에 연결.
//   - Tag "Projectile", 적은 Tag "Enemy".

using UnityEngine;
using SolomonCopy.Magic;
using SolomonCopy.Systems;
using SolomonCopy.Enemy;

namespace SolomonCopy.ProjectileSys
{
    public class Projectile : MonoBehaviour
    {
        private Vector2 _dir;
        private float _speed;
        private int _damage;
        private float _expireAt;
        private bool _pierce;
        private StatusEffect _effect;
        private float _effectDuration;

        // 콤보 옵션 (없으면 0/false)
        private bool _explodeOnHit;
        private float _aoeRadius;
        private int _chainCount;
        private float _chainRange;

        // Spawn 직후 호출. 풀에서 꺼낼 때마다 모든 상태 초기화.
        public void Initialize(Vector2 dir, float speed, float lifetime, int damage,
                                bool pierce, StatusEffect effect, float effectDuration,
                                bool explodeOnHit = false, float aoeRadius = 0f,
                                int chainCount = 0, float chainRange = 0f)
        {
            _dir = dir.normalized;
            _speed = speed;
            _damage = damage;
            _pierce = pierce;
            _effect = effect;
            _effectDuration = effectDuration;
            _expireAt = Time.time + lifetime;
            _explodeOnHit = explodeOnHit;
            _aoeRadius = aoeRadius;
            _chainCount = chainCount;
            _chainRange = chainRange;

            // 발사 방향으로 시각 회전 (스프라이트가 위를 보고 그려졌다고 가정)
            float angle = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        private void Update()
        {
            if (Time.time >= _expireAt) { Despawn(); return; }
            transform.position += (Vector3)(_dir * _speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;

            var enemy = other.GetComponent<EnemyController>();
            if (enemy == null) return;

            enemy.TakeDamage(_damage, _effect, _effectDuration);

            if (_explodeOnHit && _aoeRadius > 0f)
            {
                ProjectileEffects.Explode(transform.position, _aoeRadius, _damage / 2, _effect, _effectDuration);
            }
            if (_chainCount > 0 && _chainRange > 0f)
            {
                ProjectileEffects.Chain(enemy, _chainCount, _chainRange, _damage / 2, _effect, _effectDuration);
            }

            if (!_pierce) Despawn();
        }

        private void Despawn()
        {
            if (ObjectPooler.Instance != null) ObjectPooler.Instance.Return(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
