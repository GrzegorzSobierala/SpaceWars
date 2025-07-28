using Game.Management;
using Game.Player.Control;
using Game.Room.Enemy;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Zenject;

namespace Game.Physics
{
    public class FieldOfViewEntitiesController : MonoBehaviour
    {
        public event Action<Collider2D> OnTriggerEnterEvent;
        public event Action<Collider2D> OnTriggerExitEvent;

        [Inject] private PlayerManager _playerManager;
        [Inject] private Rigidbody2D _body;
        [Inject] private List<EnemyBase> _roomEnemies;
        [Inject] private CursorCamera _cursorCamera;
        [Inject] private GlobalAssets _globalAssets;
        [Inject] private FieldOfViewSystem _fovSystem;
        [Inject] private EnemyBase _enemyBase;

        private List<FieldOfViewEntity> _entities = new();
        private Collider2D _trigger;

        private Dictionary<EnemyBase, EnemySeeEnemyArrow> _enemiesLine = new();
        private HashSet<EnemyBase> _thisFrameEnemiesWithLine = new();
        private List<EnemyBase> _enemiesToRemoveFomDic = new();

        private float _enemyEnableDistance;
        private float _enemyEnableSqrMagnitude;
        private float _playerEnableDistance;

        private const float _SAFE_DISTANCE_ADD = 10f;

        private FieldOfViewSystemFacade FovFacade => _fovSystem.Facade;

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
            UpdateEnableEntities();
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

            FovFacade.AddController(this, entity);
        }

        public void UnsubscribeTriggerEvents(FieldOfViewEntity entity)
        {
            OnTriggerEnterEvent -= entity.TriggerEnter2D;
            OnTriggerExitEvent -= entity.TriggerExit2D;

            _entities.Remove(entity);

            FovFacade.RemoveController(this, entity);
        }

        private void UpdateEnableEntities()
        {
            if (IsPlayerInRange() || IsNonGuardEnemyInRange())
            {
                if (_trigger.enabled)
                    return;

                foreach (var entity in _entities)
                {
                    entity.EnableEntity();
                }
                _trigger.enabled = true;
            }
            else
            {
                if (!_trigger.enabled)
                    return;

                foreach (var entity in _entities)
                {
                    entity.DisableEntity();
                }
                _trigger.enabled = false;
            }
        }

        private bool IsPlayerInRange()
        {
            Vector2 playerPos = _playerManager.PlayerBody.position;
            Vector2 enemyPos = _body.position;

            return Vector2.Distance(playerPos, enemyPos) < _playerEnableDistance;
        }

        private bool IsNonGuardEnemyInRange()
        {
            for (int i = 0; i < _roomEnemies.Count; i++)
            {
                EnemyBase enemy = _roomEnemies[i];

                if (enemy == null)
                    continue;

                if (enemy.StateMachine.CurrentState is EnemyGuardStateBase)
                    continue;

                if (Vector2.SqrMagnitude(transform.position - enemy.transform.position) > _enemyEnableSqrMagnitude)
                    continue;

                return true;
            }

            return false;
        }
        
        private void SetEnableDistance()
        {
            Vector2 screenMid = new(Screen.width / 2, Screen.height / 2);
            Vector2 screenMidWorld = _cursorCamera.ScreanPositionOn2DIntersection(screenMid);
            Vector2 screnTopRight = new(Screen.width, Screen.height);
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

            _playerEnableDistance = largestFovDistance + midToTopRightDistanceWorld + _SAFE_DISTANCE_ADD;
            _enemyEnableDistance = largestFovDistance;
            _enemyEnableSqrMagnitude = _enemyEnableDistance * _enemyEnableDistance;
        }

        public void OnEnemySeeEnemy(IGuardStateDetectable detectable)
        {
            if(!_thisFrameEnemiesWithLine.Contains(detectable.Enemy))
            {
                _thisFrameEnemiesWithLine.Add(detectable.Enemy);
            }

            if (!_enemiesLine.ContainsKey(detectable.Enemy))
            {
                EnemySeeEnemyArrow line = Instantiate(_globalAssets.EnemySeeEnemyLine, transform);
                _enemiesLine.Add(detectable.Enemy, line);
            }

            _enemiesLine[detectable.Enemy].TransformArrow(_enemyBase, detectable.Enemy);
        }

        public void OnPostEnemySeeEnemy()
        {
            foreach (var enemy in _enemiesLine)
            {
                if (!_thisFrameEnemiesWithLine.Contains(enemy.Key))
                {
                    _enemiesToRemoveFomDic.Add(enemy.Key);
                }
            }
            
            foreach (var enemy in _enemiesToRemoveFomDic)
            {
                Destroy(_enemiesLine[enemy].gameObject);
                _enemiesLine.Remove(enemy);
            }

            _thisFrameEnemiesWithLine.Clear();
            _enemiesToRemoveFomDic.Clear();
        }
    }
}
