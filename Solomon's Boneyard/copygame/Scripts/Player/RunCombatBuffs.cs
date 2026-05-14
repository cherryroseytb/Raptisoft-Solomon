using UnityEngine;
using SolomonCopy.Systems;

namespace SolomonCopy.Player
{
    public class RunCombatBuffs : MonoBehaviour
    {
        public float damageMultiplier = 1f;

        private float _damageBuffUntil;

        private void Update()
        {
            if (damageMultiplier > 1f && Time.time >= _damageBuffUntil)
            {
                damageMultiplier = 1f;
            }
        }

        public void ApplyDamageMultiplier(float mul, float duration)
        {
            if (mul <= 1f || duration <= 0f) return;
            damageMultiplier = Mathf.Max(damageMultiplier, mul);
            _damageBuffUntil = Mathf.Max(_damageBuffUntil, Time.time + duration);
            if (TopCenterMessageFeed.Instance != null)
                TopCenterMessageFeed.Instance.Show($"{mul:0.#}x Damage!");
        }
    }
}
