using UnityEngine;
using UnityEngine.AI;

namespace Game
{
    [RequireComponent(typeof(Rigidbody2D), typeof(NavMeshAgent))]
    public class AgentForceReceiver : MonoBehaviour
    {
        [SerializeField] private float _mass = 1000;

        private NavMeshAgent _agent;

        private void Awake()
        {
            Init();
        }

        public void AddForce(Vector2 force)
        {
            _agent.velocity += (Vector3)(force / _mass);
        }

        private void Init()
        {
            _agent = GetComponent<NavMeshAgent>();
        }
    }
}
