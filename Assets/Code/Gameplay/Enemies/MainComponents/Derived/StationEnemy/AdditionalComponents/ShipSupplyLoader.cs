using System.Collections;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class ShipSupplyLoader : MonoBehaviour
    {
        [Inject] private DockPlace _dockPlace;

        [SerializeField] private Transform _supplaySpawnParent;
        [SerializeField] private AmmoSupply _supplyPrototype;
        [SerializeField] private float _loadSupplyTime = 4f;

        private Coroutine _loadingCoroutine;
        private AmmoSupply _currentSupply;

        private void Start()
        {
            _dockPlace.OnDock += StartLoading;
        }

        private void StartLoading(IDocking ship)
        {
            ShipCargoSpace shipCargoSpace = ship.Body.transform.GetComponentInChildren<ShipCargoSpace>();

            if (shipCargoSpace == null)
            {
                Debug.LogError("No " + nameof(ShipCargoSpace));
                return;
            }

            ship.CanUndock += shipCargoSpace.IsCargoSpaceFull;

            StartLoadingSupply(shipCargoSpace);
        }

        private void StartLoadingSupply(ShipCargoSpace shipCargoSpace)
        {
            Transform targetSlot = shipCargoSpace.GetFreeSlot();
            if (targetSlot == null)
            {
                return;
            }

            _currentSupply = Instantiate(_supplyPrototype, _supplaySpawnParent,false);
            _currentSupply.gameObject.SetActive(true);

            _loadingCoroutine = StartCoroutine(Loading(shipCargoSpace));
            
        }

        private IEnumerator Loading(ShipCargoSpace shipCargoSpace)
        {
            yield return new WaitForSeconds(_loadSupplyTime);

            shipCargoSpace.LoadCargo(_currentSupply);
            _currentSupply = null;
            _loadingCoroutine = null;
            StartLoadingSupply(shipCargoSpace);
        }
    }
}
