using System.Collections.Generic;
using UnityEngine;

namespace Game.Room.Enemy
{
    [RequireComponent(typeof(EnemyGunBase))]
    public class ShootBarrelChanger : MonoBehaviour
    {
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private List<Transform> _changePoints;

        private EnemyGunBase _gunBase;
        private int _currentPointIndex = 0;

        private void Awake()
        {
            _gunBase = GetComponent<EnemyGunBase>();
            _gunBase.OnBeforeShootEvent += ChangeShootPoint;
        }

        private void OnDestroy()
        {
            _gunBase.OnBeforeShootEvent -= ChangeShootPoint;
        }

        private void ChangeShootPoint()
        {
            if(_changePoints.Count == 0)
            {
                Debug.LogError("No change points for shoot barrel changer");
                return;
            }

            if (_currentPointIndex >= _changePoints.Count)
            {
                _currentPointIndex = 0;
            }

            _shootPoint.SetParent(_changePoints[_currentPointIndex], false);

            _currentPointIndex++;
            if (_currentPointIndex >= _changePoints.Count)
            {
                _currentPointIndex = 0;
            }
        }
    }
}
