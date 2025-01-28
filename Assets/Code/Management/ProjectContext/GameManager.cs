using UnityEngine;

namespace Game.Management
{
    public class GameManager : MonoBehaviour
    {
        private static bool _isGameQuiting = false;

        public static bool IsGameQuiting => _isGameQuiting;

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
