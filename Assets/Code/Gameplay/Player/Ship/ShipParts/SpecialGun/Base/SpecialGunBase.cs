using Game.Player.Ship;
using Game.Player;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Player.Ship

{
    public abstract class SpecialGunBase : ShipPart, IGun
    {
        [SerializeField] protected UnityEvent OnShootEvent;

        protected abstract void OnShoot();

        public void TryShoot()
        {
            OnShoot();
            OnShootEvent?.Invoke();
        }
    }
}
