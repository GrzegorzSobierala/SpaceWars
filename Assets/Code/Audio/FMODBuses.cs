using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Game.Audio
{
    public class FMODBuses : MonoBehaviour
    {
        public Bus Master { get; private set; }
        public Bus SFX { get; private set; }
        public Bus Music { get; private set; }

        private void Awake()
        {
            Master = RuntimeManager.GetBus("bus:/");
            SFX = RuntimeManager.GetBus("bus:/SFX");
            Music = RuntimeManager.GetBus("bus:/Music");
        }
    }
}