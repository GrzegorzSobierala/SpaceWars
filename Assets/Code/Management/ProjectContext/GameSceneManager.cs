using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Game.Management
{
    public class GameSceneManager : MonoBehaviour
    {
        [Inject] private ZenjectSceneLoader _sceneLoader;
        [Inject] private ScenesData _data;

        [Scene, SerializeField] private string _DEBUG_sceneToLoad;

        private const UnloadSceneOptions UNLOAD_OPTION = UnloadSceneOptions.UnloadAllEmbeddedSceneObjects;

        private Coroutine loadCoroutine;

        public void LoadMainMenu(Action onEnd = null)
        {
            string [] unloadScenes = _data.RoomScenes.
                Concat(new string[] { _data.HubScene , _data.PlayerScene }).ToArray();

            StartSwitchingScenes(unloadScenes, new string[] { _data.MainMeneScene }, onEnd);
        }

        public void LoadHub(Action onEnd = null)
        {
            string[] unloadScenes = _data.RoomScenes.
                Concat(new string[] { _data.MainMeneScene }).ToArray();
            string[] loadScenes = new string[] { _data.PlayerScene, _data.HubScene };
            StartSwitchingScenes(unloadScenes, loadScenes, onEnd);
        }

        public void LoadRoom(string roomScene, Action onEnd = null)
        {
            StartSwitchingScenes(new string[] { _data.HubScene }, new string[] {roomScene}, onEnd);
        }

        public void ReloadCurrentRoom(Action onEnd = null)
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

            if (roomName == string.Empty)
            {
                Debug.LogError("ThereIsNoRoomToReload");
            }

            StartSwitchingScenes(new string[] { roomName }, new string[] { roomName }, onEnd);
        }

        private void StartSwitchingScenes(string[] unloadScenes, string[] loadScenes, Action onEnd)
        {
            if (loadCoroutine != null)
            {
                StopCoroutine(loadCoroutine);
                Debug.LogError("Stoped loading scene and start new loading");
            }

            loadCoroutine = StartCoroutine(SwitchScenes(unloadScenes, loadScenes, onEnd));
        }

        private IEnumerator SwitchScenes(string[] unloadScenes, string[] loadScenes, Action onEnd)
        {
            yield return SceneManager.LoadSceneAsync(_data.LoadingScene, LoadSceneMode.Additive);
            yield return TryUnloadScenes(unloadScenes);
            yield return Resources.UnloadUnusedAssets();
            yield return TryLoadScenes(loadScenes);
            yield return SceneManager.UnloadSceneAsync(_data.LoadingScene, UNLOAD_OPTION);

            loadCoroutine = null;
            onEnd?.Invoke();
        }

        private IEnumerator TryUnloadScenes(string[] scenes)
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
                    yield return SceneManager.UnloadSceneAsync(scene, UNLOAD_OPTION);
                }
            }
        }

        private IEnumerator TryLoadScenes(string[] scenes)
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
                    yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
                }
            }
        }

        private string[] GetActiveScenes()
        {
            List<string> roomSceneNames = new();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == _data.LoadingScene)
                {
                    continue;
                }

                roomSceneNames.Add(SceneManager.GetSceneAt(i).name);
            }
            return roomSceneNames.ToArray();
        }

        [Button]
        private void DEBUG_LoadRoom()
        {
            LoadRoom(_DEBUG_sceneToLoad);
        }
    }
}
