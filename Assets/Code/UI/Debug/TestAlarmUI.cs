using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.Player.UI
{
    public class TestAlarmUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textMesh;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private float colorChangeDuration = 1f; // Adjust the duration as needed
        private Coroutine colorChangeCoroutine;

        public void Activate()
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine);
            }

            colorChangeCoroutine = StartCoroutine(ChangeColorYoyo(Color.red));
            _audioSource.Play();
        }

        public void Deactivate()
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine);
            }

            _textMesh.color = Color.white;
            colorChangeCoroutine = null;
        }

        private IEnumerator ChangeColorYoyo(Color targetColor)
        {
            Color startColor = _textMesh.color;
            float elapsedTime = 0f;

            while (true)
            {
                float t = Mathf.PingPong(elapsedTime / colorChangeDuration, 1f);
                _textMesh.color = Color.Lerp(startColor, targetColor, t);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}
