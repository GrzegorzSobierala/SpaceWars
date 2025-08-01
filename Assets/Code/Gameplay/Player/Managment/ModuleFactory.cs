using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public class ModuleFactory : MonoBehaviour
    {
        [Inject] private DiContainer _container;
        [Inject] private ModuleHandler _moduleHandler;
        
        [SerializeField] private List<HullModuleBase> _hullPrefabs;
        [SerializeField] private List<GunModuleBase> _gunPrefabs;
        [SerializeField] private List<BridgeModuleBase> _bridgesPrefabs;
        [SerializeField] private List<SpecialGunModuleBase> _specialGunsPrefabs;

        private HullModuleBase _currentHullPrototype;
        private GunModuleBase _currentGunPrototype;
        private BridgeModuleBase _currentBridgePrototype;
        private SpecialGunModuleBase _currentSpecialGunPrototype;

        private void Awake()
        {
            ReferencesCheck();
            Init();
        }

        #region Changing modules

        [ContextMenu("SetNextHull")]
        public void SetNextHull()
        {
            SetNext(_hullPrefabs, ref _currentHullPrototype, false);

            ReplaceHull(_currentHullPrototype);
        }

        [ContextMenu("SetPreviusHull")]
        public void SetPreviusHull()
        {
            SetNext(_hullPrefabs, ref _currentHullPrototype, true);

            ReplaceHull(_currentHullPrototype);
        }

        [ContextMenu("SetNextGun")]
        public void SetNextGun()
        {
            SetNext(_gunPrefabs, ref _currentGunPrototype, false);

            ReplaceGun(_currentGunPrototype);
        }

        [ContextMenu("SetPreviusGun")]
        public void SetPreviusGun()
        {
            SetNext(_gunPrefabs, ref _currentGunPrototype, true);

            ReplaceGun(_currentGunPrototype);
        }

        [ContextMenu("SetNextBridge")]
        public void SetNextBridge()
        {
            SetNext(_bridgesPrefabs, ref _currentBridgePrototype, false);

            ReplaceBridge(_currentBridgePrototype);
        }

        [ContextMenu("SetPreviusBridge")]
        public void SetPreviusBridge()
        {
            SetNext(_bridgesPrefabs, ref _currentBridgePrototype, true);

            ReplaceBridge(_currentBridgePrototype);
        }

        [ContextMenu("SetNextSpecialGun")]
        public void SetNextSpecialGun()
        {
            SetNext(_specialGunsPrefabs, ref _currentSpecialGunPrototype, false);

            ReplaceSpecialGun(_currentSpecialGunPrototype);
        }

        [ContextMenu("SetPreviusSpecialGun")]
        public void SetPreviusSpecialGun()
        {
            SetNext(_specialGunsPrefabs, ref _currentSpecialGunPrototype, true);

            ReplaceSpecialGun (_currentSpecialGunPrototype);
        }

        #endregion

        public void ReplaceHull(HullModuleBase hullPrototype)
        {
            if (_moduleHandler.CurrentHull != null)
            {
                Destroy(_moduleHandler.CurrentHull.gameObject);
            }

            Transform parent = _moduleHandler.ModulesParent;
            HullModuleBase newHull = hullPrototype.Instatiate(parent, _container);
            _moduleHandler.SetHull(this, newHull);
            ReplaceGun(_currentGunPrototype);
            //ReplaceSpecialGun(_currentSpecialGunPrototype);
        }

        public void ReplaceGun(GunModuleBase gunPrototype)
        {
            if (_moduleHandler.CurrentGun != null)
            {
                Destroy(_moduleHandler.CurrentGun.gameObject);
            }

            Transform gunSpot = _moduleHandler.CurrentHull.GunSpot;
            GunModuleBase newGun = gunPrototype.Instatiate(gunSpot, _container);
            _moduleHandler.SetGun(this, newGun);
        }

        public void ReplaceBridge(BridgeModuleBase bridgePrototype)
        {
            if (_moduleHandler.CurrentBridge != null)
            {
                Destroy(_moduleHandler.CurrentBridge.gameObject);
            }

            Transform bridgeSpot = _moduleHandler.CurrentHull.SpecialGunSpot;
            BridgeModuleBase newBridge = bridgePrototype.Instatiate(bridgeSpot, _container);
            _moduleHandler.SetBridge(this, newBridge);
        }

        public void ReplaceSpecialGun(SpecialGunModuleBase gunPrototype)
        {
            if (_moduleHandler.CurrentSpecialGun != null)
            {
                Destroy(_moduleHandler.CurrentSpecialGun.gameObject);
            }

            Transform gunSpot = _moduleHandler.CurrentHull.SpecialGunSpot;
            SpecialGunModuleBase newGun = gunPrototype.Instatiate(gunSpot, _container);
            _moduleHandler.SetSpecialGun(this, newGun);
        }


        private void Init()
        {
            _currentHullPrototype = _hullPrefabs[0];
            _currentGunPrototype = _gunPrefabs[0];
            _currentBridgePrototype = _bridgesPrefabs[0];
            _currentSpecialGunPrototype = _specialGunsPrefabs[0];

            ReplaceHull(_currentHullPrototype);
            ReplaceGun(_currentGunPrototype);
            //ReplaceSpecialGun(_currentSpecialGunPrototype);
        }
        
        private void SetNext<T>(List<T> prototypes, ref T currentModule, bool goBack) where T : IModule
        {
            int currentIndex = prototypes.IndexOf(currentModule);

            if (currentIndex == -1)
            {
                Debug.Log($"Can't find prototype {currentModule.ToString()}, setting index 0");
                currentIndex = 0;
            }

            int targetIndex = currentIndex + (goBack ? -1 : 1);

            if (targetIndex >= prototypes.Count)
            {
                targetIndex = 0;
            }
            else if (targetIndex < 0)
            {
                targetIndex = prototypes.Count - 1;
            }

            currentModule = prototypes[targetIndex];
        }

        private void ReferencesCheck()
        {
            if (_hullPrefabs.Count == 0)
            {
                Debug.LogError("List of hull prototypes is empty", this);
            }

            if (_gunPrefabs.Count == 0)
            {
                Debug.LogError("List of gun prototypes is empty", this);
            }

            if(_bridgesPrefabs.Count == 0)
            {
                Debug.LogError("List of bridges prototypes is empty", this);
            }

            if(_specialGunsPrefabs.Count == 0)
            {
                Debug.LogError("List of special gun prototypes is empty", this);
            }
        }
    }
}
