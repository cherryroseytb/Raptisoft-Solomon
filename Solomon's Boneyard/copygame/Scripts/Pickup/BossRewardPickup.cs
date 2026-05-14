using UnityEngine;
using SolomonCopy.Meta;
using SolomonCopy.Player;
using SolomonCopy.Systems;

namespace SolomonCopy.Pickup
{
    public class BossRewardPickup : MonoBehaviour
    {
        public enum BossRewardType
        {
            RandomRing = 1,
            SkillPoint = 2,
            Damage4x = 3
        }

        public BossRewardType rewardType = BossRewardType.RandomRing;
        public float attractRadius = 2.2f;
        public float attractSpeed = 7f;
        public float collectRadius = 0.45f;
        public float damage4xDuration = 10f;

        private Transform _player;

        public void SetReward(BossRewardType type)
        {
            rewardType = type;
        }

        private void OnEnable()
        {
            if (_player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) _player = p.transform;
            }
        }

        private void Update()
        {
            if (_player == null) return;
            float dist = Vector2.Distance(transform.position, _player.position);
            if (dist <= collectRadius) { Collect(); return; }
            if (dist <= attractRadius)
            {
                Vector3 dir = (_player.position - transform.position).normalized;
                transform.position += dir * attractSpeed * Time.deltaTime;
            }
        }

        private void Collect()
        {
            switch (rewardType)
            {
                case BossRewardType.RandomRing:
                    if (MetaProgressionManager.Instance != null)
                    {
                        var ring = MetaProgressionManager.Instance.CreateRandomRing();
                        var inv = _player != null ? _player.GetComponent<RunRingInventory>() : null;
                        if (ring != null && inv != null) inv.AddRing(ring);
                    }
                    if (TopCenterMessageFeed.Instance != null) TopCenterMessageFeed.Instance.Show("Boss Reward: Ring");
                    break;
                case BossRewardType.SkillPoint:
                    if (PlayerExperience.Instance != null) PlayerExperience.Instance.AddXp(PlayerExperience.Instance.XpNeeded);
                    if (TopCenterMessageFeed.Instance != null) TopCenterMessageFeed.Instance.Show("Boss Reward: Skill Point");
                    break;
                case BossRewardType.Damage4x:
                    var buffs = _player != null ? _player.GetComponent<RunCombatBuffs>() : null;
                    if (buffs != null) buffs.ApplyDamageMultiplier(4f, damage4xDuration);
                    break;
            }

            if (ObjectPooler.Instance != null) ObjectPooler.Instance.Return(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
