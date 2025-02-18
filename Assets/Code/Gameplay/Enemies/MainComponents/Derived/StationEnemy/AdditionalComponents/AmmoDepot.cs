using Game.Utility;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class AmmoDepot : MonoBehaviour
    {
        [Inject] private DockPlace _dockPlace;
        [Inject] private CargoSpace _cargoSpace;

        [SerializeField] private float _unloadTime = 3f;

        private Coroutine _unloadingCoroutine;

        private void Start()
        {
            _dockPlace.OnDock += StartUnloading;
            _dockPlace.OnUndock += EndUnloading;
        }

        public bool TryUseAmmo(int amount)
        {
            int takenAmmoFromAll = 0;

            while (!_cargoSpace.IsCargoSpaceEmpty())
            {
                AmmoSupply ammoSupply = _cargoSpace.SupplyFromTop;

                int takenAmmoFromOne = ammoSupply.TakeAmmo(amount - takenAmmoFromAll);

                takenAmmoFromAll += takenAmmoFromOne;

                if (takenAmmoFromAll < amount)
                {
                    AmmoSupply unloaded = _cargoSpace.UnloadCargo(transform);

                    if (unloaded != ammoSupply)
                    {
                        Debug.LogError("AmmoSupply error");
                    }

                    ammoSupply.DestroySupply();
                }

                if (takenAmmoFromAll == amount)
                    return true;

                if (takenAmmoFromAll > amount)
                {
                    Debug.LogError("To much ammmo taken");
                }
            }

            if (takenAmmoFromAll != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void StartUnloading(IDocking ship)
        {
            CargoSpace shipCargoSpace = ship.Body.transform.GetComponentInChildren<CargoSpace>();

            if (shipCargoSpace == null)
            {
                Debug.LogError("No " + nameof(CargoSpace));
                return;
            }

            ship.CanUndock += () => IsUnloadingEnd(shipCargoSpace);

            _unloadingCoroutine = StartCoroutine(Unloading(shipCargoSpace));
        }

        private IEnumerator Unloading(CargoSpace shipCargoSpace)
        {
            while (true)
            {
                if(_cargoSpace.IsCargoSpaceFull())
                {
                    yield return null;
                }
                else
                {
                    yield return new WaitForSeconds(_unloadTime);

                    AmmoSupply ammoSupply = shipCargoSpace.UnloadCargo(transform);
                    _cargoSpace.LoadCargo(ammoSupply);
                    ammoSupply.EnableCollider(true);
                    yield return null;
                }
            }
        }

        private bool IsUnloadingEnd(CargoSpace shipCargoSpace)
        {
            return shipCargoSpace.IsCargoSpaceEmpty() || _cargoSpace.IsCargoSpaceFull();
        }

        private void EndUnloading(IDocking _)
        {
            this.StopAndClearCoroutine(ref _unloadingCoroutine);
        }
    }
}
