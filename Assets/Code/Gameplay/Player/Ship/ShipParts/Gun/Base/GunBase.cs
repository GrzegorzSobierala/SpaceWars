using System;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Player.Ship
{
    public abstract class GunBase : ShipPart, IGun
    {
        [SerializeField] protected UnityEvent OnShootEvent;

        protected abstract void OnShoot();

        public void Shoot()
        {
            OnShoot();
            OnShootEvent?.Invoke();
        }
    }
}
