using System;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Player.Ship
{
    public abstract class MainGunBase : ShipPart, IGun
    {
        [SerializeField] protected UnityEvent OnShootEvent;

        protected abstract bool OnTryShoot();

        public void TryShoot()
        {
            if(OnTryShoot())
            {
                OnShootEvent?.Invoke();
            }
        }
    }
}
