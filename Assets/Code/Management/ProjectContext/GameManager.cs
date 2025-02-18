using Game.Utility;
using UnityEngine;

namespace Game.Management
{
    public class GameManager : MonoBehaviour
    {
        private static bool _isGameQuiting = false;

        public static bool IsGameQuiting => _isGameQuiting;

        public static bool IsGameQuitungOrSceneUnloading(GameObject go)
        {
            Utils.SceneLoadingState? loadingState = Utils.GetLoadingState(go.scene);
            bool isUnloading = loadingState == Utils.SceneLoadingState.Unloading;

            return _isGameQuiting || isUnloading;
        }

        private void Awake()
        {
            Application.quitting += SetGameQuiting;
        }

        private void SetGameQuiting()
        {
            _isGameQuiting = true;
        }
    }
}
