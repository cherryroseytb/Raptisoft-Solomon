// WaveSpawner.cs
// 시간 기반 웨이브 스폰. 화면 바깥 원형 좌표에서 적을 풀에서 꺼냄.
// Unity 에디터:
//   - 빈 GameObject에 부착. enemyPrefab(EnemyController 부착된 프리팹) 연결.
//   - player 비워두면 Tag "Player"로 자동 탐색.

using UnityEngine;
using SolomonCopy.Systems;
using SolomonCopy.Meta;

namespace SolomonCopy.Enemy
{
    public class WaveSpawner : MonoBehaviour
    {
        [Header("스폰 설정")]
        public GameObject enemyPrefab;       // 폴백용 (enemyTypes 비어있을 때 사용)
        public EnemyTypeSO[] enemyTypes;     // 가중치 추첨 대상 (비워두면 enemyPrefab 사용)
        public Transform player;
        public float waveInterval = 5f;     // 웨이브 간 간격(초)
        public int enemiesPerWave = 5;
        public float spawnRadius = 10f;     // 플레이어 기준 스폰 거리
        public GameObject bossEnemyPrefab;   // Fate: Boss Monsters 전용 보스 프리팹
        public bool useKillBasedBossSpawn = true;
        public int bossFirstSpawnAtKills = 400;
        public int bossSpawnEveryKills = 400;
        public int bossWaveInterval = 8;     // kill 기반을 끄는 경우 폴백

        [Header("난이도")]
        public int waveIncreasePerCycle = 1; // 매 웨이브마다 적 수 증가

        private float _nextWaveAt;
        private int _waveIndex;
        private int _nextBossSpawnKillCount;
        private float _cachedWaveTotal = -1f;

        private void Start()
        {
            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
            }
            _nextWaveAt = Time.time + 1f;
            _nextBossSpawnKillCount = Mathf.Max(1, bossFirstSpawnAtKills);
            SoundManager.Instance?.PlayBattleBgm();
        }

        private void Update()
        {
            TrySpawnBossByKillCount();
            if (Time.time < _nextWaveAt) return;
            SpawnWave();
            _nextWaveAt = Time.time + waveInterval;
        }

        private void SpawnWave()
        {
            if (player == null) return;
            bool spawnBossThisWave = !useKillBasedBossSpawn && ShouldSpawnBossThisWave();
            if (spawnBossThisWave)
            {
                SpawnBoss();
            }

            int count = enemiesPerWave + _waveIndex * waveIncreasePerCycle;
            for (int i = 0; i < count; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
                Vector3 pos = player.position + (Vector3)offset;

                EnemyTypeSO type = PickEnemyType();
                GameObject prefabToUse = type != null ? type.prefab : enemyPrefab;
                if (prefabToUse == null) continue;

                GameObject go = ObjectPooler.Instance != null
                    ? ObjectPooler.Instance.Spawn(prefabToUse, pos, Quaternion.identity)
                    : Instantiate(prefabToUse, pos, Quaternion.identity);

                // 타입 데이터를 인스턴스에 적용 (풀에서 재사용된 인스턴스의 OnEnable에서 HP 리셋되도록)
                if (type != null)
                {
                    var ec = go.GetComponent<EnemyController>();
                    if (ec != null)
                    {
                        ec.maxHp = type.maxHp;
                        ec.moveSpeed = type.moveSpeed;
                        ec.contactDamage = type.contactDamage;
                        ec.xpAmount = type.xpAmount;
                    }
                }
            }
            SoundManager.Instance?.Play(SoundId.WaveStart);
            _waveIndex++;
            _cachedWaveTotal = -1f; // 다음 웨이브에서 가중치 재계산
            if (GameManager.Instance != null) GameManager.Instance.SetWave(_waveIndex);
        }

        private bool ShouldSpawnBossThisWave()
        {
            if (MetaProgressionManager.Instance == null) return false;
            if (!MetaProgressionManager.Instance.IsBossMonstersEnabled()) return false;
            if (bossEnemyPrefab == null) return false;
            if (_waveIndex <= 0) return false;
            return (_waveIndex % Mathf.Max(1, bossWaveInterval)) == 0;
        }

        private void TrySpawnBossByKillCount()
        {
            if (!useKillBasedBossSpawn) return;
            if (MetaProgressionManager.Instance == null) return;
            if (!MetaProgressionManager.Instance.IsBossMonstersEnabled()) return;
            if (bossEnemyPrefab == null) return;
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.KillCount < _nextBossSpawnKillCount) return;
            SpawnBoss();
            _nextBossSpawnKillCount += Mathf.Max(1, bossSpawnEveryKills);
        }

        private void SpawnBoss()
        {
            SoundManager.Instance?.Play(SoundId.BossAppear);
            SolomonCopy.UI.BossIntroController.Instance?.Play();
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
            Vector3 pos = player.position + (Vector3)offset;
            if (ObjectPooler.Instance != null)
                ObjectPooler.Instance.Spawn(bossEnemyPrefab, pos, Quaternion.identity);
            else
                Instantiate(bossEnemyPrefab, pos, Quaternion.identity);
        }

        // 현재 웨이브에서 등장 가능한 타입 중 가중치 추첨. 웨이브당 합계는 1회만 계산.
        private EnemyTypeSO PickEnemyType()
        {
            if (enemyTypes == null || enemyTypes.Length == 0) return null;

            if (_cachedWaveTotal < 0f)
            {
                _cachedWaveTotal = 0f;
                for (int i = 0; i < enemyTypes.Length; i++)
                {
                    var t = enemyTypes[i];
                    if (t != null && _waveIndex + 1 >= t.minWave) _cachedWaveTotal += t.weight;
                }
            }
            if (_cachedWaveTotal <= 0f) return null;

            float roll = Random.Range(0f, _cachedWaveTotal);
            float acc = 0f;
            for (int i = 0; i < enemyTypes.Length; i++)
            {
                var t = enemyTypes[i];
                if (t == null || _waveIndex + 1 < t.minWave) continue;
                acc += t.weight;
                if (roll <= acc) return t;
            }
            return null;
        }
    }
}
