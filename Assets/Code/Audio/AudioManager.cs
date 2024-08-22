using UnityEngine;
using FMOD.Studio;

namespace Game.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public void SetBusVolume(Bus bus, float volume)
        {
            volume = Mathf.Clamp(volume, 0f, 1f);
            bus.setVolume(volume);
        }
    }
}