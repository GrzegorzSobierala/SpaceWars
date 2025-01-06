using Game.Player.Ship;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Objectives
{
    [RequireComponent(typeof(Collider2D))]
    public class GoToTarget : MonoBehaviour
    {
        public event Action OnPlayerReachedTarget;

        private bool _wasTriggered = false;

        private void Awake()
        {
            SafeChecks();
        }

        private void OnTriggerEnter2D(Collider2D trigger)
        {
            TryTrigger(trigger);
        }

        private void TryTrigger(Collider2D trigger)
        {
            if (_wasTriggered) 
                return;

            if (!trigger.attachedRigidbody)
                return;

            if (!trigger.attachedRigidbody.GetComponent<PlayerMovement2D>())
                return;

            OnPlayerReachedTarget.Invoke();
            _wasTriggered = true;
        }

        private void SafeChecks()
        {
            Collider2D collider = GetComponent<Collider2D>();
            
            if(!collider.isTrigger)
            {
                Debug.LogError("Collider is not trigger", this);
            }
        }
    }
}
