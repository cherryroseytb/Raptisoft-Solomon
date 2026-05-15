// SoundLibrarySO.cs
// SoundId → AudioClip 매핑 데이터 에셋.
// Unity 에디터: Create > SolomonCopy > Audio > SoundLibrary 로 생성.
// 같은 SoundId에 여러 클립을 등록하면 재생 시 랜덤 선택.

using System.Collections.Generic;
using UnityEngine;

namespace SolomonCopy.Systems
{
    [CreateAssetMenu(menuName = "SolomonCopy/Audio/SoundLibrary", fileName = "SoundLibrary")]
    public class SoundLibrarySO : ScriptableObject
    {
        [System.Serializable]
        public class SoundEntry
        {
            public SoundId id;
            public AudioClip[] clips;
            [Range(0f, 1f)] public float volume = 1f;
            [Range(0.5f, 2f)] public float pitchMin = 0.95f;
            [Range(0.5f, 2f)] public float pitchMax = 1.05f;
        }

        public SoundEntry[] entries;

        private Dictionary<SoundId, SoundEntry> _lookup;

        private void BuildLookup()
        {
            _lookup = new Dictionary<SoundId, SoundEntry>(entries.Length);
            foreach (var e in entries)
                if (e != null && e.id != SoundId.None) _lookup[e.id] = e;
        }

        public SoundEntry Get(SoundId id)
        {
            if (_lookup == null) BuildLookup();
            _lookup.TryGetValue(id, out var entry);
            return entry;
        }
    }
}
