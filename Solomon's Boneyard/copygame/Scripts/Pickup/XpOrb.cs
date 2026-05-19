// XpOrb.cs
// 적이 죽을 때 드롭되는 경험치 구슬. 플레이어가 일정 거리 안에 들어오면 자석처럼 끌려감.
// Unity 에디터:
//   - 빈 GO + SpriteRenderer(작은 보석/원) + CircleCollider2D(IsTrigger ✓, radius 1) + Rigidbody2D(Kinematic)
//   - 이 스크립트 부착, Tag "Pickup", Layer "Pickup" (선택)
//   - WaveSpawner나 EnemyController에서 ObjectPooler로 스폰.

using UnityEngine;
using SolomonCopy.Player;
using SolomonCopy.Systems;
using SolomonCopy.Meta;

namespace SolomonCopy.Pickup
{
    public class XpOrb : MonoBehaviour
    {
        [Header("값")]
        public int xpAmount = 1;

        [Header("자석")]
        public float attractRadius = 2.5f;
        public float attractSpeed = 8f;
        public float collectRadius = 0.4f;

        private Transform _player;
        private float _baseAttractRadius;

        private void Awake()
        {
            _baseAttractRadius = attractRadius;
        }

        private void OnEnable()
        {
            if (_player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) _player = p.transform;
            }

            if (MetaProgressionManager.Instance != null)
            {
                attractRadius = _baseAttractRadius * MetaProgressionManager.Instance.GetXpOrbAttractRadiusMultiplier();
            }
            else attractRadius = _baseAttractRadius;
        }

        private void Update()
        {
            if (_player == null) return;
            float dist = Vector2.Distance(transform.position, _player.position);
            if (dist <= collectRadius) { Collect(); return; }
            if (dist <= attractRadius)
            {
                Vector3 dir = (_player.position - transform.position).normalized;
                transform.position += dir * attractSpeed * Time.deltaTime;
            }
        }

        private void Collect()
        {
            SoundManager.Instance?.Play(SoundId.XpCollect);
            VfxManager.Instance?.Play(VfxId.XpCollect, transform.position);
            if (PlayerExperience.Instance != null) PlayerExperience.Instance.AddXp(xpAmount);
            if (ObjectPooler.Instance != null) ObjectPooler.Instance.Return(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
