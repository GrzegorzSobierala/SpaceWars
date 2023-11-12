using Game.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class EnemiesManager : MonoBehaviour
    {
        public static Action OnRoomClear;

        [Inject] private PlayerManager _playerManager;
        [Inject] private List<EnemyBase> _roomEnemies;

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
                OnRoomClear?.Invoke();
                _roomClear = true;
            }
        }
    }
}
