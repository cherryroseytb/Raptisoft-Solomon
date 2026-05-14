using UnityEngine;

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
    }
}
