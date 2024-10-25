using UnityEngine;
using FMOD.Studio;

namespace Game.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _gameVolume = 1f;

        private void Update()
        {
            SetBusVolume(FMODBuses.Master, _gameVolume);
        }

        public void SetBusVolume(Bus bus, float volume)
        {
            volume = Mathf.Clamp(volume, 0f, 1f);
            bus.setVolume(volume);
        }
    }
}