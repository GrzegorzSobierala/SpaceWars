using FMOD.Studio;
using FMODUnity;
using Game.Audio;
using System.Collections;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Player.UI
{
    public class TestAlarmUI : MonoBehaviour
    {
        [Inject] private BackgroundMusicManager _music;

        [SerializeField] private TextMeshProUGUI _textMesh;
        [SerializeField] private float _colorChangeDuration = 1f;
        [SerializeField] private int _maxRepeats = 5;

        [SerializeField] private EventReference _musicEventRef;
        [SerializeField] private EventReference _alarmEventRef;

        private Coroutine colorChangeCoroutine;

        private void Start()
        {
            _music.SetMusic(_musicEventRef);
        }

        public void Activate()
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine);
            }
            colorChangeCoroutine = StartCoroutine(ChangeColorYoyo(Color.red));

            RuntimeManager.PlayOneShot(_alarmEventRef);
            _music.SetMusicMode(BackgroundMusicManager.LevelMusicMode.COMBAT_MODE);
        }

        public void Deactivate()
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine);
            }

            _textMesh.color = Color.white;
            colorChangeCoroutine = null;

            _music.SetMusicMode(BackgroundMusicManager.LevelMusicMode.SNEAK_MODE);
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