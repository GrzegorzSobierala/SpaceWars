using System.Collections.Generic;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class ShipCargoSpace : MonoBehaviour
    {
        [SerializeField] private List<Transform> _cargoSlotPositions;

        private Dictionary<Transform, AmmoSupply> _cargoSlots = new();

        private void Awake()
        {
            foreach (var slot in _cargoSlotPositions)
            {
                _cargoSlots.Add(slot, null);
            }
        }

        public void LoadCargo(AmmoSupply supply)
        {
            foreach (var slot in _cargoSlotPositions)
            {
                if (_cargoSlots[slot] == null)
                {
                    _cargoSlots[slot] = supply;
                    supply.transform.SetParent(slot);
                    supply.transform.localPosition = Vector3.zero;
                    supply.transform.localRotation = Quaternion.identity;
                    return;
                }
            }

            Debug.LogError("Cargo space full");
        }

        public AmmoSupply UnloadCargo(Transform newParent)
        {
            foreach (var slot in _cargoSlotPositions)
            {
                if (_cargoSlots[slot] != null)
                {
                    AmmoSupply unloadedSupply = _cargoSlots[slot];
                    _cargoSlots[slot] = null;
                    unloadedSupply.transform.SetParent(newParent);
                    return unloadedSupply;
                }
            }

            Debug.LogError("Cargo space empty");
            return null;
        }

        public bool IsCargoSpaceFull()
        {
            foreach (var slot in _cargoSlots)
            {
                if (slot.Value == null)
                    return false;
            }

            return true;
        }

        public bool IsCargoSpaceEmpty()
        {
            foreach (var slot in _cargoSlots)
            {
                if (slot.Value != null)
                    return false;
            }

            return true;
        }

        public Transform GetFreeSlot()
        {
            foreach (var slot in _cargoSlots)
            {
                if (slot.Value == null)
                    return slot.Key;
            }

            return null;
        }
    }
}
