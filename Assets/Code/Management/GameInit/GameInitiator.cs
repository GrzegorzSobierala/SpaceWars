using System.Collections;
using UnityEngine;
using Zenject;

namespace Game.Management
{
    public class GameInitiator : MonoBehaviour
    {
        [Inject] private GameSceneManager _gameSceneManager;

        private void Start()
        {
            //_gameSceneManager.LoadHub();
            StartCoroutine(LoadHubOneFrameEnd());
        }

        private IEnumerator LoadHubOneFrameEnd()
        {
            yield return new WaitForEndOfFrame();

            _gameSceneManager.LoadHub();
        }
    }
}
