using Game.Utility;
using System.Collections;
using UnityEngine;

namespace Game.Combat
{
    public class FlakBullet : ShootableObjectBase
    {
        [SerializeField] private DamageAreaExplosion _areaExplosionPrefab;
        [Space]
        [SerializeField] private float _rotationSpeed = 200.0f;
        [SerializeField, Range(0,1)] private float _velocityRotMulti = 0.8f;

        private Vector2 _targetPosition = Vector2.zero;
        private bool _forwardSpeedChecked = false;
        private float _lastDistanceToTarget = float.MaxValue;
        private float _explodeDistance = 50;

        private void FixedUpdate()
        {
            RotateToPoint(_targetPosition);

            if (!_forwardSpeedChecked ) 
            { 
                EnsureForwardSpeed(_speed);
            }
        }

        public override void Shoot(Rigidbody2D creatorBody, Transform gunTransform)
        {
            gameObject.SetActive(true);

            _body.position = gunTransform.position;
            _body.rotation = gunTransform.rotation.eulerAngles.z;

            SlowVelocityX(gunTransform, creatorBody.velocity, _horizontalMoveInpactMulti);

            float targetForce = _speed * _body.mass;
            _body.AddRelativeForce(Vector2.up * targetForce, ForceMode2D.Impulse);

            _shootTime = Time.time;
            _shootPos = _body.position;
            _shootShipSpeed = GetForwardSpeed(gunTransform, creatorBody.velocity);

            StartCoroutine(WaitAndDestroy());
            StartCoroutine(ExplodeOnReachedTarget());
        }

        public override void OnHit()
        {
            Explode();
        }

        public void SetTarget(Vector2 target)
        {
            _targetPosition = target;
        }

        private void Explode()
        {
            _areaExplosionPrefab.CreateCopy(DamageDealer, transform).Explode();

            _particleSystem.transform.SetParent(null);
            _particleSystem.Play();

            Destroy(gameObject);
        }

        private IEnumerator WaitAndDestroy()
        {
            yield return new WaitUntil(() => SchouldNukeMySelf);

            Explode();
        }

        private IEnumerator ExplodeOnReachedTarget()
        {
            yield return new WaitUntil(IsInExplodeRange);

            Explode();
        }

        private IEnumerator EnsureForwardSpeedEnu()
        {
            yield return null;

            EnsureForwardSpeed(_speed);
        }

        private bool IsInExplodeRange()
        {
            if(_forwardSpeedChecked)
                return false;

            if (_targetPosition == Vector2.zero) 
                return false;

            float distance = Vector2.Distance(_body.position, _targetPosition);

            if(distance > _explodeDistance) 
                return false;

            bool result = distance > _lastDistanceToTarget;
            _lastDistanceToTarget = distance;

            return result;
        }

        private void RotateToPoint(Vector2 point)
        {
            float playerCursorAngle = Utils.AngleDirected(_body.position, point) - 90f;

            float rotSpeed = _rotationSpeed * Time.fixedDeltaTime;
            float newAngle = Mathf.MoveTowardsAngle(_body.rotation, playerCursorAngle, rotSpeed);

            _body.MoveRotation(newAngle);

            TransferVelocity(newAngle);
        }

        private void TransferVelocity(float angle)
        {
            float relativeAngle = Mathf.DeltaAngle(_body.rotation, angle);
            float velocityAngle = relativeAngle * _velocityRotMulti;
            _body.velocity = Utils.RotateVector(_body.velocity, velocityAngle);
        }

        private void EnsureForwardSpeed(float minSpeed)
        {
            Vector2 forwardDirection = transform.up;

            float forwardSpeed = Vector2.Dot(_body.velocity, forwardDirection);

            if (forwardSpeed > minSpeed)
                return;

            float targetForce = (minSpeed - forwardSpeed) * _body.mass;
            _body.AddRelativeForce(Vector2.up * targetForce, ForceMode2D.Impulse);
        }
    }
}
