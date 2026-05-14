using UnityEngine;
using SolomonCopy.Meta;
using SolomonCopy.Systems;

namespace SolomonCopy.Pickup
{
    public class RingPickup : MonoBehaviour
    {
        public float attractRadius = 2f;
        public float attractSpeed = 7f;
        public float collectRadius = 0.45f;

        [SerializeField] private RingInstance ringData;
        private Transform _player;

        public void SetRingData(RingInstance ring)
        {
            ringData = ring;
        }

        private void OnEnable()
        {
            if (_player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) _player = p.transform;
            }
        }

        private void Update()
        {
            if (_player == null || ringData == null) return;
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
            var inv = _player != null ? _player.GetComponent<RunRingInventory>() : null;
            if (inv != null) inv.AddRing(ringData);
            if (Systems.TopCenterMessageFeed.Instance != null && ringData != null)
                Systems.TopCenterMessageFeed.Instance.Show($"{ringData.rarity} Ring 획득");
            if (ObjectPooler.Instance != null) ObjectPooler.Instance.Return(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
