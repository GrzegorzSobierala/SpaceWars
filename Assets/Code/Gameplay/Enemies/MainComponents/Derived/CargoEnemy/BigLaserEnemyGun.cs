using Game.Management;
using Game.Utility;
using Game.Utility.Globals;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class BigLaserEnemyGun : EnemyGunBase
    {
        [SerializeField] private float _chargingTime = 0.3f;
        [SerializeField] private float _rotationSpeed = 50;
        [SerializeField] private float _shootAimAngleToPlayer = 4;
        [SerializeField] private float _aimRange = 500f;
        [SerializeField] private float _aimFallowTime = 2f;
        [Space]
        [SerializeField] private Transform _gunRotate;
        [SerializeField] private Transform _shootPlace;
        [SerializeField] private LayerMask _blockAimLayerMask;

        private float _lastTargetAimableTime;
        private ContactFilter2D _contactFilter;

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

            Vector2 gunPos = _gunRotate.position;
            Vector2 gunToTargetDir = target.position - _gunRotate.position;
            float distanceToTarget = Vector2.Distance(target.position, gunPos);
            bool isInRange = distanceToTarget > _shootAimAngleToPlayer;

            bool isVisable = isInRange;
            if(isInRange)
            {
                RaycastHit2D[] raycastHits = new RaycastHit2D[1];
                Physics2D.Raycast(gunPos, gunToTargetDir, _contactFilter, raycastHits, _aimRange);
                isVisable = IsTargetVisable(raycastHits);
            }

            

            AimAt(target.position, true);

        }

        private void AimAt(Vector2 target, bool invokeOnAimTarget)
        {
            float gunPlayerAngle = Utils.AngleDirected(_gunRotate.position, target) - 90f;

            float rotSpeed = _rotationSpeed * Time.deltaTime;
            float newAngle = Mathf.MoveTowardsAngle(_gunRotate.eulerAngles.z, gunPlayerAngle, rotSpeed);

            Vector3 newRot = new Vector3(_gunRotate.eulerAngles.x, _gunRotate.eulerAngles.y, newAngle);
            _gunRotate.eulerAngles = newRot;

            float angleToTarget = newAngle - gunPlayerAngle;

            if (invokeOnAimTarget && angleToTarget < _shootAimAngleToPlayer / 2 && 
                angleToTarget > -_shootAimAngleToPlayer / 2)
            {
                OnAimTarget?.Invoke();
            }
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
                Debug.Log($"sus");
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
