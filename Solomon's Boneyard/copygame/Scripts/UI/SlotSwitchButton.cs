// SlotSwitchButton.cs
// 마법 슬롯(A/B) 교체용 UI 버튼.
// Unity 에디터:
//   - Canvas 아래 Button 2개 만들고 각각에 부착.
//   - slotIndex (0 or 1), targetMagic 필드를 인스펙터에서 설정.
//   - Button의 OnClick에 이 컴포넌트의 OnClickAssign 연결.

using UnityEngine;
using UnityEngine.UI;
using SolomonCopy.Magic;
using SolomonCopy.Player;

namespace SolomonCopy.UI
{
    [RequireComponent(typeof(Button))]
    public class SlotSwitchButton : MonoBehaviour
    {
        [Tooltip("0=슬롯A, 1=슬롯B")]
        public int slotIndex = 0;
        public BaseMagicId targetMagic = BaseMagicId.Fire;
        public MagicCaster caster;

        private void Reset()
        {
            GetComponent<Button>().onClick.AddListener(OnClickAssign);
        }

        private void Awake()
        {
            var btn = GetComponent<Button>();
            btn.onClick.RemoveListener(OnClickAssign);
            btn.onClick.AddListener(OnClickAssign);
            if (caster == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) caster = p.GetComponent<MagicCaster>();
            }
        }

        public void OnClickAssign()
        {
            if (caster == null) return;
            if (slotIndex == 0) caster.SetSlotA(targetMagic);
            else caster.SetSlotB(targetMagic);
        }
    }
}
