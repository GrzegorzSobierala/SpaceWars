using NaughtyAttributes;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Game.Management
{
    [CreateAssetMenu(fileName = "ScenesData", menuName = "Managment/ScenesData")]
    public class ScenesData : ScriptableObjectInstaller<ScenesData>
    {
        [field: SerializeField, Scene] public string InitScene { get; private set; }
        [field: SerializeField, Scene] public string LoadingScene {get; private set;}
        [field: SerializeField, Scene] public string MainMeneScene {get; private set;}
        [field: SerializeField, Scene] public string PlayerScene {get; private set;}
        [field: SerializeField, Scene] public string HubScene {get; private set;}

        public string[] RoomScenes
        {
            get
            {
                List<string> roomSceneNames = new();
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string pathScene = SceneUtility.GetScenePathByBuildIndex(i);
                    string scene = Path.GetFileNameWithoutExtension(pathScene);
                    if(scene == InitScene || scene == LoadingScene || scene == MainMeneScene || 
                        scene == PlayerScene || scene == HubScene)
                    {
                        continue;
                    }

                    roomSceneNames.Add(scene);
                }
                return roomSceneNames.ToArray();
            }
        }

        public override void InstallBindings()
        {
            Container.BindInstance(this);
        }
    }
}
