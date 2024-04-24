using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.Player.UI
{
    public class TestAlarmUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textMesh;
        [SerializeField] private AudioSource _alarmAudioSource;
        [SerializeField] private AudioSource _sneakyMusicAudioSource;
        [SerializeField] private AudioSource _combatMusicAudioSource;
        [SerializeField] private float _colorChangeDuration = 1f;
        [SerializeField] private int _maxRepeats = 5;
        private Coroutine colorChangeCoroutine;

        private void Start()
        {
            _sneakyMusicAudioSource.Play();
            _combatMusicAudioSource.Stop();
        }

        public void Activate()
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine);
            }

            colorChangeCoroutine = StartCoroutine(ChangeColorYoyo(Color.red));
            _alarmAudioSource.Play();
            _sneakyMusicAudioSource.Stop();
            _combatMusicAudioSource.Play();
        }

        public void Deactivate()
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine);
            }

            _textMesh.color = Color.white;
            colorChangeCoroutine = null;
            _sneakyMusicAudioSource.Play();
            _combatMusicAudioSource.Stop();
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
