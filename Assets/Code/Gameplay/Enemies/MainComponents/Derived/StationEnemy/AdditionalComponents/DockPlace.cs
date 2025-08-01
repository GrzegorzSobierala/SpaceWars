using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Game.Utility;
using System;
using Game.Management;

namespace Game.Room.Enemy
{
    public class DockPlace : MonoBehaviour
    {
        public event Action<IDocking> OnDock;
        public event Action<IDocking> OnUndock;

        [SerializeField] private Transform _dockingPoint;
        [SerializeField] private float _dockingTime = 4;
        [SerializeField] private float _rotSpeedMulti = 1.3f;

        private IDocking _occupand;
        private Coroutine _currentCoroutine;

        public Transform DockingPoint => _dockingPoint;

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            if (GameManager.IsGameQuitungOrSceneUnloading(gameObject))
                return;

            if(_occupand != null)
            {
                _occupand.OnDockDestroy();
            }
        }

        public void StartDocking(IDocking dockingObject)
        {
            if(!CanDock())
                return;

            _occupand = dockingObject;
            _occupand.OnObjectDestroy += OnOccupodndDestroyed;
            StartMovingOperation(Docking());
            _occupand.OnStartDocking();
        }

        public void StartUnDocking(IDocking dockingObject)
        {
            if(!CanUndock(dockingObject))
                return;

            StartMovingOperation(UnDocking());
            _occupand.OnStartUnDocking();
        }

        private void Initialize()
        {
            if (_dockingPoint == null)
            {
                _dockingPoint = transform;
            }
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
            Undock();
        }

        private IEnumerator MovingOperation(Vector2 startPos, float startRot,
            Vector2 endPos, float endRot, float endTime)
        {
            float angleToRot = Mathf.DeltaAngle(startRot, endRot);
            float rotMulti = Mathf.Clamp(Mathf.Abs(angleToRot) / 180, float.Epsilon, float.MaxValue);

            while (endTime > Time.time)
            {
                float currentTime = Time.time - (endTime - _dockingTime);
                float t = math.remap(0, _dockingTime, 0, 1, currentTime);
                float posT = LerpX.GetSmooth(ref t, LerpX.SmoothType.Smootherstep);

                float rotT = math.remap(0, _dockingTime * rotMulti, 0, 1, currentTime);
                rotT = Mathf.Clamp(rotT * _rotSpeedMulti, 0, 1);
                rotT = LerpX.GetSmooth(ref rotT, LerpX.SmoothType.Smootherstep);

                Vector2 targetPos = Vector2.Lerp(startPos, endPos, posT);
                float targetRot = Mathf.LerpAngle(startRot, endRot, rotT);

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
            OnDock?.Invoke(_occupand);
        }

        private void Undock()
        {
            EndCurrentOperation();

            _occupand.OnEndUnDocking();
            IDocking leaver = _occupand;
            _occupand = null;
            OnUndock.Invoke(leaver);
        }

        private bool CanDock()
        {
            return _occupand == null;
        }

        private bool CanUndock(IDocking objectToCheck)
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

        private void OnOccupodndDestroyed()
        {
            EndCurrentOperation() ;
            OnUndock.Invoke(_occupand);
            _occupand = null;
        }
    }
}
