using Game.Management;
using Zenject;
using UnityEngine;
using UnityEngine.AI;
using Game.Utility;
using System;
using Unity.Mathematics;

namespace Game.Room.Enemy
{
    public class CursorEnemyCombatState : EnemyCombatStateBase
    {
        [Inject] private CursorEnemyGun _gun;
        [Inject] private EnemyMovementBase _movement;
        [Inject] private PlayerManager _playerManager;
        [Inject] private NavMeshAgent _agent;

        [SerializeField] private float _maxRunRange = 1000;
        [SerializeField] private float _runAngle = 45;
        [SerializeField] private float _stopOnRunDistanceToRayHit = 100;
        [SerializeField] private float _runSpeedMulti = 1.5f;

        private Action _unSubAction;

        protected override void OnEnterState()
        {
            base.OnEnterState();

            _gun.StartShooting();
            _movement.SetAngularSpeedModifier(1.0f);

            FallowPlayer();

            _gun.SubscribeOnStartReload(RunFromPlayer);
            _gun.SubscribeOnStopReload(FallowPlayer);

            _unSubAction = () => _movement.UnsubscribeOnAchivedTarget(FallowPlayer);
            _movement.SubscribeOnChangedTarget(_unSubAction);
        }

        protected override void OnExitState()
        {
            base.OnExitState();

            _gun.StopShooting();
            _gun.StopAiming();
            _movement.StopMoving();

            _gun.UnsubscribeOnStartReload(RunFromPlayer);
            _gun.UnsubscribeOnStopReload(FallowPlayer);

            _movement.UnsubscribeOnChangedTarget(_unSubAction);
        }

        private void FallowPlayer()
        {
            _gun.StartAimingAt(_playerManager.PlayerBody.transform);
            _movement.StartGoingTo(_playerManager.PlayerBody.transform);
            _movement.SetSpeedModifier(_runSpeedMulti);
        }

        private void RunFromPlayer()
        {
            _gun.StopAiming();
            StartMovingAwayFromPlayer();
        }

        private void StartMovingAwayFromPlayer()
        {
            Vector2 rayDir = transform.up * _maxRunRange;

            float rotateMod = UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1;
            float rotateTargetPosAngle = _runAngle * rotateMod;

            rayDir = Utils.RotateVector(rayDir, rotateTargetPosAngle);
            Vector2 targetRayPos = (Vector2)transform.position + rayDir;

            NavMeshHit hit;
            _agent.Raycast(targetRayPos, out hit);
            Debug.DrawLine(transform.position, hit.position, Color.green);


            float agentSize = _agent.radius * 2;
            float stopDistMulti = math.remap(agentSize, _maxRunRange, 0, 1, hit.distance);
            float targetOffset = _stopOnRunDistanceToRayHit * stopDistMulti + agentSize;
            if (targetOffset > hit.distance)
            {
                _movement.StartGoingTo(transform.position);
            }
            else
            {
                Vector2 rayVector = (Vector2)hit.position - (Vector2)transform.position;
                Vector2 targetPos = (Vector2)hit.position - (rayVector.normalized * targetOffset);

                Debug.DrawLine(transform.position, targetPos, Color.red);
                _movement.StartGoingTo(targetPos);
            }

            _movement.SubscribeOnAchivedTarget(FallowPlayer);

            _movement.SetSpeedModifier(_runSpeedMulti);
        }
    }
}
