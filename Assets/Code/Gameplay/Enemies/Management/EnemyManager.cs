using Game.Player.Ship;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class EnemyManager : MonoBehaviour
    {
        [Inject] private HashSet<EnemyBase> _roomEnemies;
        [Inject] private PlayerSceneManager _playerSceneManager;
        [Inject] private DiContainer _container;

        private void Awake()
        {
            _playerSceneManager.SetRoomEnemies(_roomEnemies);
        }

        public EnemyBase SpawnEnemy(EnemyBase prototype, Vector2 pos, float rot)
        {
            GameObject newEnemyGo = _container.InstantiatePrefab(prototype.gameObject,pos, 
                Quaternion.Euler(0, 0, rot), transform);
            EnemyBase newEnemy = newEnemyGo.GetComponent<EnemyBase>();
            _roomEnemies.Add(newEnemy);
            return newEnemy;
        }
    }
}
