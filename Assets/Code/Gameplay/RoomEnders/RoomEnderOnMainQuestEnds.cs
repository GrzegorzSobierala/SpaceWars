using Game.Objectives;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Managment
{
    public class RoomEnderOnMainQuestEnds : MonoBehaviour
    {
        [Inject] private RoomQuestController _questController;
        [Inject] private PlayerSceneManager _playerSceneManager;

        private void Start()
        {
            _questController.OnMainQuestEnds += _playerSceneManager.EndRoom;
        }

        private void OnDestroy()
        {
            _questController.OnMainQuestEnds -= _playerSceneManager.EndRoom;
        }
    }
}
