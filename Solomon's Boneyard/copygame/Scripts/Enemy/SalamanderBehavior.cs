using UnityEngine;
using SolomonCopy.Systems;

namespace SolomonCopy.Enemy
{
    // 샐러맨더: 높은 방어력, 사망 시 임프 다수 생성
    public class SalamanderBehavior : EnemyBehavior
    {
        public GameObject impPrefab;
        public int impCount = 3;

        private void OnDisable()
        {
            if (gameObject.activeSelf == false && impPrefab != null && LobbyStateMarker.InLobby == false)
            {
                for(int i=0; i<impCount; i++)
                {
                    Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                    if (ObjectPooler.Instance != null)
                        ObjectPooler.Instance.Spawn(impPrefab, transform.position + offset, Quaternion.identity);
                    else
                        Instantiate(impPrefab, transform.position + offset, Quaternion.identity);
                }
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
