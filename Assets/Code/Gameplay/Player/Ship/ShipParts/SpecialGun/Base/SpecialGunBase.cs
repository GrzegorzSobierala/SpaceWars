using UnityEngine;
using UnityEngine.Events;

namespace Game.Player.Ship

{
    public abstract class SpecialGunBase : ShipPart, IGun
    {
        [SerializeField] protected UnityEvent OnShootEvent;

        public abstract bool IsGunReadyToShoot { get; }

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
