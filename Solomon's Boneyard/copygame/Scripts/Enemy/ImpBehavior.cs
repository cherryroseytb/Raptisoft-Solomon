using UnityEngine;

namespace SolomonCopy.Enemy
{
    // 임프: 통통 튀며 무리 지어 빠르게 접근
    public class ImpBehavior : EnemyBehavior
    {
        public float bounceFrequency = 5f;
        public float bounceAmplitude = 0.5f;
        public float randomJitter = 0.2f;

        private float _phaseOffset;

        protected override void Start()
        {
            base.Start();
            _phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        protected override void UpdateState()
        {
            currentState = EnemyState.Chase;
            Vector2 baseDir = ((Vector2)player.position - rb.position).normalized;
            
            // 통통 튀는 느낌을 위한 수직 오프셋 (진행 방향의 수직)
            Vector2 perpendicular = new Vector2(-baseDir.y, baseDir.x);
            float bounce = Mathf.Sin(Time.time * bounceFrequency + _phaseOffset) * bounceAmplitude;
            
            Vector2 jitter = new Vector2(Random.Range(-randomJitter, randomJitter), Random.Range(-randomJitter, randomJitter));
            Vector2 finalMove = (baseDir + perpendicular * bounce + jitter).normalized;

            rb.MovePosition(rb.position + finalMove * controller.moveSpeed * Time.fixedDeltaTime);
        }
    }
}
