using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class PatrolController : MonoBehaviour
    {
        [Inject] private EnemyMovementBase _enemyMovement;
        [Inject] private Rigidbody2D _body;

        [SerializeField] private List<Transform> _guardPoints;

        private Transform _currentGuardPoint;
        private bool _isPatroling = false;
        private bool _blockStop = false;
        private bool _isGoingUpList = false;

        private void Awake()
        {
            Initialize();
        }

        public void StartPatroling()
        {
            if (_isPatroling)
            {
                Debug.Log("Cant start patroling coz patroling already. Returning");
                return;
            }

            _isPatroling = true;
            gameObject.SetActive(true);
            _enemyMovement.SubscribeOnAchivedTarget(GoToNextGuardPoint);
            _enemyMovement.SubscribeOnChangedTarget(StopPatroling);
            GoToGuardPoint(_currentGuardPoint);
        }

        private void StopPatroling()
        {
            if (_blockStop)
                return;

            if (!_isPatroling)
            {
                Debug.Log("Cant stop patroling coz patroling already. Returning");
                return;
            }

            _isPatroling = false;
            _enemyMovement.UnsubscribeOnAchivedTarget(GoToNextGuardPoint);
            _enemyMovement.UnsubscribeOnChangedTarget(StopPatroling);
            gameObject.SetActive(false);
        }

        private void Initialize()
        {
            if (_guardPoints.Count < 2)
            {
                Debug.LogError("There need to be at least 2 guard points");
                return;
            }

            _currentGuardPoint = GetNearestGuardPoint();
            gameObject.SetActive(false);
        }

        private Transform GetNearestGuardPoint()
        {
            float lowestDistance = float.PositiveInfinity;
            int indexOfLowestDistance = -1;
            for (int i = 0; i < _guardPoints.Count; i++)
            {
                Vector2 pos = _guardPoints[i].transform.position;
                float distance = Vector2.Distance(pos, _body.position);
                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                    indexOfLowestDistance = i;
                }
            }

            return _guardPoints[indexOfLowestDistance];
        }

        private void GoToGuardPoint(Transform guardPoint)
        {
            _currentGuardPoint = guardPoint;

            _blockStop = true;
            _enemyMovement.StartGoingTo(guardPoint.position);
            _blockStop = false;
        }

        private void GoToNextGuardPoint()
        {
            int currentIndex = _guardPoints.IndexOf(_currentGuardPoint);
            int nextIndex;

            if (_isGoingUpList)
            {
                if(currentIndex + 1 > _guardPoints.Count - 1)
                {
                    _isGoingUpList = false;
                    nextIndex = currentIndex - 1;
                }
                else
                {
                    nextIndex = currentIndex + 1;
                }
            }
            else
            {
                if (currentIndex - 1 < 0)
                {
                    _isGoingUpList = true;
                    nextIndex = currentIndex + 1;
                }
                else
                {
                    nextIndex = currentIndex - 1;
                }
            }

            GoToGuardPoint(_guardPoints[nextIndex]);
        }
    }
}
