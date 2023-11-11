using Game.Management;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class EnemiesManager : MonoBehaviour
    {
        [Inject] private PlayerManager _playerManager;
        [Inject] private List<EnemyBase> _roomEnemies;

        private void Update()
        {
            
        }
    }
}
