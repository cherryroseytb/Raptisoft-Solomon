// EnemyTypeSO.cs
// 적 종류별 스탯 데이터. WaveSpawner가 웨이브 인덱스에 따라 다른 타입을 섞어 스폰.
// Unity 에디터: Create > SolomonCopy > Enemy > EnemyType.

using UnityEngine;

namespace SolomonCopy.Enemy
{
    [CreateAssetMenu(menuName = "SolomonCopy/Enemy/EnemyType", fileName = "EnemyType_New")]
    public class EnemyTypeSO : ScriptableObject
    {
        public string displayName = "Skeleton";
        public GameObject prefab;
        public int maxHp = 30;
        public float moveSpeed = 2.5f;
        public int contactDamage = 1;
        public int xpAmount = 1;

        [Header("등장 제약")]
        public int minWave = 1;       // 이 웨이브부터 등장
        public float weight = 1f;     // 가중치 추첨용
    }
}
