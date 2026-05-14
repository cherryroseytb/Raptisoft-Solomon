using UnityEngine;

namespace SolomonCopy.Meta
{
    // UI에서 무덤지기 버튼/선택을 연결하기 위한 얇은 서비스 레이어.
    public class GravekeeperService : MonoBehaviour
    {
        [Header("비용 범위")]
        public int minCost = 100;
        public int maxCost = 400;

        public int LastRolledCost { get; private set; }

        public int RollCost()
        {
            if (MetaProgressionManager.Instance == null) return 0;
            if (!MetaProgressionManager.Instance.IsScavengerUnlocked()) return 0;
            LastRolledCost = MetaProgressionManager.Instance.RollGravekeeperCost(minCost, maxCost);
            return LastRolledCost;
        }

        // 무덤 메뉴 진입 시 1회 결제.
        public bool EnterGraveInventory()
        {
            if (MetaProgressionManager.Instance == null) return false;
            if (!MetaProgressionManager.Instance.IsScavengerUnlocked()) return false;
            if (LastRolledCost <= 0) LastRolledCost = RollCost();
            return MetaProgressionManager.Instance.TryEnterGraveSession(LastRolledCost);
        }

        // 진입 이후 반지 선택에는 추가 비용이 없다.
        public bool ConfirmSelection(int graveIndexA, int graveIndexB)
        {
            if (MetaProgressionManager.Instance == null) return false;
            if (!MetaProgressionManager.Instance.IsScavengerUnlocked()) return false;
            return MetaProgressionManager.Instance.TrySelectCarryOverInOpenedSession(graveIndexA, graveIndexB);
        }

        public bool ShouldShowBreakRiskNpcLine()
        {
            if (MetaProgressionManager.Instance == null) return false;
            if (MetaProgressionManager.Instance.explainedBreakRisk) return false;
            return MetaProgressionManager.Instance.HasShakingRingInGravePool();
        }

        public void MarkBreakRiskExplained()
        {
            if (MetaProgressionManager.Instance == null) return;
            MetaProgressionManager.Instance.explainedBreakRisk = true;
        }
    }
}
