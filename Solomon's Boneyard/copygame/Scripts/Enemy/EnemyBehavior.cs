using UnityEngine;
using SolomonCopy.Systems;

namespace SolomonCopy.Enemy
{
    public enum EnemyState { Idle, Chase, Attack, Charge, Flee }

    public abstract class EnemyBehavior : MonoBehaviour
    {
        protected EnemyController controller;
        protected Rigidbody2D rb;
        protected Transform player;

        public EnemyState currentState = EnemyState.Chase;

        protected virtual void Awake()
        {
            controller = GetComponent<EnemyController>();
            rb = GetComponent<Rigidbody2D>();
        }

        protected virtual void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        protected virtual void FixedUpdate()
        {
            if (player == null) return;
            UpdateState();
        }

        protected abstract void UpdateState();

        protected void MoveTowardsPlayer(float speedMultiplier = 1f)
        {
            Vector2 dir = ((Vector2)player.position - rb.position).normalized;
            rb.MovePosition(rb.position + dir * controller.moveSpeed * speedMultiplier * Time.fixedDeltaTime);
        }

        protected void MoveAwayFromPlayer(float speedMultiplier = 1f)
        {
            Vector2 dir = (rb.position - (Vector2)player.position).normalized;
            rb.MovePosition(rb.position + dir * controller.moveSpeed * speedMultiplier * Time.fixedDeltaTime);
        }

        protected GameObject SpawnObject(GameObject prefab, Vector3? position = null, Quaternion? rotation = null)
        {
            var pos = position ?? transform.position;
            var rot = rotation ?? Quaternion.identity;
            return ObjectPooler.Instance != null
                ? ObjectPooler.Instance.Spawn(prefab, pos, rot)
                : Instantiate(prefab, pos, rot);
        }
    }
}
