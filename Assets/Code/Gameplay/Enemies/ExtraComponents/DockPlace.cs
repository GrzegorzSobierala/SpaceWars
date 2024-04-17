using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.Room.Enemy
{
    public class DockPlace : MonoBehaviour
    {
        [SerializeField] private Transform _dockingPoint;
        [SerializeField] private float _dockingTime = 4;

        private IDocking _occupand;
        private Coroutine _currentCoroutine;

        public Transform DockingPoint => _dockingPoint;

        private void Awake()
        {
            if (_dockingPoint == null)
            {
                _dockingPoint = transform;
            }
        }

        public void StartDocking(IDocking dockingObject)
        {
            if(!CanDock())
                return;

            _occupand = dockingObject;
            StartMovingOperation(Docking());
            _occupand.OnStartDocking();
        }

        public void StartUnDocking(IDocking dockingObject)
        {
            if(!CanUnDock(dockingObject))
                return;

            StartMovingOperation(UnDocking());
            _occupand.OnStartUnDocking();
        }

        private IEnumerator Docking()
        {
            float endTime = Time.time + _dockingTime;
            Vector2 startPos = _occupand.Body.position;
            float startRot = _occupand.Body.rotation;
            Vector2 endPos = _dockingPoint.position;
            float endRot = _dockingPoint.eulerAngles.z;

            yield return MovingOperation(startPos, startRot, endPos, endRot, endTime);

            _occupand.Body.MovePosition(_dockingPoint.position);
            _occupand.Body.MoveRotation(_dockingPoint.eulerAngles.z);
            Dock();
        }

        private IEnumerator UnDocking()
        {
            float endTime = Time.time + _dockingTime;
            Vector2 startPos = _dockingPoint.position;
            float startRot = _dockingPoint.eulerAngles.z;
            Vector2 leaveVector = transform.up * _occupand.DistanceBeforeDock;
            Vector2 endPos = _occupand.Body.position + leaveVector;
            float endRot = _occupand.Body.rotation;

            yield return MovingOperation(startPos, startRot, endPos, endRot, endTime);

            _occupand.Body.MovePosition(endPos);
            _occupand.Body.MoveRotation(endRot);
            UnDock();
        }

        private IEnumerator MovingOperation(Vector2 startPos, float startRot,
            Vector2 endPos, float endRot, float endTime)
        {
            while (endTime > Time.time)
            {
                float currentTime = Time.time - (endTime - _dockingTime);
                float t = math.remap(0, _dockingTime, 0, 1, currentTime);
                Vector2 targetPos = Vector2.Lerp(startPos, endPos, t);
                float targetRot = Mathf.LerpAngle(startRot, endRot, t);

                _occupand.Body.MovePosition(targetPos);
                _occupand.Body.MoveRotation(targetRot);

                yield return null;
            }
        }

        private void StartMovingOperation(IEnumerator operation)
        {
            EndCurrentOperation();

            _currentCoroutine = StartCoroutine(operation);
        }

        private void Dock()
        {
            EndCurrentOperation();
            _occupand.OnEndDocking();
        }

        private void UnDock()
        {
            EndCurrentOperation();

            _occupand.OnEndUnDocking();
            _occupand = null;
        }

        private bool CanDock()
        {
            return _occupand == null;
        }

        private bool CanUnDock(IDocking objectToCheck)
        {
            if(_occupand == null)
            {
                Debug.Log("Nothing is docked");
                return false;
            }

            if (objectToCheck != _occupand)
            {
                Debug.Log($"This object isnt docked here. Current: {_occupand}, to check {objectToCheck}");
                return false;
            }

            return true;
        }

        private void EndCurrentOperation()
        {
            if(_currentCoroutine == null)
                return;

            StopCoroutine( _currentCoroutine );
            _currentCoroutine = null;
        }
    }
}
