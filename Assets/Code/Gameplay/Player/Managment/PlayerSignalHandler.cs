using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.Combat;

namespace Game.Player
{
    public class PlayerSignalHandler : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            _signalBus.Fire(new OnPlayerCollision2DEnterSignal(collision));
        }
    }
}
