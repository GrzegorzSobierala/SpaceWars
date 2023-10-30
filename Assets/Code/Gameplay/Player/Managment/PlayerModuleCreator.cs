using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public class PlayerModuleCreator : MonoBehaviour
    {
        [Inject] private DiContainer _container;
        [Inject] private PlayerModuleHandler _moduleHandler;

        [SerializeField] private List<PlayerHullModuleBase> _hullPrefabs;
        [SerializeField] private List<PlayerGunModuleBase> _gunPrefabs;
        [SerializeField] private List<BridgeModuleBase> _bridgesPrefabs;

        private PlayerHullModuleBase _currentHullPrototype;
        private PlayerGunModuleBase _currentGunPrototype;
        private BridgeModuleBase _currentViewfinderPrototype;

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

        [ContextMenu("SetNextViewfinder")]
        public void SetNextViewfinder()
        {
            SetNext(_bridgesPrefabs, ref _currentViewfinderPrototype, false);

            ReplaceViewfinder(_currentViewfinderPrototype);
        }

        [ContextMenu("SetPreviusViewfinder")]
        public void SetPreviusViewfinder()
        {
            SetNext(_bridgesPrefabs, ref _currentViewfinderPrototype, true);

            ReplaceViewfinder(_currentViewfinderPrototype);
        }

        private void SetNext<T>(List<T> prototypes, ref T currentModule, bool goBack) where T : IModule
        {
            int currentIndex = prototypes.IndexOf(currentModule);
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

        #endregion

        private void Init()
        {
            _currentHullPrototype = _hullPrefabs[0];
            _currentGunPrototype = _gunPrefabs[0];
            _currentViewfinderPrototype = _bridgesPrefabs[0];

            ReplaceHull(_currentHullPrototype);
            ReplaceGun(_currentGunPrototype);
            ReplaceViewfinder(_currentViewfinderPrototype);
        }

        private void ReplaceHull(PlayerHullModuleBase hullPrototype)
        {
            if (_moduleHandler.CurrentHull != null)
            {
                Destroy(_moduleHandler.CurrentHull.gameObject);
            }

            PlayerHullModuleBase newHull = hullPrototype.Instatiate(transform, _container);
            _moduleHandler.SetHull(this, newHull);
            ReplaceGun(_currentGunPrototype);
        }

        private void ReplaceGun(PlayerGunModuleBase gunPrototype)
        {
            if (_moduleHandler.CurrentGun != null)
            {
                Destroy(_moduleHandler.CurrentGun.gameObject);
            }

            Transform gunSpot = _moduleHandler.CurrentHull.GunSpot;
            PlayerGunModuleBase newGun = gunPrototype.Instatiate(gunSpot, _container);
            _moduleHandler.SetGun(this, newGun);
        }

        private void ReplaceViewfinder(BridgeModuleBase viewfinderPrototype)
        {
            if(_moduleHandler.CurrentViewfinder != null)
            {
                Destroy(_moduleHandler.CurrentViewfinder.gameObject);
            }

            Transform viewfinderSpot = _moduleHandler.CurrentHull.ViewfinderSpot;
            BridgeModuleBase newViewfinder = viewfinderPrototype.Instatiate(viewfinderSpot, _container);
            _moduleHandler.SetViewfinder(this, newViewfinder);
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
                Debug.LogError("List of bridges is empty", this);
            }
        }
    }
}
