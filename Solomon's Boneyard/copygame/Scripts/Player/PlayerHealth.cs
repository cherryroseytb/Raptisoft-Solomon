// PlayerHealth.cs
// 플레이어 체력 관리. 적 접촉/투사체 데미지 수신 → 0 이하 시 GameOver.
// Unity 에디터:
//   - Player GameObject에 부착 (Tag "Player").
//   - hpBar(UI.Slider) 인스펙터 연결하면 자동 갱신.
//   - 무적 시간(invulnDuration) 동안 추가 데미지 차단(깜빡임 효과는 별도).

using UnityEngine;
using UnityEngine.UI;
using SolomonCopy.Systems;
using SolomonCopy.Meta;
using SolomonCopy.Magic;

namespace SolomonCopy.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        public static PlayerHealth Instance { get; private set; }

        [Header("스탯")]
        public int maxHp = 100;
        public float invulnDuration = 0.5f;

        [Header("UI")]
        public Slider hpBar;

        [Header("서비스 연동")]
        public float autoPotionTriggerHpRatio = 0.25f;
        public int autoPotionHealPercent = 50;
        public float blazeRadius = 3.2f;
        public int blazeDamage = 120;

        public int Hp { get; private set; }
        public bool IsDead { get; private set; }

        private float _invulnUntil;
        private int _autoPotionCharges;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Hp = maxHp;
            _autoPotionCharges = 0;
            UpdateUI();
        }

        public void ConfigureRunServiceState(bool autoPotionEnabled)
        {
            _autoPotionCharges = autoPotionEnabled ? 1 : 0;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;
            if (Time.time < _invulnUntil) return;
            _invulnUntil = Time.time + invulnDuration;

            Hp = Mathf.Max(0, Hp - amount);

            // Auto Potion: 치명 구간에서 1회 자동 회복
            if (!IsDead && _autoPotionCharges > 0 && MetaProgressionManager.Instance != null && MetaProgressionManager.Instance.IsAutoPotionEnabled())
            {
                float threshold = maxHp * autoPotionTriggerHpRatio;
                if (Hp > 0 && Hp <= threshold)
                {
                    _autoPotionCharges--;
                    int healAmount = Mathf.RoundToInt(maxHp * (autoPotionHealPercent / 100f));
                    Heal(healAmount);
                    if (TopCenterMessageFeed.Instance != null) TopCenterMessageFeed.Instance.Show("Auto Potion 발동");
                }
            }

            UpdateUI();
            if (Hp <= 0) Die();
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            Hp = Mathf.Min(maxHp, Hp + amount);
            UpdateUI();
        }

        private void Die()
        {
            IsDead = true;

            // Blaze of Glory: 사망 시 주변 폭발
            if (MetaProgressionManager.Instance != null && MetaProgressionManager.Instance.IsBlazeOfGloryEnabled())
            {
                var hits = Physics2D.OverlapCircleAll(transform.position, blazeRadius);
                for (int i = 0; i < hits.Length; i++)
                {
                    var enemy = hits[i].GetComponent<SolomonCopy.Enemy.EnemyController>();
                    if (enemy != null) enemy.TakeDamage(blazeDamage, StatusEffect.None, 0f);
                }
                if (TopCenterMessageFeed.Instance != null) TopCenterMessageFeed.Instance.Show("Blaze of Glory!");
            }

            if (MetaProgressionManager.Instance != null)
            {
                var runInv = GetComponent<RunRingInventory>();
                MetaProgressionManager.Instance.OnRunEndedByDeath(runInv);
            }
            if (GameManager.Instance != null) GameManager.Instance.GameOver();
        }

        private void UpdateUI()
        {
            if (hpBar == null) return;
            hpBar.maxValue = maxHp;
            hpBar.value = Hp;
        }
    }
}
