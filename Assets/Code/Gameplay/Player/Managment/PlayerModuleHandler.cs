using UnityEngine;

namespace Game.Player.Ship
{
    public class PlayerModuleHandler : MonoBehaviour
    {
        public PlayerHullModuleBase CurrentHull => _currentHull;
        public PlayerGunModuleBase CurrentGun => _currentGun;
        public ViewfinderModuleBase CurrentViewfinder => _currentViewfinder;

        private PlayerHullModuleBase _currentHull;
        private PlayerGunModuleBase _currentGun;
        private ViewfinderModuleBase _currentViewfinder;

        public void SetGun(PlayerModuleCreator creator, PlayerGunModuleBase gun)
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

        public void SetViewfinder(PlayerModuleCreator creator, ViewfinderModuleBase viewfinder)
        {
            if (creator == null)
            {
                Debug.LogError("Creator is null");
                return;
            }

            _currentViewfinder = viewfinder;
        }

    }
}
