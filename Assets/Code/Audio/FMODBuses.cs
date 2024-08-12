using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Game.Audio
{
    public class FMODBuses : MonoBehaviour
    {
        public Bus MasterBus { get; private set; }
        public Bus SfxBus { get; private set; }
        public Bus MusicBus { get; private set; }

        private void Awake()
        {
            MasterBus = RuntimeManager.GetBus("bus:/");
            SfxBus = RuntimeManager.GetBus("bus:/SFX");
            MusicBus = RuntimeManager.GetBus("bus:/Music");
        }
    }
}