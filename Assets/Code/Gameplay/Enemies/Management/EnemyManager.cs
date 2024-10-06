using Game.Management;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class EnemyManager : MonoBehaviour
    {
        [Inject] private List<EnemyBase> _roomEnemies;
        [Inject] private PlayerSceneManager _playerSceneManager;

        private void Awake()
        {
            _playerSceneManager.SetListOfRoomEnemies(_roomEnemies);
        }
    }
}
