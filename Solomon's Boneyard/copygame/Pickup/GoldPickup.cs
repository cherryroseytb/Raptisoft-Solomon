using UnityEngine;
using SolomonCopy.Systems;
using SolomonCopy.Meta;

namespace SolomonCopy.Pickup
{
    public class GoldPickup : MonoBehaviour
    {
        public int goldAmount = 1;
        public float attractRadius = 2.2f;
        public float attractSpeed = 7f;
        public float collectRadius = 0.45f;

        private Transform _player;

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
            if (MetaProgressionManager.Instance != null)
                MetaProgressionManager.Instance.AddGold(goldAmount);
            else if (GameManager.Instance != null)
                GameManager.Instance.AddGold(goldAmount);

            if (ObjectPooler.Instance != null) ObjectPooler.Instance.Return(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
