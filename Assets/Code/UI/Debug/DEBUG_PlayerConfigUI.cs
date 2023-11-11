using Game.Management;
using Game.Player.Ship;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game
{
    public class DEBUG_PlayerConfigUI : MonoBehaviour
    {
        [Inject] private PlayerManager _playerManager;

        [SerializeField] private Button _setNextHullButton;
        [SerializeField] private Button _setPreviiusHullButton;
        [SerializeField] private Button _setNextGunButton;
        [SerializeField] private Button _setPreviousGunButton;
        [SerializeField] private Button _setNextBridgeButton;
        [SerializeField] private Button _setPreviousBridgeButton;

        private ModuleCreator ModuleCreator => _playerManager.ModuleCreator;

        private void OnEnable()
        {
            SubscribeButtons();
        }

        private void OnDisable()
        {
            UnsubscribeButtons();
        }

        private void SubscribeButtons()
        {
            _setNextHullButton.onClick.AddListener(ModuleCreator.SetNextHull);
            _setPreviiusHullButton.onClick.AddListener(ModuleCreator.SetPreviusHull);
            _setNextGunButton.onClick.AddListener(ModuleCreator.SetNextGun);
            _setPreviousGunButton.onClick.AddListener(ModuleCreator.SetPreviusGun);
            _setNextBridgeButton.onClick.AddListener(ModuleCreator.SetNextBridge);
            _setPreviousBridgeButton.onClick.AddListener(ModuleCreator.SetPreviusBridge);
        }

        private void UnsubscribeButtons()
        {
            _setNextHullButton.onClick.RemoveListener(ModuleCreator.SetNextHull);
            _setPreviiusHullButton.onClick.RemoveListener(ModuleCreator.SetPreviusHull);
            _setNextGunButton.onClick.RemoveListener(ModuleCreator.SetNextGun);
            _setPreviousGunButton.onClick.RemoveListener(ModuleCreator.SetPreviusGun);
            _setNextBridgeButton.onClick.RemoveListener(ModuleCreator.SetNextBridge);
            _setPreviousBridgeButton.onClick.RemoveListener(ModuleCreator.SetPreviusBridge);
        }
    }
}
