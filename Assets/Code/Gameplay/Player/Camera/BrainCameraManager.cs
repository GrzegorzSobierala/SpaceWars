using Game.Player.Control;
using UnityEngine;
using Zenject;

namespace Game.Management
{
    public class BrainCameraManager : MonoBehaviour
    {
        [Inject] private CursorCamera _cursorCamera;

        private void Start()
        {
            _cursorCamera.OnCameraActivation();
        }
    }
}
