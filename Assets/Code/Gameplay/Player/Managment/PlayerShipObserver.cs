using UnityEngine;
using Zenject;

namespace Game.Player
{
    public class PlayerShipObserver : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            _signalBus.Fire(new PlayerCollisionEnter2DSignal(collision));
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            _signalBus.Fire(new PlayerCollisionStay2DSignal(collision));
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            _signalBus.Fire(new PlayerCollisionExit2DSignal(collision));
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            _signalBus.Fire(new PlayerTriggerEnter2DSignal(collider));
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            _signalBus.Fire(new PlayerTriggerStay2DSignal(collider));
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            _signalBus.Fire(new PlayerTriggerExit2DSignal(collider));
        }
    }
}
