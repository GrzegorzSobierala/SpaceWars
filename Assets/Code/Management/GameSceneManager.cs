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

        [Scene, SerializeField] private string _loadingScene;
        [Scene, SerializeField] private string _mainMeneScene;
        [Scene, SerializeField] private string _playerScene;
        [Scene, SerializeField] private string _hubScene;
        [Scene, SerializeField] private string[] _roomScenes;

        [Scene, SerializeField] private string _DEBUG_sceneToLoad;

        private const UnloadSceneOptions UNLOAD_OPTION = UnloadSceneOptions.UnloadAllEmbeddedSceneObjects;

        public TaskAwaiter LoadMainMenu()
        {
            string [] unloadScenes = _roomScenes.
                Concat(new string[] { _hubScene , _playerScene }).ToArray();

            return ReloadScenes(unloadScenes, new string[] {_mainMeneScene}).GetAwaiter();
        }

        public TaskAwaiter LoadHub()
        {
            string[] unloadScenes = _roomScenes.Concat(new string[] {_mainMeneScene}).ToArray();
            string[] loadScenes = new string[] { _playerScene, _hubScene };
            return ReloadScenes(unloadScenes, loadScenes).GetAwaiter();
        }

        public TaskAwaiter LoadRoom(string roomScene)
        {
            return ReloadScenes(new string[] {_hubScene}, new string[] {roomScene}).GetAwaiter();
        }

        private async Task ReloadScenes(string[] unloadScenes, string[] loadScenes)
        {
            await CreateTask(_sceneLoader.LoadSceneAsync(_loadingScene, LoadSceneMode.Additive));
            await TryUnloadScenes(unloadScenes);
            await CreateTask(Resources.UnloadUnusedAssets());
            await TryLoadScenes(loadScenes);
            await CreateTask(SceneManager.UnloadSceneAsync(_loadingScene, UNLOAD_OPTION));
        }

        private async Task TryUnloadScenes(string[] scenes)
        {
            //for (int i = 0; i < SceneManager.sceneCount; i++)
            //{
            //    Scene scene = SceneManager.GetSceneAt(i);
            //    if (!scenes.Contains(scene.name))
            //        continue;
            //
            //    await CreateTask(SceneManager.UnloadSceneAsync(scene.name, UNLOAD_OPTION));
            //}

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



            //for (int i = 0; i < SceneManager.sceneCount; i++)
            //{
            //    Scene scene = SceneManager.GetSceneAt(i);
            //    if (!scenes.Contains(scene.name))
            //    {
            //        await CreateTask(_sceneLoader.LoadSceneAsync(scene.name, LoadSceneMode.Additive));
            //        continue;
            //    }
            //
            //}
        }

        private Task CreateTask(AsyncOperation operation)
        {
            var tcs = new TaskCompletionSource<bool>();
            operation.completed += (AsyncOperation op) => tcs.SetResult(true);
            return tcs.Task;
        }

        [Button]
        private void DEBUG_MainMenu()
        {
            LoadMainMenu();
        }

        [Button]
        private void DEBUG_Hub()
        {
            LoadHub();
        }

        [Button]
        private void DEBUG_LoadRoom()
        {
            LoadRoom(_DEBUG_sceneToLoad);
        }
    }
}
