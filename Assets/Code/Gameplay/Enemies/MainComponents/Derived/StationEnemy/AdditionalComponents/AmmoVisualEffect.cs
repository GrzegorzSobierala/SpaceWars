using Game.Utility;
using System.Collections;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class AmmoVisualEffect : MonoBehaviour
    {
        [SerializeField] private float _lifeTime = 0.25f;

        private bool _isPlayingDestroy = false;
        private Coroutine _currentCoroutine = null;

        [SerializeField] private GameObject _loadEffect;
        [SerializeField] private GameObject _destroyEffect;

        public void PlayEffect()
        {
            if (_isPlayingDestroy)
                return;

            _loadEffect.SetActive(true);
            this.StopAndClearCoroutine(ref _currentCoroutine);
            _currentCoroutine = StartCoroutine(DisableAfterTime());
        }

        public void PlayEffectAndDestroy(Transform newParent)
        {
            if (_isPlayingDestroy)
                return;

            transform.SetParent(newParent);
            _destroyEffect.SetActive(true);
            this.StopAndClearCoroutine(ref _currentCoroutine);
            _currentCoroutine = StartCoroutine(DestroyAfterTime());
            _isPlayingDestroy = true;
        }

        IEnumerator DisableAfterTime()
        {
            yield return new WaitForSeconds(_lifeTime);
            _loadEffect.SetActive(false);
        }

        IEnumerator DestroyAfterTime()
        {
            yield return new WaitForSeconds(_lifeTime);
            Destroy(gameObject);
        }
    }
}
