using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Audio
{
    public class EventEmitter : StudioEventEmitter
    {
        [Inject] private AudioManager _audioManager;
        protected override void Start()
        {
            base.Start();
            _audioManager.StartEventEmitter(this);
        }
    }
}
