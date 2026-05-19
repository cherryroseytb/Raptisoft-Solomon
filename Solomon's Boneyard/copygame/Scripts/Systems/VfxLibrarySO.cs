// VfxLibrarySO.cs
// VfxId → 이펙트 프리팹 매핑 데이터 에셋.
// Unity 에디터:
//   - Create > SolomonCopy > Vfx > VfxLibrary 로 생성.
//   - entries 배열에 VfxId와 프리팹을 짝지어 등록.
//   - 같은 id에 여러 prefab 등록 시 재생 시 랜덤 선택 (없으면 첫 번째).
//   - prefab은 ParticleSystem 또는 SpriteRenderer + 자동 비활성 스크립트 권장.

using System.Collections.Generic;
using UnityEngine;

namespace SolomonCopy.Systems
{
    [CreateAssetMenu(menuName = "SolomonCopy/Vfx/VfxLibrary", fileName = "VfxLibrary")]
    public class VfxLibrarySO : ScriptableObject
    {
        [System.Serializable]
        public class VfxEntry
        {
            public VfxId id;
            public GameObject[] prefabs;
            [Tooltip("자동 회수까지의 시간(초). 0 이하이면 회수하지 않음 (프리팹이 직접 관리).")]
            public float lifetime = 1.0f;
            [Tooltip("재생 위치에 적용할 스케일 배수. 0 이하이면 1로 처리.")]
            public float scale = 1.0f;
        }

        public VfxEntry[] entries;

        private Dictionary<VfxId, VfxEntry> _lookup;

        private void BuildLookup()
        {
            _lookup = new Dictionary<VfxId, VfxEntry>(entries != null ? entries.Length : 0);
            if (entries == null) return;
            foreach (var e in entries)
                if (e != null && e.id != VfxId.None) _lookup[e.id] = e;
        }

        public VfxEntry Get(VfxId id)
        {
            if (_lookup == null) BuildLookup();
            _lookup.TryGetValue(id, out var entry);
            return entry;
        }
    }
}
