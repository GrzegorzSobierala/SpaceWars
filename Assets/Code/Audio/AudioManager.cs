using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.UIElements;
using System.IO;
using UnityEngine.Rendering;

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

            /*
            RuntimeManager.StudioSystem.getBankList(out Bank[] banks);
            foreach(Bank bank in banks)
            {
                bank.getEventList(out EventDescription[] eventDescs);
                //bank.getPath(out string path);
                //string text = "Bank: " + path;
                foreach (EventDescription eventDesc in eventDescs)
                {
                    //eventDesc.getPath(out string paths);
                    //text += "\nEvent path:" + paths;
                    eventDesc.getInstanceList(out EventInstance[] eventInstances);
                    foreach(EventInstance eventInstance in eventInstances)
                    {
                        //text += "\nInstance: " + eventInstance.GetHashCode();
                    }
                }
                //Debug.Log(text);
            }
            */
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
