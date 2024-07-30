using FMOD.Studio;
using FMODUnity;
using Game.Audio;
using Game.Input.System;
using System.Collections;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Player.UI
{
    public class TestAlarmUI : MonoBehaviour
    {
        [Inject] private AudioManager _audioManager;
        [Inject] private FmodEvents _fmodEvents;

        [SerializeField] private TextMeshProUGUI _textMesh;
        [SerializeField] private float _colorChangeDuration = 1f;
        [SerializeField] private int _maxRepeats = 5;

        private Coroutine colorChangeCoroutine;
        private EventInstance _musicEvent;
        private EventInstance _alarmSfxEvent;

        private enum MusicMode
        {
            SNEAKY_MODE = 0,
            COMBAT_MODE = 1
        }

        private void Start()
        {
            _musicEvent = _audioManager.CreateEventInstance(_fmodEvents.Music);
            _musicEvent.start();
        }

        public void Activate()
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine);
            }

            colorChangeCoroutine = StartCoroutine(ChangeColorYoyo(Color.red));
            _alarmSfxEvent = _audioManager.CreateEventInstance(_fmodEvents.Alarm);
            _alarmSfxEvent.start();
            _alarmSfxEvent.release();
            SetMusicMode(MusicMode.COMBAT_MODE);
        }

        public void Deactivate()
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine);
            }

            _textMesh.color = Color.white;
            colorChangeCoroutine = null;
            SetMusicMode(MusicMode.SNEAKY_MODE);
        }

        private void SetMusicMode(MusicMode mode)
        {
            _musicEvent.setParameterByName("Mode", (float)mode);
        }

        private IEnumerator ChangeColorYoyo(Color targetColor)
        {
            Color startColor = _textMesh.color;
            float elapsedTime = 0f;
            float animTime = _colorChangeDuration * _maxRepeats + _colorChangeDuration / 2;
            float animeEndTime = Time.time + animTime;

            while (animeEndTime > Time.time)
            {
                float t = Mathf.PingPong(elapsedTime / _colorChangeDuration * 2, 1f);
                _textMesh.color = Color.Lerp(startColor, targetColor, t);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _textMesh.color = targetColor;
        }
    }
}
