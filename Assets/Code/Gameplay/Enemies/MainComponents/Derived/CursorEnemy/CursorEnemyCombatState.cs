using Game.Management;
using Zenject;
using UnityEngine;
using UnityEngine.AI;
using Game.Utility;
using Unity.Mathematics;
using System.Collections;

namespace Game.Room.Enemy
{
    public class CursorEnemyCombatState : EnemyCombatStateBase
    {
        [Inject] private CursorEnemyGun _gun;
        [Inject] private EnemyMovementBase _movement;
        [Inject] private PlayerManager _playerManager;
        [Inject] private NavMeshAgent _agent;
        // [Inject] private CustomEnemyTarget _target;

        [SerializeField] private float _maxRunRange = 1000;
        [SerializeField] private float _runAngle = 45;
        [SerializeField] private float _stopOnRunDistanceToRayHit = 100;
        [SerializeField] private float _followSpeedMulti = 1.5f;
        [SerializeField] private float _followAngularSpeedMulti = 1.5f;
        [SerializeField] private float _runSpeedMulti = 1.5f;
        [SerializeField] private float _runAngularSpeedMulti = 1.5f;
        [SerializeField] private float _spotPlayerRange = 750;
        [SerializeField] private float _maxSpotPlayerRange = float.PositiveInfinity;
        [SerializeField] private float _spotRangeIncreasePerSec = 5;

        private float _enterStateTime;

        public float CurrentSpotRange
        {
            get
            {
                float activeStateTime = Time.time - _enterStateTime;
                float range = _spotPlayerRange + (activeStateTime * _spotRangeIncreasePerSec);
                return Mathf.Clamp(range, 0, _maxSpotPlayerRange);
            }
        }

        protected override void OnEnterState()
        {
            base.OnEnterState();

            _enterStateTime = Time.time;
            _gun.StartShooting();
            _movement.SetAngularSpeedModifier(1.0f);

            StartCoroutine(TryFallowPlayer());
        }

        protected override void OnExitState()
        {
            base.OnExitState();

            _gun.StopShooting();
            _gun.StopAiming();
            _movement.StopMoving();

            _gun.UnsubscribeOnStartReload(RunFromPlayer);
            _gun.UnsubscribeOnStopReload(FallowPlayer);
        }

        private void FallowPlayer()
        {
            _movement.UnsubscribeOnAchivedTarget(FallowPlayer);
            _gun.UnsubscribeOnStopReload(FallowPlayer);

            _gun.StartAimingAt(_playerManager.PlayerBody.transform);
            _movement.StartGoingTo(_playerManager.PlayerBody.transform);
            // _movement.StartGoingTo(_target.SetEnemyTargetPoint.transform);
            _movement.SetSpeedModifier(_followSpeedMulti);
            _movement.SetAngularSpeedModifier(_followAngularSpeedMulti);


            _gun.UnsubscribeOnStartReload(RunFromPlayer);
            _gun.SubscribeOnStartReload(RunFromPlayer);
        }

        private void RunFromPlayer()
        {
            _gun.UnsubscribeOnStartReload(RunFromPlayer);

            _gun.UnsubscribeOnStopReload(FallowPlayer);
            _gun.SubscribeOnStopReload(FallowPlayer);
            _movement.SetSpeedModifier(_runSpeedMulti);
            _movement.SetAngularSpeedModifier(_runAngularSpeedMulti);

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

            _movement.UnsubscribeOnAchivedTarget(FallowPlayer);
            _movement.SubscribeOnAchivedTarget(FallowPlayer);

            _movement.SetSpeedModifier(_runSpeedMulti);
            _movement.SetAngularSpeedModifier(_runAngularSpeedMulti);
        }

        private IEnumerator TryFallowPlayer()
        {
            yield return new WaitUntil(IsPlayerInSpotRange);

            FallowPlayer();
        }

        private bool IsPlayerInSpotRange()
        {
            Vector2 playerPos = _playerManager.PlayerBody.position;
            return Vector2.Distance(transform.position, playerPos) < CurrentSpotRange;
        }
    }
}
