using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;
using Game.Player.Ship;
using Game.Management;

namespace Game.Player.UI
{
    public class DEBUG_MachineGunUI : MonoBehaviour
    {
        [Inject] PlayerManager _playerManager;

        [SerializeField] private TextMeshProUGUI _textMesh;

        private void Update()
        {
            if (_playerManager.ModuleHandler.CurrentGun is not PlayerMachineGun)
                return;

            PlayerMachineGun machineGun = (PlayerMachineGun)_playerManager.ModuleHandler.CurrentGun;

            string text =  $"{machineGun.CurrentAmmo}/{machineGun.MaxAmmo}";
            _textMesh.text = text;
        }
    }
}
