using UnityEngine;

namespace Game.Player.Ship
{
    public class ModuleHandler : MonoBehaviour
    {
        public HullModuleBase CurrentHull => _currentHull;
        public GunModuleBase CurrentGun => _currentGun;
        public BridgeModuleBase CurrentBridge => _currentBridge;
        public SpecialGunModuleBase CurrentSpecialGun => _currentspecialGun;

        private HullModuleBase _currentHull;
        private GunModuleBase _currentGun;
        private BridgeModuleBase _currentBridge;
        private SpecialGunModuleBase _currentspecialGun;
        public void SetGun(ModuleFactory creator, GunModuleBase gun)
        {
            if(creator == null)
            {
                Debug.LogError("Creator is null");
                return;
            }

            _currentGun = gun;
        }

        public void SetHull(ModuleFactory creator, HullModuleBase hull)
        {
            if (creator == null)
            {
                Debug.LogError("Creator is null");
                return;
            }

            _currentHull = hull;
        }

        public void SetBridge(ModuleFactory creator, BridgeModuleBase viewfinder)
        {
            if (creator == null)
            {
                Debug.LogError("Creator is null");
                return;
            }

            _currentBridge = viewfinder;
        }

        public void SetSpecialGun(ModuleFactory creator, SpecialGunModuleBase specialGun)
        {
            if (creator == null)
            {
                Debug.LogError("Creator is null");
                return;
            }

            _currentspecialGun = specialGun;
        }
    }
}
