using Game.Player.Modules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game
{
    public class PlayerModuleHandler : MonoBehaviour
    {
        public PlayerHullBase CurrentHull => _currentHull;
        public PlayerGunBase CurrentGun => _currentGun;

        private PlayerHullBase _currentHull;
        private PlayerGunBase _currentGun;

        public void SetGun(PlayerModuleCreator creator, PlayerGunBase gun)
        {
            if(creator == null)
            {
                Debug.LogError("Creator is null");
                return;
            }

            _currentGun = gun;
        }

        public void SetHull(PlayerModuleCreator creator, PlayerHullBase hull)
        {
            if (creator == null)
            {
                Debug.LogError("Creator is null");
                return;
            }

            _currentHull = hull;
        }
    }
}
