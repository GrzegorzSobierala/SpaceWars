using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class CargoSpace : MonoBehaviour
    {
        [SerializeField] private SerializedDictionary<Transform, AmmoSupply> _cargoSlots;

        private AmmoSupply _supplyFromTop = null;
        private int _fullSlotsCount = 0;

        public AmmoSupply SupplyFromTop => _supplyFromTop;
        public int FullSlotsCount => _fullSlotsCount;

        private void Awake()
        {
            InitFullSlotsCount();
        }

        public void LoadCargo(AmmoSupply supply)
        {
            foreach (var slot in _cargoSlots)
            {
                if (slot.Value == null)
                {
                    _cargoSlots[slot.Key] = supply;
                    supply.transform.SetParent(slot.Key);
                    supply.transform.localPosition = Vector3.zero;
                    supply.transform.localRotation = Quaternion.identity;
                    
                    _fullSlotsCount++;
                    _supplyFromTop = supply;

                    if (_fullSlotsCount > _cargoSlots.Count)
                    {
                        Debug.LogError("_fullSlotsCount error too much");
                        _fullSlotsCount = Mathf.Clamp(_fullSlotsCount,0, _cargoSlots.Count);
                    }

                    return;
                }
            }

            Debug.LogError("Cargo space full");
        }

        public AmmoSupply UnloadCargo(Transform newParent)
        {
            int index = 0;
            Transform lastCheckSlot = null;
            Transform prevLastCheckSlot = null;


            foreach (var slot in _cargoSlots)
            {
                if (slot.Value == null || index + 1 == _cargoSlots.Count)
                {
                    if(index + 1 == _cargoSlots.Count && slot.Value != null)
                    {
                        prevLastCheckSlot = lastCheckSlot;
                        lastCheckSlot = slot.Key;
                    }

                    AmmoSupply unloadedSupply = _cargoSlots[lastCheckSlot];
                    _cargoSlots[lastCheckSlot] = null;
                    unloadedSupply.transform.SetParent(newParent);

                    _fullSlotsCount--;

                    if(prevLastCheckSlot == null)
                    {
                        _supplyFromTop = null;
                    }
                    else
                    {
                        _supplyFromTop = _cargoSlots[prevLastCheckSlot];
                    }

                    if (_fullSlotsCount < 0)
                    {
                        Debug.LogError("_fullSlotsCount error too few");
                        _fullSlotsCount = Mathf.Clamp(_fullSlotsCount, 0, _cargoSlots.Count);
                    }

                    return unloadedSupply;
                }

                prevLastCheckSlot = lastCheckSlot;
                lastCheckSlot = slot.Key;
                index++;
            }

            Debug.LogError("Cargo space empty");
            return null;
        }

        public bool IsCargoSpaceFull()
        {
            return _fullSlotsCount == _cargoSlots.Count;
        }

        public bool IsCargoSpaceEmpty()
        {
            return _fullSlotsCount == 0;
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

        private void InitFullSlotsCount()
        {
            int fullSlotsCount = 0;
            AmmoSupply lastChckedSupply = null;
            foreach (var slot in _cargoSlots)
            {
                if (slot.Value != null)
                {
                    fullSlotsCount++;
                    lastChckedSupply = slot.Value;
                }
            }

            _fullSlotsCount = fullSlotsCount;
            _supplyFromTop = lastChckedSupply;
        }
    }
}
