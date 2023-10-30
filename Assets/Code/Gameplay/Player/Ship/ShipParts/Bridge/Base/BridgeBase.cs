using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public abstract class BridgeBase : ShipPart
    {
        [Inject] protected Rigidbody2D _body;
        [Inject] protected PlayerMovement2D _playerMovement;

        [Inject] private PlayerModuleHandler _playerModuleHandler;


        protected PlayerGunModuleBase Gun => _playerModuleHandler.CurrentGun;

        public abstract void OnStartAim();

        public abstract void OnEndAim();
    }
}
