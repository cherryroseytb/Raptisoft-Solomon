using UnityEngine;

namespace SolomonCopy.Enemy
{
    public class ChargerEnemyBehavior : EnemyBehavior
    {
        public float detectRange = 6f;
        public float chargeSpeed = 12f;
        public float chargeDuration = 1.2f;
        public float chargeCooldown = 3f;

        private float _chargeUntil;
        private float _nextChargeTime;
        private Vector2 _chargeDir;

        protected override void UpdateState()
        {
            if (Time.time < _chargeUntil)
            {
                currentState = EnemyState.Charge;
                PerformCharge();
                return;
            }

            float dist = Vector2.Distance(rb.position, player.position);

            if (dist <= detectRange && Time.time >= _nextChargeTime)
            {
                StartCharge();
            }
            else
            {
                currentState = EnemyState.Chase;
                MoveTowardsPlayer();
            }
        }

        private void StartCharge()
        {
            currentState = EnemyState.Charge;
            _chargeDir = ((Vector2)player.position - rb.position).normalized;
            _chargeUntil = Time.time + chargeDuration;
            _nextChargeTime = Time.time + chargeDuration + chargeCooldown;
        }

        private void PerformCharge()
        {
            rb.MovePosition(rb.position + _chargeDir * chargeSpeed * Time.fixedDeltaTime);
        }

    }
}
