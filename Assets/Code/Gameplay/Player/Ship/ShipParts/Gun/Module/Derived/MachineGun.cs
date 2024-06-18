using UnityEngine;

namespace Game.Player.Ship
{
    public class MachineGun : GunModuleBase
    {
        [Header("MachineGun properies")]
        [SerializeField] private int _maxAmmo = 20;
        [SerializeField] private float _loadCooldown = 0.1f;

        private int _currentAmmo = 0;
        private float _lastLoadTime = 0;

        public int MaxAmmo => _maxAmmo;
        public int CurrentAmmo => _currentAmmo;

        public override bool IsGunReadyToShoot 
        {  
            get 
            { 
                return Time.time - _lastShotTime >= _cooldown && _currentAmmo > 0; 
            } 
        }

        private void Start()
        {
            _currentAmmo = _maxAmmo;
        }

        private void Update()
        {
            TryLoad();
        }

        protected override bool OnTryShoot()
        {
            if (!IsGunReadyToShoot)
                return false;

            _lastShotTime = Time.time;

            GameObject damageDealer = _body.gameObject;
            Transform parent = _playerManager.transform;
            _shootableObjectPrototype.CreateCopy(damageDealer, parent).Shoot(_body, transform);

            _currentAmmo--;
            return true;
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
