using Game.Management;
using Game.Utility;
using Game.Utility.Globals;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace Game.Room.Enemy
{
    public class BigLaserEnemyGun : EnemyGunBase
    {
        [Inject] private EnemyBase _enemy;

        [SerializeField] private float _rotationSpeed = 50;
        [SerializeField] private float _shootAimAngleToPlayer = 4;
        [SerializeField] private float _aimRange = 500f;
        [SerializeField] private float _aimFallowTime = 2f;
        [Space]
        [SerializeField] private LaserBeam _laserBeam;
        [SerializeField] private Transform _handleTrans;
        [SerializeField] private Transform _gunTrans;
        [SerializeField] private LayerMask _blockAimLayerMask;

        private float _lastTargetAimableTime = -100;
        private ContactFilter2D _contactFilter;
        private bool isAimedAtPlayer = false;

        private void Awake()
        {
            Initalize();
        }

        public override void Prepare()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnAimingAt(Transform target)
        {
            base.OnAimingAt(target);

            Vector2 gunPos = _handleTrans.position;
            Vector2 gunToTargetDir = target.position - _handleTrans.position;
            float distanceToTarget = Vector2.Distance(target.position, gunPos);
            bool isInRange = distanceToTarget > _shootAimAngleToPlayer;

            bool isVisable = isInRange;
            if(isInRange)
            {
                RaycastHit2D[] raycastHits = new RaycastHit2D[1];
                Physics2D.Raycast(gunPos, gunToTargetDir, _contactFilter, raycastHits, _aimRange);
                isVisable = IsTargetVisable(raycastHits);
            }

            if(isVisable)
            {
                _lastTargetAimableTime = Time.time;
            }

            if(_lastTargetAimableTime + _aimFallowTime < Time.time)
            {
                Vector2 lookForwardPoint = _enemy.transform.up + _enemy.transform.position;
                AimAt(lookForwardPoint, false);
            }
            else
            {
                AimAt(target.position, true);
            }

            RotateGunTransform(target.position);
            
        }

        private void AimAt(Vector2 target, bool invokeOnAimTarget)
        {
            float gunPlayerAngle = Utils.AngleDirected(_handleTrans.position, target) - 90f;

            float rotSpeed = _rotationSpeed * Time.deltaTime;
            float newAngle = Mathf.MoveTowardsAngle(_handleTrans.eulerAngles.z, gunPlayerAngle, rotSpeed);

            Vector3 newRot = new Vector3(_handleTrans.eulerAngles.x, _handleTrans.eulerAngles.y, newAngle);
            _handleTrans.eulerAngles = newRot;

            float angleToTarget = newAngle - gunPlayerAngle;

            if (invokeOnAimTarget && angleToTarget < _shootAimAngleToPlayer / 2 && 
                angleToTarget > -_shootAimAngleToPlayer / 2)
            {
                OnAimTarget?.Invoke();

                Vector3[] positions = new Vector3[2];
                positions[0] = target;
                positions[1] = _laserBeam.transform.position;

                isAimedAtPlayer = true;
            }
            else
            {
                isAimedAtPlayer = false;
            }
        }

        protected override void OnShooting()
        {
            if (!isAimedAtPlayer)
                return;

            _laserBeam.StartFire();
        }

        private void RotateGunTransform(Vector2 target)
        {
            float distance = Vector2.Distance(_handleTrans.position, target);
            Vector2 lookAtPos = _handleTrans.up * distance + _handleTrans.position;
            _gunTrans.LookAt(lookAtPos, -_handleTrans.forward);
        }

        private bool IsTargetVisable(RaycastHit2D[] raycastHits)
        {
            if (raycastHits[0].rigidbody == null)
            {
                return false;
            }

            int layer = LayerMask.NameToLayer(Layers.Player);
            if (raycastHits[0].rigidbody.gameObject.layer == layer)
            {
                return true;
            }

            return false;
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
