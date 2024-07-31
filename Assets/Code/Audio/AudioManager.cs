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

        [SerializeField] private List<EventInstance> _eventInstances = new List<EventInstance>();
        [SerializeField] private List<StudioEventEmitter> _eventEmitters = new List<StudioEventEmitter>();

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

        public EventInstance CreateEventInstance(EventReferenceScriptable eventReferenceScriptable)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventReferenceScriptable.EventReference);
            _eventInstances.Add(eventInstance);
            return eventInstance;

        }

        public EventInstance CreateEventInstance(EventReferenceScriptable eventReferenceScriptable, Transform parent)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventReferenceScriptable.EventReference);
            RuntimeManager.AttachInstanceToGameObject(eventInstance, parent);
            _eventInstances.Add(eventInstance);
            return eventInstance;
        }

        public EventInstance CreateEventInstance(EventReferenceScriptable eventReferenceScriptable, Vector3 position)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventReferenceScriptable.EventReference);
            eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            _eventInstances.Add(eventInstance);
            return eventInstance;
        }

        public EventInstance PlayOneShot(EventReferenceScriptable eventReferenceScriptable)
        {
            EventInstance eventInstance = CreateEventInstance(eventReferenceScriptable);
            eventInstance.start();
            eventInstance.release();
            return eventInstance;
        }

        public EventInstance PlayOneShot(EventReferenceScriptable eventReferenceScriptable, Transform parent)
        {
            EventInstance eventInstance = CreateEventInstance(eventReferenceScriptable, parent);
            eventInstance.start();
            eventInstance.release();
            return eventInstance;
        }

        public EventInstance PlayOneShot(EventReferenceScriptable eventReference, Vector3 position)
        {
            EventInstance eventInstance = CreateEventInstance(eventReference, position);
            eventInstance.start();
            eventInstance.release();
            return eventInstance;
        }

        /*
        public StudioEventEmitter InitializeEventEmitter(EventReferenceScriptable eventReferenceScriptable, StudioEventEmitter eventEmitter)
        {
            eventEmitter.EventReference = eventReferenceScriptable.EventReference;
            _eventEmitters.Add(eventEmitter);
            return eventEmitter;
        }
        */

        public void StartEventEmitter(StudioEventEmitter eventEmitter)
        {
            _eventEmitters.Add(eventEmitter);
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
