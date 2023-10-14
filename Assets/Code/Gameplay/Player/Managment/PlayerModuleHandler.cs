using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Player.Modules
{
    public class PlayerModuleHandler : MonoBehaviour
    {
        [Inject] private DiContainer _container;

        [SerializeField] private List<PlayerHullBase> _hullPrototypes;
        [SerializeField] private List<PlayerGunBase> _gunPrototypes;

        private PlayerHullBase _currentHullPrototype;
        private PlayerGunBase _currentGunPrototype;

        private PlayerHullBase _currentHull;
        private PlayerGunBase _currentGun;    

        private void Awake()
        {
            ReferencesCheck();
            Init();
        }

        #region Changing modules

        [ContextMenu("SetNextHull")]
        public void SetNextHull()
        {
            SetNext(_hullPrototypes, ref _currentHullPrototype, false);

            ReplaceHull(_currentHullPrototype);
        }

        [ContextMenu("SetPreviusHull")]
        public void SetPreviusHull()
        {
            SetNext(_hullPrototypes, ref _currentHullPrototype, true);

            ReplaceHull(_currentHullPrototype);
        }

        [ContextMenu("SetNextGun")]
        public void SetNextGun()
        {
            SetNext(_gunPrototypes, ref _currentGunPrototype, false);

            _currentGun = _currentHull.ReplaceGun(_currentGunPrototype);
        }

        [ContextMenu("SetPreviusGun")]
        public void SetPreviusGun()
        {
            SetNext(_gunPrototypes, ref _currentGunPrototype, true);

            _currentGun = _currentHull.ReplaceGun(_currentGunPrototype);
        }
        private void SetNext<T>(List<T> prototypes, ref T currentModule, bool goBack) where T : UpgradableObjectBase
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
            _currentHullPrototype = _hullPrototypes[0];
            _currentGunPrototype = _gunPrototypes[0];

            ReplaceHull(_currentHullPrototype);
            _currentGun = _currentHull.ReplaceGun(_currentGunPrototype);
        }

        private void ReplaceHull(PlayerHullBase hullPrototype)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            _currentHull = hullPrototype.Instatiate(transform, _container);
            _currentHull.ReplaceGun(_currentGunPrototype);
        }

        private void ReferencesCheck()
        {
            if (_hullPrototypes.Count == 0)
            {
                Debug.LogError("List of hull prototypes is empty");
            }

            if (_gunPrototypes.Count == 0)
            {
                Debug.LogError("List of gun prototypes is empty");
            }
        }
    }
}
