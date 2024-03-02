using Game.Management;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class EnemyManager : MonoBehaviour
    {
        [Inject] private PlayerManager _playerManager;
        [Inject] private List<EnemyBase> _roomEnemies;
        [Inject] private TestSceneManager _testSceneManager;

        private bool _roomClear = false;

        private void Update()
        {
            UpdateRoomClearCheck();
        }

        private void UpdateRoomClearCheck()
        {
            if (_roomClear)
                return;

            bool allEnemiesDed = true;
            foreach (var item in _roomEnemies)
            {
                if (item != null)
                {
                    allEnemiesDed = false;
                    break;
                }
            }

            if(allEnemiesDed)
            {
                _testSceneManager.OnRoomMainObjectiveCompleted.Invoke();
                _roomClear = true;
            }
        }
    }
}
