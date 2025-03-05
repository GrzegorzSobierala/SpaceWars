using System.Collections;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public abstract class EnemyCombatStateBase : EnemyStateBase
    {
        [Inject] private EnemyRoomAlarm _alarm;
        [Inject] private AlarmActivatorTimer _alarmActivatorTimer;
        
        private Coroutine _activationCoroutine;
        private float _activateTime = 0;

        protected override void OnEnterState()
        {
            //TryStartTurningOnAlarm();
        }

        protected override void OnExitState()
        {
            //StopTurningOnAlarm();
        }

        private void TryStartTurningOnAlarm()
        {
            if (_alarm.IsActivated)
            {
                _alarmActivatorTimer.Deactivate();
                return;
            }

            _alarmActivatorTimer.Activate();

            _activateTime = Time.time + _alarmActivatorTimer.ActivationTime;
            DiplayTimeLeft();
            _activationCoroutine = StartCoroutine(TryStartAlarm());
        }

        private IEnumerator TryStartAlarm()
        {
            yield return new WaitUntil(IsTimeForAlarmPassed);

            _alarm.ActivateAlarm();
            _alarmActivatorTimer.Deactivate();
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
            _alarmActivatorTimer.Deactivate();

            if (_activationCoroutine == null)
                return;

            StopCoroutine(_activationCoroutine);
            _activationCoroutine = null;
        }

        private void DiplayTimeLeft()
        {
            _alarmActivatorTimer.UpadteTimeLeft(_activateTime - Time.time);
        }
    }
}
