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

        [Header("Sound")]
        [SerializeField] private EventReference _music;
        [SerializeField] private EventReference _alarmSfx;

        private Coroutine colorChangeCoroutine;
        private EventInstance _musicEventInstance;
        private EventInstance _alarmSfxEventInstance;

        private enum MusicMode
        {
            SNEAKY_MODE = 0,
            COMBAT_MODE = 1
        }

        private void Start()
        {
            _musicEventInstance = _audioManager.CreateEventInstance(_music);
            _musicEventInstance.start();
        }

        public void Activate()
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine);
            }

            colorChangeCoroutine = StartCoroutine(ChangeColorYoyo(Color.red));
            _alarmSfxEventInstance = _audioManager.CreateEventInstance(_alarmSfx);
            _alarmSfxEventInstance.start();
            _alarmSfxEventInstance.release();
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
            _musicEventInstance.setParameterByName("Mode", (float)mode);
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
