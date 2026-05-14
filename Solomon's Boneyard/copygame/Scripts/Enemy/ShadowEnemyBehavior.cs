using UnityEngine;

namespace SolomonCopy.Enemy
{
    // 그림자 적: 이동 시 투명, 공격 시 노출, 사망 시 광역 폭발
    public class ShadowEnemyBehavior : EnemyBehavior
    {
        public SpriteRenderer spriteRenderer;
        public float attackRange = 1.5f;
        public GameObject explosionEffect;

        protected override void Awake()
        {
            base.Awake();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        protected override void UpdateState()
        {
            float dist = Vector2.Distance(rb.position, player.position);
            if (dist <= attackRange)
            {
                // 공격 순간에만 보임
                SetVisibility(true);
                currentState = EnemyState.Attack;
            }
            else
            {
                // 이동 중에는 투명
                SetVisibility(false);
                currentState = EnemyState.Chase;
                Vector2 dir = ((Vector2)player.position - rb.position).normalized;
                rb.MovePosition(rb.position + dir * controller.moveSpeed * Time.fixedDeltaTime);
            }
        }

        private void SetVisibility(bool visible)
        {
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = visible ? 1f : 0f;
                spriteRenderer.color = c;
            }
        }

        private void OnDisable()
        {
            // 사망 시 폭발 (Pooler 연동 권장)
            if (gameObject.activeSelf == false && explosionEffect != null && LobbyStateMarker.InLobby == false)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }
        }
    }
}
