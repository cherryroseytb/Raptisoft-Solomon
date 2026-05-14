using UnityEngine;
using SolomonCopy.Meta;
using SolomonCopy.Systems;
using SolomonCopy.Magic;

namespace SolomonCopy.Player
{
    public class RunStartBootstrap : MonoBehaviour
    {
        [Header("Master of Offense 시작 보너스")]
        public int masterOfOffensePrimarySkillBonus = 2;

        private void Start()
        {
            if (MetaProgressionManager.Instance == null) return;
            var inv = GetComponent<RunRingInventory>();
            var slots = GetComponent<LoadoutSlotManager>();
            var hp = GetComponent<PlayerHealth>();
            var caster = GetComponent<MagicCaster>();
            MetaProgressionManager.Instance.ApplyRunStartState(inv, slots);
            if (hp != null) hp.ConfigureRunServiceState(MetaProgressionManager.Instance.IsAutoPotionEnabled());
            if (caster != null)
            {
                caster.ConfigureStartSkillBonuses(
                    MetaProgressionManager.Instance.baseSkillPointBonusOnStart,
                    MetaProgressionManager.Instance.HasEquippedService(LobbySkillEffectId.MasterOfOffense)
                        ? Mathf.Max(0, masterOfOffensePrimarySkillBonus) : 0);
                ApplySecondSecondaryIfNeeded(caster);
            }
            if (GameManager.Instance != null)
                GameManager.Instance.SetGold(MetaProgressionManager.Instance.totalGold);
        }

        private void ApplySecondSecondaryIfNeeded(MagicCaster caster)
        {
            if (caster == null || MetaProgressionManager.Instance == null) return;
            if (!MetaProgressionManager.Instance.IsSecondSecondaryEnabled()) return;

            BaseMagicId primary = caster.slotA;
            BaseMagicId currentSecondary = caster.slotB;
            var all = (BaseMagicId[])System.Enum.GetValues(typeof(BaseMagicId));
            int count = 0;
            for (int i = 0; i < all.Length; i++)
            {
                var id = all[i];
                if (id == BaseMagicId.None || id == primary || id == currentSecondary) continue;
                count++;
            }
            if (count <= 0) return;

            int roll = Random.Range(0, count);
            int idx = 0;
            for (int i = 0; i < all.Length; i++)
            {
                var id = all[i];
                if (id == BaseMagicId.None || id == primary || id == currentSecondary) continue;
                if (idx == roll)
                {
                    caster.SetSlotB(id);
                    return;
                }
                idx++;
            }
        }
    }
}
