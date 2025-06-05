using Game.Combat;
using Game.Management;
using Game.Utility;
using Game.Utility.Globals;
using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Game.Room.Enemy
{
    public class BasicEnemyGun : EnemyGunBase
    {
        public event Action OnStartReload;
        public event Action OnStopReload;

        [Inject] private PlayerManager _playerManager;
        [Inject] private EnemyManager _enemyManager;

        [SerializeField, AutoFill] private Transform _gunShootPoint;
        [SerializeField] private ShootableObjectBase _bulletPrototype;
        [Space, Header("Aim")]
        [SerializeField] private float _gunTraverse = 45f;
        [SerializeField] private float _rotateSpeed = 100f;
        [SerializeField] private float _aimedAngle = 4;
        [SerializeField] private float _aimRange = 500f;
        [SerializeField] private float _aimFollowTime = 2f;
        [SerializeField] LostTargetMode _lostTargetMode;
        [SerializeField, ShowIf(nameof(_lostTargetMode), LostTargetMode.Search)]
        private float _searchSpeedMulti = 0.25f;
        [SerializeField, ShowIf(nameof(_lostTargetMode), LostTargetMode.SearchRandom)]
        private float _searchRandomSpeedMulti = 0.25f;
        [Space, Header("Shoot")]
        [SerializeField] private float _shotInterval = 0.5f;
        [SerializeField] private int _magCapacity = 5;
        [SerializeField] private float _reloadTime = 7f;
        [SerializeField] private float _shootAtMaxDistanceMutli = 0.7f;
        [Space]
        [SerializeField] private UnityEvent _onStartReload;
        [SerializeField] private float _beforeReloadEventTime = 0.5f;
        [SerializeField] private UnityEvent _onBeforeReloaded;
        [SerializeField] private UnityEvent _onReloaded;

        private Coroutine _reloadCoroutine;
        private float _lastShootTime = 0f;
        private float _endReloadTime = 0f;
        private int _currentNotReservedMagAmmo = 0;
        private int _currenaMagAmmo = 0;
        private bool _wasOnBeforeReloadedCalled = false;
        private ContactFilter2D _contactFilter;
        private OscillateController _oscillateCont = new();
        private float _randomSearchTarget;

        public float GunTraverse => _gunTraverse;

        protected override void Awake()
        {
            base.Awake();

            _currentNotReservedMagAmmo = _magCapacity;
            _currenaMagAmmo = _magCapacity;
            Initalize();
        }

        public override void Prepare()
        {
            _lastShootTime = Time.time;
        }

        protected override void OnAimingAt(Transform target)
        {
            base.OnAimingAt(target);

            if (IsKnowWherePlayerIs(target.position, _aimRange, _aimFollowTime, _contactFilter))
            {
                Aim(target.position, _gunTraverse, _rotateSpeed, _aimedAngle, true);
            }
            else
            {
                LostTargetAction(_lostTargetMode);
            }
        }

        protected override void OnAimingIdle()
        {
            base.OnAimingIdle();

            LostTargetAction(_lostTargetMode);
        }

        protected override void OnStopAiming()
        {
            base.OnStopAiming();
        }

        protected override void OnShooting()
        {
            base.OnShooting();

            TryShootOne();
        }

        protected override void OnStopShooting()
        {
            base.OnStopShooting();

            StartReloading();
        }

        protected override void OnShoot()
        {
            FireProjectile();
        }

        private void Initalize()
        {
            _contactFilter = new ContactFilter2D
            {
                useTriggers = false,
                layerMask = LayerMask.GetMask(Layers.Player, Layers.Obstacle, Layers.SmallObstacle),
                useLayerMask = true,
            };
        }

        private void FireProjectile()
        {
            GameObject damageDealer = _body.gameObject;
            Transform parent = _enemyManager.transform;

            ShootableObjectBase bulletInstance = _bulletPrototype.CreateCopy(damageDealer, parent);
            bulletInstance.Shoot(_body, _gunShootPoint);

            if(bulletInstance is FlakBullet flakBullet)
            {
                flakBullet.SetTarget(_playerManager.PlayerBody.position);
            }

            if (_currenaMagAmmo <= 0)
                Debug.LogWarning("No ammo left in the current mag!");

            _currenaMagAmmo--;

            if (_currenaMagAmmo == 0)
            {
                StartReloading();
            }
        }

        private void TryShootOne()
        {
            if (Time.time - _lastShootTime < _shotInterval || _currentNotReservedMagAmmo <= 0)
                return;

            if (!IsAimedAtPlayer)
                return;

            float targetDistance = Vector2.Distance(_playerManager.PlayerBody.position, 
                transform.position);
            if (targetDistance > _bulletPrototype.MaxDistance * _shootAtMaxDistanceMutli)
                return;

            if (!TryShoot())
                return;

            _lastShootTime = Time.time;
            _currentNotReservedMagAmmo--;
        }

        private bool TryReload()
        {
            if (!_wasOnBeforeReloadedCalled && Time.time > _endReloadTime - _beforeReloadEventTime)
            {
                _onBeforeReloaded?.Invoke();
                _wasOnBeforeReloadedCalled = true;
            }

            if (Time.time < _endReloadTime)
                return false;

            Reload();
            return true;
        }

        private void Reload()
        {
            _currentNotReservedMagAmmo = _magCapacity;
            _currenaMagAmmo = _magCapacity;
            OnStopReload?.Invoke();
            _onReloaded?.Invoke();
            _wasOnBeforeReloadedCalled = false;
        }

        private void StartReloading()
        {
            if (_reloadCoroutine != null)
            {
                Debug.Log($"Reloading is already in progress. Time left {Time.time - _endReloadTime}");
                return;
            }

            _endReloadTime = Time.time + _reloadTime;
            OnStartReload?.Invoke();
            _reloadCoroutine = StartCoroutine(ReloadingMag());
        }

        private IEnumerator ReloadingMag()
        {
            _onStartReload?.Invoke();

            yield return new WaitUntil(TryReload);

            _reloadCoroutine = null;
        }

        protected void LostTargetAction(LostTargetMode lostTargetMode)
        {
            switch (lostTargetMode)
            {
                case LostTargetMode.Stay:
                    LostTargetStay();
                    break;
                case LostTargetMode.Forward:
                    LostTargetActionForward();
                    break;
                case LostTargetMode.Search:
                    LostTargetActionSearch();
                    break;
                case LostTargetMode.SearchRandom:
                    LostTargetActionSearchRandom();
                    break;
                default:
                    Debug.LogError("Switch error");
                    break;
            }
        }

        private void LostTargetStay()
        {
        }

        private void LostTargetActionForward()
        {
            if (CurrentGunRot == 0)
                return;

            Aim(0, _gunTraverse, _rotateSpeed, _aimedAngle, false);
        }

        private void LostTargetActionSearch()
        {
            float searchRotSpeed = _rotateSpeed * _searchSpeedMulti;
            float targetAngle = _oscillateCont.OscillateRotThisThisFrame(_gunTraverse,
                searchRotSpeed, CurrentGunRot);

            Aim(targetAngle, _gunTraverse, searchRotSpeed, _aimedAngle, false);
        }

        private void LostTargetActionSearchRandom()
        {
            if (0.01f > Mathf.Abs(CurrentGunRot - _randomSearchTarget) || _randomSearchTarget == 0)
            {
                float side = Mathf.Sign(_randomSearchTarget * -1);

                float min = _gunTraverse / 2 - Mathf.Abs(CurrentGunRot);
                _randomSearchTarget = UnityEngine.Random.Range(min, _gunTraverse/2) * side;
            }
            
            float searchRotSpeed = _rotateSpeed * _searchRandomSpeedMulti;
            Aim(_randomSearchTarget, _gunTraverse, searchRotSpeed, _aimedAngle, false);
        }
    }
}