using UnityEngine;

namespace Game.Player.Ship
{
    public class PlayerModuleHandler : MonoBehaviour
    {
        public PlayerHullModuleBase CurrentHull => _currentHull;
        public PlayerGunModuleBase CurrentGun => _currentGun;
        public BridgeModuleBase CurrentBridge => _currentBridge;

        private PlayerHullModuleBase _currentHull;
        private PlayerGunModuleBase _currentGun;
        private BridgeModuleBase _currentBridge;

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

        public void SetBridge(PlayerModuleCreator creator, BridgeModuleBase viewfinder)
        {
            if (creator == null)
            {
                Debug.LogError("Creator is null");
                return;
            }

            _currentBridge = viewfinder;
        }

    }
}
