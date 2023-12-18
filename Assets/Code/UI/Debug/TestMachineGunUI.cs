using TMPro;
using UnityEngine;
using Zenject;
using Game.Player.Ship;
using Game.Management;

namespace Game.Player.UI
{
    public class TestMachineGunUI : MonoBehaviour
    {
        [Inject] PlayerManager _playerManager;

        [SerializeField] private TextMeshProUGUI _textMesh;

        private void Update()
        {
            if (_playerManager.ModuleHandler.CurrentGun is not MachineGun)
            {
                _textMesh.gameObject.SetActive(false);
                return;
            }

            MachineGun machineGun = (MachineGun)_playerManager.ModuleHandler.CurrentGun;

            string text =  $"{machineGun.CurrentAmmo}/{machineGun.MaxAmmo}";
            _textMesh.text = text;

            _textMesh.gameObject.SetActive(true);
        }
    }
}
