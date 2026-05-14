using UnityEngine;

namespace SolomonCopy.Enemy
{
    // 아머 스켈레톤: 방어구 파괴 후 일반 스켈레톤으로 전환
    public class ArmoredSkeletonBehavior : EnemyBehavior
    {
        public int armorHp = 50;
        public GameObject normalSkeletonPrefab; 

        public void OnTakeDamage(int damage)
        {
            armorHp -= damage;
            if (armorHp <= 0)
            {
                BreakArmor();
            }
        }

        private void BreakArmor()
        {
            // 방어구 탈착 이펙트나 사운드 재생 후 일반 스켈레톤으로 교체하거나 스탯 변경
            if (normalSkeletonPrefab != null)
            {
                Instantiate(normalSkeletonPrefab, transform.position, transform.rotation);
                gameObject.SetActive(false); // 현재(아머형)는 비활성화
            }
        }

        protected override void UpdateState()
        {
            currentState = EnemyState.Chase;
            Vector2 dir = ((Vector2)player.position - rb.position).normalized;
            rb.MovePosition(rb.position + dir * controller.moveSpeed * Time.fixedDeltaTime);
        }
    }
}
