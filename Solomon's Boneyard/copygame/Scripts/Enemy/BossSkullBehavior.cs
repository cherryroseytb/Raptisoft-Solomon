using UnityEngine;
using SolomonCopy.Systems;
using System.Collections;

namespace SolomonCopy.Enemy
{
    // 보스: 떠다니는 큰 해골. 화살, 불덩이, 레이저, 몸통 박치기.
    public class BossSkullBehavior : EnemyBehavior
    {
        [Header("공격 프리팹")]
        public GameObject arrowPrefab;
        public GameObject fireballPrefab;
        public GameObject laserPrefab;
        public GameObject impPrefab;

        [Header("설정")]
        public float attackInterval = 4f;
        public float chargeSpeed = 15f;

        private float _nextActionTime;

        protected override void UpdateState()
        {
            if (Time.time < _nextActionTime)
            {
                // 동작 중에는 기본 추적만 천천히
                Vector2 dir = ((Vector2)player.position - rb.position).normalized;
                rb.MovePosition(rb.position + dir * controller.moveSpeed * 0.5f * Time.fixedDeltaTime);
                return;
            }

            // 무작위 패턴 선택
            int pattern = Random.Range(0, 4);
            switch(pattern)
            {
                case 0: StartCoroutine(PatternArrows()); break;
                case 1: StartCoroutine(PatternFireballs()); break;
                case 2: StartCoroutine(PatternLaser()); break;
                case 3: StartCoroutine(PatternCharge()); break;
            }
            _nextActionTime = Time.time + attackInterval;
        }

        IEnumerator PatternArrows()
        {
            for(int i=0; i<3; i++)
            {
                // 화살 2개씩 발사
                FireProjectile(arrowPrefab, 0.2f);
                FireProjectile(arrowPrefab, -0.2f);
                yield return new WaitForSeconds(0.4f);
            }
        }

        IEnumerator PatternFireballs()
        {
            int count = Random.Range(4, 7);
            for(int i=0; i<count; i++)
            {
                // 플레이어 이동 예측 지점으로 불덩이 투척
                Vector3 targetPos = player.position + (Vector3)player.GetComponent<Rigidbody2D>().velocity * 0.5f;
                GameObject fb = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
                // Fireball 로직: 바닥 도착 시 파이어로드 생성 및 확률적으로 임프 생성
                yield return new WaitForSeconds(0.6f);
            }
        }

        IEnumerator PatternLaser()
        {
            if (laserPrefab != null)
            {
                laserPrefab.SetActive(true);
                yield return new WaitForSeconds(2.0f);
                laserPrefab.SetActive(false);
            }
        }

        IEnumerator PatternCharge()
        {
            Vector2 startPos = rb.position;
            Vector2 targetPos = player.position;
            Vector2 dir = (targetPos - startPos).normalized;
            
            float elapsed = 0;
            while(elapsed < 0.8f)
            {
                rb.MovePosition(rb.position + dir * chargeSpeed * Time.fixedDeltaTime);
                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
        }

        private void FireProjectile(GameObject prefab, float angleOffset)
        {
            if (prefab == null) return;
            Vector2 dir = ((Vector2)player.position - rb.position).normalized;
            // 회전 적용 로직 생략 (단순화)
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }
}
