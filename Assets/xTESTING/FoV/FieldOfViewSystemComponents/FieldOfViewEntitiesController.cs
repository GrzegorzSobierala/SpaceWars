using Game.Management;
using Game.Player.Control;
using Game.Room.Enemy;
using Game.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Zenject;

namespace Game.Physics
{
    public class FieldOfViewEntitiesController : MonoBehaviour
    {
        [Inject] private PlayerManager _playerManager;
        [Inject] private Rigidbody2D _body;
        [Inject] private List<EnemyDamageHandler> _damageHandles;
        [Inject] private List<EnemyBase> _roomEnemies;
        [Inject] private CursorCamera _cursorCamera;

        public event Action<Collider2D> OnTriggerEnterEvent;
        public event Action<Collider2D> OnTriggerExitEvent;

        private List<FieldOfViewEntity> _entities = new();
        private Collider2D _trigger;

        //private float _viewDistance = 300f;
        //private const float PlayerCameraMaxViewDistance = 500f;
        private float _enemyEnableDistance;
        private float _playerEnableDistance;
        private float _saveDistaneAdd = 10f;

        private void Awake()
        {
            _trigger = GetComponent<Collider2D>();
        }

        private void Start()
        {
            SetEnableDistance();
        }

        private void Update()
        {
            if (IsPlayerInRange() || IsNonGuardEnemyInRange())
            {
                if(!_trigger.enabled)
                {
                    foreach (var entity in _entities)
                    {
                        entity.EnableEntity();
                    }
                    _trigger.enabled = true;
                }

            }
            else
            {
                if (_trigger.enabled)
                {
                    foreach (var entity in _entities)
                    {
                        entity.DisableEntity();
                    }
                    _trigger.enabled = false;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            OnTriggerEnterEvent?.Invoke(collision);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if(GameManager.IsGameQuitungOrSceneUnloading(gameObject))
                return;

            if (!_trigger.enabled)
                return;

            OnTriggerExitEvent?.Invoke(collision);
        }

        public void SubscribeTriggerEvents(FieldOfViewEntity entity)
        {
            OnTriggerEnterEvent += entity.TriggerEnter2D;
            OnTriggerExitEvent += entity.TriggerExit2D;

            _entities.Add(entity);

            SetEnableDistance();
        }

        public void UnsubscribeTriggerEvents(FieldOfViewEntity entity)
        {
            OnTriggerEnterEvent -= entity.TriggerEnter2D;
            OnTriggerExitEvent -= entity.TriggerExit2D;

            _entities.Remove(entity);
        }

        private bool IsPlayerInRange()
        {
            Vector2 playerPos = _playerManager.PlayerBody.position;
            Vector2 enemyPos = _body.position;

            return Vector2.Distance(playerPos, enemyPos) < _playerEnableDistance;
        }

        private bool IsNonGuardEnemyInRange()
        {
            Profiler.BeginSample("IsNonGuardEnemyInRange");
            for (int i = 0; i < _roomEnemies.Count; i++)
            {
                var enemy = _roomEnemies[i];
                if (enemy == null)
                    continue;

                if (enemy.StateMachine.CurrentState is EnemyGuardStateBase)
                    continue;

                if (Vector2.Distance(enemy.transform.position, transform.position) > _enemyEnableDistance)
                    continue;

                Profiler.EndSample();
                return true;
            }

            Profiler.EndSample();
            return false;
        }
        
        private void SetEnableDistance()
        {
            Vector2 screenMid = new Vector2(Screen.width / 2, Screen.height / 2);
            Vector2 screenMidWorld = _cursorCamera.ScreanPositionOn2DIntersection(screenMid);
            Vector2 screnTopRight = new Vector2(Screen.width, Screen.height);
            Vector2 screenTopRightWorld = _cursorCamera.ScreanPositionOn2DIntersection(screnTopRight);

            float midToTopRightDistanceWorld = Vector2.Distance(screenMidWorld, screenTopRightWorld);

            float largestFovDistance = 0;
            foreach (var entity in _entities)
            {
                float posDistance = Vector2.Distance(entity.transform.position, transform.position);

                float entityFovDistance = posDistance + entity.ViewDistance;

                if (entityFovDistance > largestFovDistance)
                    largestFovDistance = entityFovDistance;
            }

            _playerEnableDistance = largestFovDistance + midToTopRightDistanceWorld + _saveDistaneAdd;
            _enemyEnableDistance = largestFovDistance;
        }
    }
}
