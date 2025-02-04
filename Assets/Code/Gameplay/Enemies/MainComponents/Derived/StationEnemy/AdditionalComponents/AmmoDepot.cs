using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class AmmoDepot : MonoBehaviour
    {
        [SerializeField] private List<AmmoSupply> _ammoSupplies;

        public bool TryUseAmmo(int amount)
        {
            int takenAmmoFromAll = 0;

            while (_ammoSupplies.Count != 0)
            {
                int takenAmmoFromOne = _ammoSupplies[0].TakeAmmo(amount - takenAmmoFromAll);

                if(takenAmmoFromOne < amount)
                {
                    _ammoSupplies[0].DestroySupply();
                    _ammoSupplies.RemoveAt(0);
                }

                takenAmmoFromAll += takenAmmoFromOne;

                if (takenAmmoFromAll == amount)
                    return true;

                if(takenAmmoFromAll > amount)
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
    }
}
