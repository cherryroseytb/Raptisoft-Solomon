// ObjectPooler.cs
// 발사체/적/이펙트 재사용을 위한 단순 싱글톤 풀.
// Unity 에디터: 빈 GameObject(이름 "ObjectPooler")에 이 스크립트 부착. 씬에 1개만.
// 사용법:
//   GameObject go = ObjectPooler.Instance.Spawn(prefab, pos, rot);
//   ObjectPooler.Instance.Return(go);
// 첫 부족 시 Instantiate, 이후엔 재사용.

using System.Collections.Generic;
using UnityEngine;

namespace SolomonCopy.Systems
{
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        // 프리팹의 InstanceID -> Queue<GameObject> 매핑
        private readonly Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();
        // 인스턴스 -> 원본 프리팹 InstanceID (반환 시 어디로 돌릴지 식별)
        private readonly Dictionary<int, int> _instanceToPrefabId = new Dictionary<int, int>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;
            int prefabId = prefab.GetInstanceID();
            if (!_pools.TryGetValue(prefabId, out var queue))
            {
                queue = new Queue<GameObject>();
                _pools[prefabId] = queue;
            }

            GameObject go;
            if (queue.Count > 0)
            {
                go = queue.Dequeue();
                go.transform.SetPositionAndRotation(position, rotation);
                go.SetActive(true);
            }
            else
            {
                go = Instantiate(prefab, position, rotation);
                _instanceToPrefabId[go.GetInstanceID()] = prefabId;
            }
            return go;
        }

        public void Return(GameObject instance)
        {
            if (instance == null) return;
            instance.SetActive(false);
            int instId = instance.GetInstanceID();
            if (_instanceToPrefabId.TryGetValue(instId, out int prefabId))
            {
                if (!_pools.TryGetValue(prefabId, out var queue))
                {
                    queue = new Queue<GameObject>();
                    _pools[prefabId] = queue;
                }
                queue.Enqueue(instance);
            }
            else
            {
                // 풀에서 나온 게 아니면 그냥 파괴
                Destroy(instance);
            }
        }
    }
}
