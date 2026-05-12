// PlayerHealth.cs
// 플레이어 체력 관리. 적 접촉/투사체 데미지 수신 → 0 이하 시 GameOver.
// Unity 에디터:
//   - Player GameObject에 부착 (Tag "Player").
//   - hpBar(UI.Slider) 인스펙터 연결하면 자동 갱신.
//   - 무적 시간(invulnDuration) 동안 추가 데미지 차단(깜빡임 효과는 별도).

using UnityEngine;
using UnityEngine.UI;
using SolomonCopy.Systems;

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

        public int Hp { get; private set; }
        public bool IsDead { get; private set; }

        private float _invulnUntil;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Hp = maxHp;
            UpdateUI();
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;
            if (Time.time < _invulnUntil) return;
            _invulnUntil = Time.time + invulnDuration;

            Hp = Mathf.Max(0, Hp - amount);
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
