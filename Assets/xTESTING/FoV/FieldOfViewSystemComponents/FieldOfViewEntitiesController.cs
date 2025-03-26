using Game.Management;
using System;
using UnityEngine;

namespace Game.Physics
{
    public class FieldOfViewEntitiesController : MonoBehaviour
    {
        public event Action<Collider2D> OnTriggerEnterEvent;
        public event Action<Collider2D> OnTriggerExitEvent;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            OnTriggerEnterEvent.Invoke(collision);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if(GameManager.IsGameQuitungOrSceneUnloading(gameObject))
                return;

            OnTriggerExitEvent.Invoke(collision);
        }
    }
}
