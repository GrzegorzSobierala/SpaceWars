using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class CustomEnemyTarget : MonoBehaviour
    {
        [Inject] private EnemyMovementBase _enemyMovement;
        [Inject] private Rigidbody2D _body;

        [SerializeField] private List<Transform> _enemyTargets;
        private Transform _currentEnemyTarget;

        public List<Transform> EnemyTargets => _enemyTargets;

        public Transform CurrentEnemyTarget => _currentEnemyTarget;

        public void SetEnemyTargetPoint(Transform enemyTarget)
        {
            _currentEnemyTarget = enemyTarget;
        }
    }
}