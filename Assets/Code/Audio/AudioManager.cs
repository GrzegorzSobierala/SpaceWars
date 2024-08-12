using UnityEngine;
using FMOD.Studio;
using Zenject;

namespace Game.Audio
{
    [RequireComponent(typeof(FMODEvents))]
    [RequireComponent(typeof(FMODBuses))]
    public class AudioManager : MonoBehaviour
    {
        [Header("Volume")]
        [Range(0, 1)] public float MasterVolume = 1;
        [Range(0, 1)] public float SfxVolume = 1;
        [Range(0, 1)] public float MusicVolume = 1;

        private void SetBusVolume(Bus bus, float volume)
        {
            volume = Mathf.Clamp(volume, 0f, 1f);
            bus.setVolume(volume);
        }
    }
}