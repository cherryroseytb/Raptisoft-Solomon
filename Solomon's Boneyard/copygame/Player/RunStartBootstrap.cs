using UnityEngine;
using SolomonCopy.Meta;
using SolomonCopy.Systems;

namespace SolomonCopy.Player
{
    public class RunStartBootstrap : MonoBehaviour
    {
        private void Start()
        {
            if (MetaProgressionManager.Instance == null) return;
            var inv = GetComponent<RunRingInventory>();
            var slots = GetComponent<LoadoutSlotManager>();
            MetaProgressionManager.Instance.ApplyRunStartState(inv, slots);
            if (GameManager.Instance != null)
                GameManager.Instance.SetGold(MetaProgressionManager.Instance.totalGold);
        }
    }
}
