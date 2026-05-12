// WaveSpawner.cs
// 시간 기반 웨이브 스폰. 화면 바깥 원형 좌표에서 적을 풀에서 꺼냄.
// Unity 에디터:
//   - 빈 GameObject에 부착. enemyPrefab(EnemyController 부착된 프리팹) 연결.
//   - player 비워두면 Tag "Player"로 자동 탐색.

using UnityEngine;
using SolomonCopy.Systems;

namespace SolomonCopy.Enemy
{
    public class WaveSpawner : MonoBehaviour
    {
        [Header("스폰 설정")]
        public GameObject enemyPrefab;
        public Transform player;
        public float waveInterval = 5f;     // 웨이브 간 간격(초)
        public int enemiesPerWave = 5;
        public float spawnRadius = 10f;     // 플레이어 기준 스폰 거리

        [Header("난이도")]
        public int waveIncreasePerCycle = 1; // 매 웨이브마다 적 수 증가

        private float _nextWaveAt;
        private int _waveIndex;

        private void Start()
        {
            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
            }
            _nextWaveAt = Time.time + 1f;
        }

        private void Update()
        {
            if (Time.time < _nextWaveAt) return;
            SpawnWave();
            _nextWaveAt = Time.time + waveInterval;
        }

        private void SpawnWave()
        {
            if (enemyPrefab == null || player == null) return;
            int count = enemiesPerWave + _waveIndex * waveIncreasePerCycle;
            for (int i = 0; i < count; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
                Vector3 pos = player.position + (Vector3)offset;
                if (ObjectPooler.Instance != null)
                    ObjectPooler.Instance.Spawn(enemyPrefab, pos, Quaternion.identity);
                else
                    Instantiate(enemyPrefab, pos, Quaternion.identity);
            }
            _waveIndex++;
            if (GameManager.Instance != null) GameManager.Instance.SetWave(_waveIndex);
        }
    }
}
