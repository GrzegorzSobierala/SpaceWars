using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Game.Audio
{
    public class BackgroundMusicManager : MonoBehaviour
    {
        private EventInstance _musicEventInstance;

        public void SetMusic(EventReference eventRef)
        {
            if (!_musicEventInstance.isValid())
            {
                InitializeMusic(eventRef);
            }
            else
            {
                ChangeMusic(eventRef);
            }
        }

        public void SetMusicMode(LevelMusicMode mode)
        {
            _musicEventInstance.setParameterByName("Mode", (float)mode);
        }

        private void InitializeMusic(EventReference eventRef)
        {
            _musicEventInstance = RuntimeManager.CreateInstance(eventRef);
            _musicEventInstance.start();
        }

        private void ChangeMusic(EventReference eventRef)
        {
            //probably this is the place for some smooth transitions if needed
            _musicEventInstance.release();
            _musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _musicEventInstance = RuntimeManager.CreateInstance(eventRef);
        }

        public enum LevelMusicMode
        {
            SNEAK_MODE = 0,
            COMBAT_MODE = 1
        }
    }
}