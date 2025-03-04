using NaughtyAttributes;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class BasicEnemySpawner : MonoBehaviour
    {
        [Inject] private EnemyManager _enemyManager;

        [SerializeField] private EnemyBase test;

        [Button]
        public void SpawnEnemy()
        {
            _enemyManager.SpawnEnemy(test, transform.position, transform.eulerAngles.z);
        }

    }
}
