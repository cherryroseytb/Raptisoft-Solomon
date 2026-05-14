// MagicRegistrySO.cs
// 모든 BaseMagicSO와 ComboMagicSO를 한 곳에 모아두는 인덱스 에셋.
// Unity 에디터: Create > SolomonCopy > Magic > MagicRegistry. 단 1개만 생성하고 MagicCaster에 연결.
// 런타임 룩업은 순서 무관 (Lookup(A,B) == Lookup(B,A)).

using System.Collections.Generic;
using UnityEngine;

namespace SolomonCopy.Magic
{
    [CreateAssetMenu(menuName = "SolomonCopy/Magic/MagicRegistry", fileName = "MagicRegistry")]
    public class MagicRegistrySO : ScriptableObject
    {
        [SerializeField] private List<BaseMagicSO> baseMagics = new List<BaseMagicSO>();
        [SerializeField] private List<ComboMagicSO> combos = new List<ComboMagicSO>();

        // (정렬된 ID 쌍) -> Combo 캐시. OnEnable에서 빌드.
        private Dictionary<(BaseMagicId, BaseMagicId), ComboMagicSO> _comboLookup;
        private Dictionary<BaseMagicId, BaseMagicSO> _baseLookup;

        private void OnEnable()
        {
            BuildCache();
        }

        private void BuildCache()
        {
            _baseLookup = new Dictionary<BaseMagicId, BaseMagicSO>();
            foreach (var bm in baseMagics)
            {
                if (bm == null || bm.id == BaseMagicId.None) continue;
                _baseLookup[bm.id] = bm;
            }

            _comboLookup = new Dictionary<(BaseMagicId, BaseMagicId), ComboMagicSO>();
            foreach (var c in combos)
            {
                if (c == null) continue;
                var key = SortKey(c.inputA, c.inputB);
                _comboLookup[key] = c;
            }
        }

        public BaseMagicSO GetBase(BaseMagicId id)
        {
            if (_baseLookup == null) BuildCache();
            return _baseLookup.TryGetValue(id, out var bm) ? bm : null;
        }

        // 두 슬롯 ID로 조합 마법 룩업. 없으면 null.
        public ComboMagicSO LookupCombo(BaseMagicId a, BaseMagicId b)
        {
            if (_comboLookup == null) BuildCache();
            var key = SortKey(a, b);
            return _comboLookup.TryGetValue(key, out var combo) ? combo : null;
        }

        private static (BaseMagicId, BaseMagicId) SortKey(BaseMagicId a, BaseMagicId b)
        {
            // enum 정수값 기준 정렬해서 순서 무관 키 생성
            return ((int)a <= (int)b) ? (a, b) : (b, a);
        }
    }
}
