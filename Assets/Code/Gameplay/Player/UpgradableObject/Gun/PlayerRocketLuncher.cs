using UnityEngine;
using Zenject;
using Game.Input.System;

namespace Game.Player.Modules
{
    public class PlayerRocketLuncher : PlayerGunBase
    {
        [Inject] private Rigidbody2D _body;
        [Inject] private InputProvider _input;

        [SerializeField] private ShootableObjectBase _shootableObjectPrefab;
        [SerializeField] private float _force = 10f;
        [SerializeField] private float _cooldown = 1f;

        private float _lastShotTime = 0f;

        private void Update()
        {
            if (_input.PlayerControls.Gameplay.Shoot.ReadValue<float>() == 1.0f)
            {
                TryShoot();
            }
        }

        private void TryShoot()
        {
            if (Time.time - _lastShotTime < _cooldown)
            {
                return;
            }

            Shoot();
        }

        public override void Shoot()
        {
            _lastShotTime = Time.time;

            _shootableObjectPrefab.CreateCopy().Shoot(_body);
        }
    }
}
