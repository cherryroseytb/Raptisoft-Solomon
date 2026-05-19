// VfxManager.cs
// 파티클/이펙트 재생 싱글톤. ObjectPooler를 이용해 재사용.
// Unity 에디터:
//   - 빈 GO "VfxManager" 생성 후 부착. DontDestroyOnLoad 동작.
//   - library 필드에 VfxLibrarySO 연결.
// 사용법:
//   VfxManager.Instance?.Play(VfxId.EnemyHit, transform.position);
//   VfxManager.Instance?.Play(VfxId.Explosion, pos, rotation);

using System.Collections;
using UnityEngine;

namespace SolomonCopy.Systems
{
    public class VfxManager : MonoBehaviour
    {
        public static VfxManager Instance { get; private set; }

        [Header("라이브러리")]
        public VfxLibrarySO library;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Play(VfxId id, Vector3 position)
        {
            Play(id, position, Quaternion.identity);
        }

        public void Play(VfxId id, Vector3 position, Quaternion rotation)
        {
            if (library == null || id == VfxId.None) return;
            var entry = library.Get(id);
            if (entry == null || entry.prefabs == null || entry.prefabs.Length == 0) return;

            var prefab = entry.prefabs[entry.prefabs.Length == 1 ? 0 : Random.Range(0, entry.prefabs.Length)];
            if (prefab == null) return;

            GameObject go = ObjectPooler.Instance != null
                ? ObjectPooler.Instance.Spawn(prefab, position, rotation)
                : Instantiate(prefab, position, rotation);
            if (go == null) return;

            float s = entry.scale > 0f ? entry.scale : 1f;
            go.transform.localScale = Vector3.one * s;

            // ParticleSystem이 있으면 재시작
            var ps = go.GetComponent<ParticleSystem>();
            if (ps != null) { ps.Clear(); ps.Play(); }

            if (entry.lifetime > 0f) StartCoroutine(ReturnAfter(go, entry.lifetime));
        }

        private IEnumerator ReturnAfter(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go == null) yield break;
            if (ObjectPooler.Instance != null) ObjectPooler.Instance.Return(go);
            else Destroy(go);
        }
    }
}
