// ProjectileEffects.cs
// 발사체의 특수 효과(폭발 AOE / 연쇄 번개) 헬퍼.
// Projectile.cs 에서 충돌 순간 호출.

using UnityEngine;
using System.Collections.Generic;
using SolomonCopy.Magic;
using SolomonCopy.Enemy;
using SolomonCopy.Systems;

namespace SolomonCopy.ProjectileSys
{
    public static class ProjectileEffects
    {
        // 폭발: 지정 반경 내 모든 Enemy에 데미지/상태이상 적용
        public static void Explode(Vector2 center, float radius, int damage, StatusEffect effect, float duration)
        {
            VfxManager.Instance?.Play(VfxId.Explosion, center);
            var hits = Physics2D.OverlapCircleAll(center, radius);
            foreach (var col in hits)
            {
                if (!col.CompareTag("Enemy")) continue;
                var e = col.GetComponent<EnemyController>();
                if (e != null) e.TakeDamage(damage, effect, duration);
            }
        }

        // 연쇄: 기준 적에서 가까운 다음 적으로 chainCount만큼 점프
        public static void Chain(EnemyController origin, int chainCount, float range,
                                  int damage, StatusEffect effect, float duration)
        {
            var hit = new HashSet<EnemyController> { origin };
            EnemyController current = origin;
            for (int i = 0; i < chainCount; i++)
            {
                var next = FindNearestEnemy(current.transform.position, range, hit);
                if (next == null) break;
                next.TakeDamage(damage, effect, duration);
                VfxManager.Instance?.Play(VfxId.ChainLightning, next.transform.position);
                hit.Add(next);
                current = next;
            }
        }

        private static EnemyController FindNearestEnemy(Vector2 from, float range, HashSet<EnemyController> exclude)
        {
            var hits = Physics2D.OverlapCircleAll(from, range);
            EnemyController best = null;
            float bestDist = float.MaxValue;
            foreach (var col in hits)
            {
                if (!col.CompareTag("Enemy")) continue;
                var e = col.GetComponent<EnemyController>();
                if (e == null || exclude.Contains(e)) continue;
                float d = ((Vector2)e.transform.position - from).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = e; }
            }
            return best;
        }
    }
}
