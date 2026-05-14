namespace SolomonCopy.Meta
{
    // 맵(묘지) 선택 UI에서 사용 가능 슬롯을 조회하는 서비스.
    public class GraveyardSelectionService
    {
        public int GetAvailableCount()
        {
            if (MetaProgressionManager.Instance == null) return 1;
            return MetaProgressionManager.Instance.GetAvailableGraveyardCount();
        }

        public bool IsIndexUnlocked(int index)
        {
            // index: 0..3
            return index >= 0 && index < GetAvailableCount();
        }
    }
}
