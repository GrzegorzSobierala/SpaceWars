using NaughtyAttributes;
using UnityEngine;
using Zenject;

namespace Game.Management
{
    [CreateAssetMenu(fileName = "ScenesData", menuName = "Managment/ScenesData")]
    public class ScenesData : ScriptableObjectInstaller<ScenesData>
    {
        [field: SerializeField, Scene] public string LoadingScene {get; private set;}
        [field: SerializeField, Scene] public string MainMeneScene {get; private set;}
        [field: SerializeField, Scene] public string PlayerScene {get; private set;}
        [field: SerializeField, Scene] public string HubScene {get; private set;}
        [field: SerializeField, Scene] public string[] RoomScenes {get; private set;}

        public override void InstallBindings()
        {
            Container.BindInstance(this);
        }
    }
}
