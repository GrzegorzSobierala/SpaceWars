using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public class MachineGun : GunModuleBase
    {
        [Header("MaschineGun properies")]
        [SerializeField] private int _maxAmmo = 20;
        [SerializeField] private float _loadCooldown = 0.1f;

        private int _currentAmmo = 0;
        private float _lastLoadTime = 0;

        public int MaxAmmo => _maxAmmo;
        public int CurrentAmmo => _currentAmmo;

        private void Start()
        {
            _currentAmmo = _maxAmmo;
        }

        private void Update()
        {
            TryLoad();

            if (Input.Shoot.ReadValue<float>() == 1.0f)
            {
                TryShoot();
            }
        }

        public override void Shoot()
        {
            _lastShotTime = Time.time;

            _shootableObjectPrefab.CreateCopy(_playerManager.transform).Shoot(_body, transform);

            _currentAmmo--;
        }

        private void TryShoot()
        {
            if (Time.time - _lastShotTime < _cooldown || _currentAmmo <= 0)
                return;

            Shoot();
        }

        private void TryLoad()
        {
            if (_maxAmmo <= _currentAmmo || Time.time - _lastLoadTime <= _loadCooldown)
                return;

            Load();
        }

        private void Load()
        {
            _currentAmmo++;
            _lastLoadTime = Time.time;
        }
    }
}
