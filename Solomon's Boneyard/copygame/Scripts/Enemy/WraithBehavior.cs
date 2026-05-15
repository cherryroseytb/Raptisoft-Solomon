using UnityEngine;

namespace SolomonCopy.Enemy
{
    // 레이스: 벽 통과, 플레이어 통과 시 데미지, 낮은 체력
    public class WraithBehavior : EnemyBehavior
    {
        protected override void Awake()
        {
            base.Awake();
            // 벽(Environment) 레이어와 충돌 무시 설정 필요 (코드 또는 에디터)
            // Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Environment"));
        }

        protected override void UpdateState()
        {
            currentState = EnemyState.Chase;
            MoveTowardsPlayer();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                var ph = other.GetComponent<SolomonCopy.Player.PlayerHealth>();
                if (ph != null) ph.TakeDamage(controller.contactDamage);
            }
        }
    }
}
