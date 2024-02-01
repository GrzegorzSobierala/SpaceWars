using System.Collections;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public abstract class EnemyCombatStateBase : EnemyStateBase
    {
        [Inject] private EnemyRoomAlarm _alarm;

        [SerializeField] private TextMeshPro _textMesh;
        [SerializeField] private float _activatingTime = 8;

        private Coroutine _activationCoroutine;
        private float _activateTime = 0;

        protected override void OnEnterState()
        {
            TryStartTurningOnAlarm();
        }

        protected override void OnExitState()
        {
            StopTurningOnAlarm();
        }

        private void TryStartTurningOnAlarm()
        {
            if (_alarm.IsActivated)
            {
                _textMesh.gameObject.SetActive(false);
                return;
            }

            _textMesh.gameObject.SetActive(true);

            _activateTime = Time.time + _activatingTime;
            DiplayTimeLeft();
            _activationCoroutine = StartCoroutine(TryStartAlarm());
        }

        private IEnumerator TryStartAlarm()
        {
            yield return new WaitUntil(IsTimeForAlarmPassed);

            _alarm.ActivateAlarm();
            _textMesh.gameObject.SetActive(false);
            _activationCoroutine = null;
        }

        private bool IsTimeForAlarmPassed()
        {
            if(_alarm.IsActivated)
            {
                StopTurningOnAlarm();
                return false;
            }

            DiplayTimeLeft();
            return _activateTime <= Time.time;
        }

        private void StopTurningOnAlarm()
        {
            _textMesh.gameObject.SetActive(false);

            if (_activationCoroutine == null)
                return;

            StopCoroutine(_activationCoroutine);
            _activationCoroutine = null;
        }

        private void DiplayTimeLeft()
        {
            _textMesh.text = (_activateTime - Time.time ).ToString("0");
        }
    }
}
