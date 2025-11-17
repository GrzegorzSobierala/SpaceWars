using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Game.Management
{
    public class GameInitiator : MonoBehaviour
    {
        [Inject] private GameSceneManager _gameSceneManager;

        [SerializeField] private InitialSceneType _initialSceneType = InitialSceneType.Room;
        [SerializeField, Scene] private string _initialRoom;

        private void Start()
        {
            StartCoroutine(InitGameOneFrameEnd());
        }

        private IEnumerator InitGameOneFrameEnd()
        {
            yield return new WaitForEndOfFrame();

            switch (_initialSceneType)
            {
                case InitialSceneType.Room:
                    _gameSceneManager.LoadPlayerWithRoom(_initialRoom);
                    break;
                case InitialSceneType.Hub:
                    _gameSceneManager.LoadHub();
                    break;
                case InitialSceneType.MainMenu:
                    _gameSceneManager.LoadMainMenu();
                    break;
                default:
                    Debug.LogError("Unknown initial scene type");
                    break;
            }

        }

        private enum InitialSceneType
        {
            Room = 0,
            Hub = 1,
            MainMenu = 2,
        }
    }
}
