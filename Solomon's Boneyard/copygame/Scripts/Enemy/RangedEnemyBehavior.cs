using UnityEngine;
using SolomonCopy.Systems;

namespace SolomonCopy.Enemy
{
    public class RangedEnemyBehavior : EnemyBehavior
    {
        public float keepDistance = 5f;
        public float attackRange = 7f;
        public float attackCooldown = 2f;
        public GameObject projectilePrefab;
        public float projectileSpeed = 5f;

        private float _nextAttackTime;

        protected override void UpdateState()
        {
            float dist = Vector2.Distance(rb.position, player.position);

            if (dist > attackRange)
            {
                currentState = EnemyState.Chase;
                MoveTowardsPlayer();
            }
            else if (dist < keepDistance)
            {
                currentState = EnemyState.Flee;
                MoveAwayFromPlayer();
            }
            else
            {
                currentState = EnemyState.Attack;
                TryAttack();
            }
        }

        private void MoveTowardsPlayer()
        {
            Vector2 dir = ((Vector2)player.position - rb.position).normalized;
            rb.MovePosition(rb.position + dir * controller.moveSpeed * Time.fixedDeltaTime);
        }

        private void MoveAwayFromPlayer()
        {
            Vector2 dir = (rb.position - (Vector2)player.position).normalized;
            rb.MovePosition(rb.position + dir * controller.moveSpeed * Time.fixedDeltaTime);
        }

        private void TryAttack()
        {
            if (Time.time < _nextAttackTime) return;
            _nextAttackTime = Time.time + attackCooldown;

            if (projectilePrefab != null)
            {
                Vector2 dir = ((Vector2)player.position - rb.position).normalized;
                var go = ObjectPooler.Instance != null 
                    ? ObjectPooler.Instance.Spawn(projectilePrefab, transform.position, Quaternion.identity)
                    : Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                
                var p = go.GetComponent<SolomonCopy.ProjectileSys.Projectile>();
                if (p != null) p.Initialize(dir, projectileSpeed, 5f, controller.contactDamage, 1);
            }
        }
    }
}
