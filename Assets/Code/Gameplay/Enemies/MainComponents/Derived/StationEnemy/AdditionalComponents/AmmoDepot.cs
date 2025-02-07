using System.Collections;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class AmmoDepot : MonoBehaviour
    {
        [Inject] private DockPlace _dockPlace;
        [Inject] private ShipCargoSpace _cargoSpace;

        [SerializeField] private float _unloadTime = 3f;

        private Coroutine _unloadingCoroutine;

        private void Start()
        {
            _dockPlace.OnDock += StartUnloading;
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
            ShipCargoSpace shipCargoSpace = ship.Body.transform.GetComponentInChildren<ShipCargoSpace>();

            if (shipCargoSpace == null)
            {
                Debug.LogError("No " + nameof(ShipCargoSpace));
                return;
            }

            ship.CanUndock += () => IsUnloadingEnd(shipCargoSpace);

            StartUnloadingSupply(shipCargoSpace);
        }

        private void StartUnloadingSupply(ShipCargoSpace shipCargoSpace)
        {
            if (_cargoSpace.IsCargoSpaceFull())
                return;

            if (shipCargoSpace.IsCargoSpaceEmpty())
                return;

            _unloadingCoroutine = StartCoroutine(Unloading(shipCargoSpace));
        }

        private IEnumerator Unloading(ShipCargoSpace shipCargoSpace)
        {
            yield return new WaitForSeconds(_unloadTime);

            AmmoSupply ammoSupply = shipCargoSpace.UnloadCargo(transform);
            _cargoSpace.LoadCargo(ammoSupply);

            _unloadingCoroutine = null;
            StartUnloadingSupply(shipCargoSpace);
        }

        private bool IsUnloadingEnd(ShipCargoSpace shipCargoSpace)
        {
            return shipCargoSpace.IsCargoSpaceEmpty() || _cargoSpace.IsCargoSpaceFull();
        }
    }
}
