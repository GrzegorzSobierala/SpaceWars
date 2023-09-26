using UnityEngine;
using Zenject;
using Game.Input;
using Game.Input.System;

namespace Game.Combat
{
    public class PlayerGun : MonoBehaviour
    {
        [Inject] Rigidbody2D _body;
        [Inject] InputProvider _input;

        [SerializeField] RocketController _rocketPrefab;
        [SerializeField] float _force = 10f;
        [SerializeField] float _cooldown = 1f;

        float _lastShotTime = 0f;

        private void Update()
        {
            if (_input.PlayerControls.Gameplay.Shoot.ReadValue<float>() == 1.0f)
            {
                Shoot();
            }
        }

        public void Shoot()
        {
            if(Time.time - _lastShotTime < _cooldown)
            {
                return;
            }

            _lastShotTime = Time.time;

            RocketController.InstantiateAndShoot(_rocketPrefab, _body, _force);
        }
    }
}
