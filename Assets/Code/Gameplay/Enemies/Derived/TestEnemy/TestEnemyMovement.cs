using UnityEngine;

namespace Game.Room.Enemy
{
    public class TestEnemyMovement : EnemyMovementBase
    {
        public override bool UseFixedUpdate => true;

        [SerializeField] private float _moveCooldown = 5f;
        [SerializeField] private bool _isRandomMove = false;
        [SerializeField] private bool _isHorizontalMove = false;
        [SerializeField] private bool _fallowPlayer = false;
        [Space]
        [SerializeField] private float _randomMulti = 2;
        [SerializeField] private float _horizontalMulti = 1;
        [SerializeField] private float _fallowMulti = 0.3f;

        private float _nextRandomMoveTime = 0;
        private float _nextHorizontalMoveTime = 0;
        private float _nextFallowPlayerMoveTime = 0;
        private bool _wasLastHorizontalLeft = false;

        protected override void OnGoingTo(Transform fallowTarget)
        {
            base.OnGoingTo(fallowTarget);

            if (_isRandomMove)
            {
                TryRandomMove();
            }

            if (_isHorizontalMove)
            {
                TryHorizontalMove();
            }

            if (_fallowPlayer)
            {
                TryFallowPlayerMove(fallowTarget);
            }
        }

        private void TryRandomMove()
        {
            if (_nextRandomMoveTime < Time.time)
            {
                RandomMovement();
            }
        }

        private void RandomMovement()
        {
            float randomX = Random.Range(-1.0f, 1.0f);
            float randomY = Random.Range(-1.0f, 1.0f);

            float force = BaseSpeed * _randomMulti * _body.mass * Random.Range(1.0f, 2.0f);

            Vector2 forceVector = new Vector2(randomX, randomY) * force;

            _body.AddRelativeForce(forceVector);

            float moveRange = _moveCooldown * 0.5f;
            _nextRandomMoveTime = Random.Range(_moveCooldown - moveRange, _moveCooldown + moveRange) + Time.time;
        }

        private void TryHorizontalMove()
        {
            if (_nextHorizontalMoveTime < Time.time)
            {
                HorizontalMove();
            }
        }

        private void HorizontalMove()
        {
            if (!_fallowPlayer && !_isRandomMove)
            {
                _body.velocity = Vector2.zero;
            }

            float force = BaseSpeed * _horizontalMulti * _body.mass * Random.Range(1.0f, 2.0f);

            force *= _wasLastHorizontalLeft ? 1.0f : -1.0f;

            Vector2 forceVector = Vector2.left * force;
            _body.AddRelativeForce(forceVector);

            float moveRange = _moveCooldown * 0.2f;
            float moveCooldown = Random.Range(_moveCooldown - moveRange, _moveCooldown + moveRange);
            _nextHorizontalMoveTime = moveCooldown + Time.time;
            _wasLastHorizontalLeft = !_wasLastHorizontalLeft;
        }

        private void TryFallowPlayerMove(Transform fallowTarget)
        {
            if (_nextFallowPlayerMoveTime < Time.time)
            {
                FallowPalyerMove(fallowTarget);
            }
        }

        private void FallowPalyerMove(Transform fallowTarget)
        {
            float force = BaseSpeed * _fallowMulti * _body.mass * Random.Range(1.0f, 2.0f);

            Vector2 forceVector = (Vector2)fallowTarget.position - _body.position;
            forceVector = forceVector.normalized * force;

            _body.AddForce(forceVector);

            float moveCooldown = _moveCooldown * 0.2f;
            float moveRange = moveCooldown * 0.5f;
            float targetMoveCooldown = Random.Range(moveCooldown - moveRange, moveCooldown + moveRange);
            _nextFallowPlayerMoveTime = targetMoveCooldown + Time.time;
        }
    }
}
