using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.UIElements;

namespace Game.Audio
{
    [RequireComponent(typeof(FmodEvents))]
    public class AudioManager : MonoBehaviour
    {
        [Header("Volume")]
        [Range(0, 1)]
        public float MasterVolume = 1;
        [Range(0, 1)]
        public float SfxVolume = 1;
        [Range(0, 1)]
        public float MusicVolume = 1;

        private List<EventInstance> _eventInstances = new List<EventInstance>();
        private List<StudioEventEmitter> _eventEmitters = new List<StudioEventEmitter>();

        private Bus _masterBus;
        private Bus _sfxBus;
        private Bus _musicBus;

        private void Awake()
        {
            GetBuses();
        }

        private void Update()
        {
            SetVolumes();
        }

        private void OnDestroy()
        {
            CleanUp();
        }

        public EventInstance CreateEventInstance(EventReference eventReference)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
            _eventInstances.Add(eventInstance);
            return eventInstance;
        }

        public EventInstance CreateEventInstance(EventReference eventReference, Transform parent)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
            RuntimeManager.AttachInstanceToGameObject(eventInstance, parent);
            _eventInstances.Add(eventInstance);
            return eventInstance;
        }

        public EventInstance CreateEventInstance(EventReference eventReference, Vector3 position)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
            eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            _eventInstances.Add(eventInstance);
            return eventInstance;
        }

        public EventInstance PlayOneShot(EventReference eventReference)
        {
            EventInstance eventInstance = CreateEventInstance(eventReference);
            eventInstance.start();
            eventInstance.release();
            return eventInstance;
        }

        public EventInstance PlayOneShot(EventReference eventReference, Transform parent)
        {
            EventInstance eventInstance = CreateEventInstance(eventReference, parent);
            eventInstance.start();
            eventInstance.release();
            return eventInstance;
        }

        public EventInstance PlayOneShot(EventReference eventReference, Vector3 position)
        {
            EventInstance eventInstance = CreateEventInstance(eventReference, position);
            eventInstance.start();
            eventInstance.release();
            return eventInstance;
        }

        public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, StudioEventEmitter eventEmitter)
        {
            eventEmitter.EventReference = eventReference;
            _eventEmitters.Add(eventEmitter);
            return eventEmitter;
        }

        private void GetBuses()
        {
            _masterBus = RuntimeManager.GetBus("bus:/");
            _sfxBus = RuntimeManager.GetBus("bus:/SFX");
            _musicBus = RuntimeManager.GetBus("bus:/Music");
        }

        private void SetVolumes()
        {
            _masterBus.setVolume(MasterVolume);
            _sfxBus.setVolume(SfxVolume);
            _musicBus.setVolume(MusicVolume);
        }

        private void CleanUp()
        {
            foreach (EventInstance eventInstance in _eventInstances)
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                eventInstance.release();
            }

            foreach (StudioEventEmitter eventEmitter in _eventEmitters)
            {
                eventEmitter.Stop();
            }
        }
    }
}
