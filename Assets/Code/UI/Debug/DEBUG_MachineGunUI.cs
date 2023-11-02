using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;
using Game.Player.Ship;

namespace Game.Player.UI
{
    public class DEBUG_MachineGunUI : MonoBehaviour
    {
        [Inject] PlayerModuleHandler _moduleHandler;

        [SerializeField] private TextMeshProUGUI _textMesh;

        private void Update()
        {
            if (_moduleHandler.CurrentGun is not PlayerMachineGun)
                return;

            PlayerMachineGun machineGun = (PlayerMachineGun)_moduleHandler.CurrentGun;

            string text =  $"{machineGun.CurrentAmmo}/{machineGun.MaxAmmo}";
            _textMesh.text = text;
        }
    }
}
