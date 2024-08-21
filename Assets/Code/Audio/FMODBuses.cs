using FMOD.Studio;
using FMODUnity;

namespace Game.Audio
{
    public static class FMODBuses 
    {
        public static Bus Master => RuntimeManager.GetBus("bus:/");
        public static Bus SFX => RuntimeManager.GetBus("bus:/SFX");
        public static Bus Music => RuntimeManager.GetBus("bus:/Music");
    }
}