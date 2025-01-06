using Game.Utility;
using Game.Utility.Globals;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class BigLaserEnemyGun : EnemyGunBase
    {
        [Inject] private EnemyBase _enemy;

        [SerializeField] private float _rotationSpeed = 50;
        [SerializeField] private float _aimedAngle = 4;
        [SerializeField] private float _aimRange = 500f;
        [SerializeField] private float _aimFallowTime = 2f;
        [Space]
        [SerializeField] private LaserBeam _laserBeam;
        [SerializeField] private Transform _gunTrans;
        [SerializeField] private LayerMask _blockAimLayerMask;

        private ContactFilter2D _contactFilter;

        protected override void Awake()
        {
            base.Awake();

            Initalize();
        }

        public override void Prepare()
        {
            _laserBeam.StopFire();
        }

        protected override void OnAimingAt(Transform target)
        {
            base.OnAimingAt(target);

            if(IsKnowWherePlayerIs(target.position, _aimRange, _aimFallowTime, 
                _contactFilter))
            {
                Aim(target.position, 360, _rotationSpeed, _aimedAngle, true);
            }
            else
            {
                Vector2 lookForwardPoint = _enemy.transform.up + _enemy.transform.position;
                Aim(lookForwardPoint, 360, _rotationSpeed, _aimedAngle, false);
            }

            VerticalRotate(_gunTrans, _rotationTrans, target.position);
        }

        protected override void OnShooting()
        {
            if (!IsAimedAtPlayer)
                return;

            if(_laserBeam.TryStartFire())
            {
                OnShoot?.Invoke();
            }
        }

        private void Initalize()
        {
            _contactFilter = new ContactFilter2D
            {
                useTriggers = false,
                layerMask = _blockAimLayerMask,
                useLayerMask = true,
            };
        }
    }
}
