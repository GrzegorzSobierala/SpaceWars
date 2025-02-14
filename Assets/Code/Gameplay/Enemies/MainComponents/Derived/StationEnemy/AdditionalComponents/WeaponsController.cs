using Game.Management;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class WeaponsController : MonoBehaviour
    {
        [Inject] protected PlayerManager _playerManager;
        [Inject] private AmmoDepot _ammoDepot;

        [SerializeField] private Transform _ammoWeaponsParent;
        [SerializeField] private Transform _noAmmoWeaponsParent;

        private Dictionary<EnemyGunBase, GunAmmoUsage> _ammoWeapons = new();
        private EnemyGunBase[] _noAmmoWeapons;

        private void Awake()
        {
            Init();
        }

        public void StartShooting()
        {
            foreach (var gun in _ammoWeapons)
            {
                gun.Key.StartAimingAt(_playerManager.PlayerBody.transform);
                gun.Key.StartShooting();
                gun.Key.CanShoot += TryGiveAmmoToGun;
            }

            foreach (var gun in _noAmmoWeapons)
            {
                gun.StartAimingAt(_playerManager.PlayerBody.transform);
                gun.StartShooting();
            }
        }

        public void StopShooting()
        {
            foreach (var gun in _ammoWeapons)
            {
                gun.Key.StopAiming();
                gun.Key.StopShooting();
                gun.Key.CanShoot -= TryGiveAmmoToGun;
            }

            foreach (var gun in _noAmmoWeapons)
            {
                gun.StopAiming();
                gun.StopShooting();
            }
        }

        private bool TryGiveAmmoToGun(EnemyGunBase gun)
        {
            return _ammoDepot.TryUseAmmo(_ammoWeapons[gun].AmmoPerShot);
        }

        private void Init()
        {
            _noAmmoWeapons = _noAmmoWeaponsParent.GetComponentsInChildren<EnemyGunBase>(true);
            EnemyGunBase[] ammoWeapons = _ammoWeaponsParent.GetComponentsInChildren<EnemyGunBase>(true);
            foreach (var weapon in ammoWeapons)
            {
                _ammoWeapons.Add(weapon, weapon.GetComponent<GunAmmoUsage>());
            }
        }
    }
}
