using Game.Management;
using Game.Room.Enemy;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Physics
{
    public class FieldOfViewEntitiesController : MonoBehaviour
    {
        [Inject] private PlayerManager _playerManager;
        [Inject] private Rigidbody2D _body;
        [Inject] private List<EnemyDamageHandler> _damageHandles;
        [Inject] private List<EnemyBase> _roomEnemies;

        public event Action<Collider2D> OnTriggerEnterEvent;
        public event Action<Collider2D> OnTriggerExitEvent;

        private List<FieldOfViewEntity> _entities = new();
        private Collider2D _trigger;

        private float _viewDistance = 300f;
        private const float PlayerCameraMaxViewDistance = 500f;



        private void Awake()
        {
            _trigger = GetComponent<Collider2D>();
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
            float maxDistanceToPlayer = PlayerCameraMaxViewDistance + _viewDistance;

            return Vector2.Distance(playerPos, enemyPos) < maxDistanceToPlayer;
        }

        private bool IsNonGuardEnemyInRange()
        {
            foreach (var enemy in _roomEnemies)
            {
                if (enemy == null)
                    continue;

                if (enemy.StateMachine.CurrentState is EnemyGuardStateBase)
                    continue;

                if (Vector2.Distance(enemy.transform.position, transform.position) > _viewDistance + 100)
                    continue;

                return true;
            }

            return false;
        }
    }
}
