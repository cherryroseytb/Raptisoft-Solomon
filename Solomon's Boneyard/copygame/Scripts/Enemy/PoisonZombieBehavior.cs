using UnityEngine;
using SolomonCopy.Systems;

namespace SolomonCopy.Enemy
{
    // 독 좀비: 사망 시 독 웅덩이 생성
    public class PoisonZombieBehavior : EnemyBehavior
    {
        public GameObject poisonPoolPrefab;

        private void OnDisable()
        {
            // 간단하게 OnDisable에서 사망 시 체크 (실제로는 Die() 훅 권장)
            if (gameObject.activeSelf == false && poisonPoolPrefab != null && LobbyStateMarker.InLobby == false)
            {
                if (ObjectPooler.Instance != null)
                    ObjectPooler.Instance.Spawn(poisonPoolPrefab, transform.position, Quaternion.identity);
                else
                    Instantiate(poisonPoolPrefab, transform.position, Quaternion.identity);
            }
        }

        protected override void UpdateState()
        {
            currentState = EnemyState.Chase;
            MoveTowardsPlayer();
        }
    }
}
