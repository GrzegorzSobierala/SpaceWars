using UnityEngine;

namespace Game.Player.Ship
{
    public class PlayerModuleHandler : MonoBehaviour
    {
        public PlayerHullModuleBase CurrentHull => _currentHull;
        public PlayerGunBase CurrentGun => _currentGun;

        private PlayerHullModuleBase _currentHull;
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

        public void SetHull(PlayerModuleCreator creator, PlayerHullModuleBase hull)
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
