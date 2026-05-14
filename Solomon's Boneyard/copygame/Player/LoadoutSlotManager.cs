using UnityEngine;

namespace SolomonCopy.Player
{
    public class LoadoutSlotManager : MonoBehaviour
    {
        [Header("기능 슬롯(게임 시작 스킬/패시브 장착용)")]
        public int baseSlots = 2;
        public int tempExtraSlots;
        public int maxSlots = 6;

        public int CurrentSlots => Mathf.Clamp(baseSlots + tempExtraSlots, 0, maxSlots);

        public void SetTemporaryExtraSlots(int extra)
        {
            tempExtraSlots = Mathf.Max(0, extra);
        }

        public void ClearTemporarySlots()
        {
            tempExtraSlots = 0;
        }
    }
}
