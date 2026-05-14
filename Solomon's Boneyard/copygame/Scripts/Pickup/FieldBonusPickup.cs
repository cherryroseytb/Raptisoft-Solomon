using UnityEngine;
using SolomonCopy.Player;
using SolomonCopy.Systems;

namespace SolomonCopy.Pickup
{
    // 일반 필드 드롭용 보너스 픽업 (스킬포인트/4배데미지)
    public class FieldBonusPickup : MonoBehaviour
    {
        public enum FieldBonusType
        {
            SkillPoint = 1,
            RandomSkillPoint = 2,
            Damage4x = 3
        }

        public FieldBonusType bonusType = FieldBonusType.SkillPoint;
        public float attractRadius = 2.2f;
        public float attractSpeed = 7f;
        public float collectRadius = 0.45f;
        public float damage4xDuration = 10f;

        private Transform _player;

        public void SetBonus(FieldBonusType type)
        {
            bonusType = type;
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
            switch (bonusType)
            {
                case FieldBonusType.SkillPoint:
                    if (PlayerExperience.Instance != null) PlayerExperience.Instance.AddXp(PlayerExperience.Instance.XpNeeded);
                    if (TopCenterMessageFeed.Instance != null) TopCenterMessageFeed.Instance.Show("Skill Point +1");
                    break;
                case FieldBonusType.RandomSkillPoint:
                    if (PlayerExperience.Instance != null) PlayerExperience.Instance.AddXp(PlayerExperience.Instance.XpNeeded);
                    if (TopCenterMessageFeed.Instance != null) TopCenterMessageFeed.Instance.Show("Random Skill Point +1");
                    break;
                case FieldBonusType.Damage4x:
                    var buffs = _player != null ? _player.GetComponent<RunCombatBuffs>() : null;
                    if (buffs != null) buffs.ApplyDamageMultiplier(4f, damage4xDuration);
                    if (TopCenterMessageFeed.Instance != null) TopCenterMessageFeed.Instance.Show("Damage x4");
                    break;
            }

            if (ObjectPooler.Instance != null) ObjectPooler.Instance.Return(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
