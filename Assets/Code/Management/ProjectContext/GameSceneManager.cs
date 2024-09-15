using NaughtyAttributes;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Game.Management
{
    public class GameSceneManager : MonoBehaviour
    {
        [Inject] private ZenjectSceneLoader _sceneLoader;

        [SerializeField] private ScenesData _data;

        [Scene, SerializeField] private string _DEBUG_sceneToLoad;

        private const UnloadSceneOptions UNLOAD_OPTION = UnloadSceneOptions.UnloadAllEmbeddedSceneObjects;

        private bool _isLoading = false;

        public TaskAwaiter LoadMainMenu()
        {
            string [] unloadScenes = _data.RoomScenes.
                Concat(new string[] { _data.HubScene , _data.PlayerScene }).ToArray();

            return ReloadScenes(unloadScenes, new string[] { _data.MainMeneScene }).GetAwaiter();
        }

        public TaskAwaiter LoadHub()
        {
            string[] unloadScenes = _data.RoomScenes.
                Concat(new string[] { _data.MainMeneScene }).ToArray();
            string[] loadScenes = new string[] { _data.PlayerScene, _data.HubScene };
            return ReloadScenes(unloadScenes, loadScenes).GetAwaiter();
        }

        public TaskAwaiter LoadRoom(string roomScene)
        {
            return ReloadScenes(new string[] { _data.HubScene }, new string[] {roomScene}).GetAwaiter();
        }

        public TaskAwaiter ReloadCurrentRoom()
        {
            string roomName = string.Empty;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene activeScene = SceneManager.GetSceneAt(i);
                if (_data.RoomScenes.Contains(activeScene.name))
                {
                    roomName = activeScene.name;
                    break;
                }
            }

            if(roomName == string.Empty)
            {
                Debug.LogError("ThereIsNoRoomToReload");
                return new TaskAwaiter();
            }

            return ReloadScenes(new string[] { roomName }, new string[] { roomName }).GetAwaiter();
        }

        private async Task ReloadScenes(string[] unloadScenes, string[] loadScenes)
        {
            if(!TryMarkLoading())
                return;

            await CreateTask(_sceneLoader.LoadSceneAsync(_data.LoadingScene, LoadSceneMode.Additive));
            await TryUnloadScenes(unloadScenes);
            await CreateTask(Resources.UnloadUnusedAssets());
            await TryLoadScenes(loadScenes);
            await CreateTask(SceneManager.UnloadSceneAsync(_data.LoadingScene, UNLOAD_OPTION));
            TryUnmarkLoading();
        }

        private async Task TryUnloadScenes(string[] scenes)
        {
            foreach (var scene in scenes)
            {
                bool contains = false;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene activeScene = SceneManager.GetSceneAt(i);
                    if (scene == activeScene.name)
                    {
                        contains = true;
                        break;
                    }
                }

                if (contains)
                {
                    await CreateTask(SceneManager.UnloadSceneAsync(scene, UNLOAD_OPTION));
                }
            }
        }

        private async Task TryLoadScenes(string[] scenes)
        {
            foreach (var scene in scenes)
            {
                bool contains = false;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene activeScene = SceneManager.GetSceneAt(i);
                    if (scene == activeScene.name)
                    {
                        contains = true;
                        break;
                    }
                }

                if (!contains)
                {
                    await CreateTask(_sceneLoader.LoadSceneAsync(scene, LoadSceneMode.Additive));
                }
            }
        }

        private Task CreateTask(AsyncOperation operation)
        {
            var tcs = new TaskCompletionSource<bool>();
            operation.completed += (AsyncOperation op) => tcs.SetResult(true);
            return tcs.Task;
        }

        private bool TryMarkLoading()
        {
            if(_isLoading)
            {
                Debug.LogError("Is loading");
                return false;
            }
            else
            {
                _isLoading = true;
                return true;
            }
        }

        private bool TryUnmarkLoading()
        {
            if (_isLoading)
            {
                _isLoading = false;
                return true;
            }
            else
            {
                Debug.LogError("Isn't loading");
                return false;
            }
        }

        [Button]
        private void DEBUG_LoadRoom()
        {
            LoadRoom(_DEBUG_sceneToLoad);
        }
    }
}
