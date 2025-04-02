using Game.Room.Enemy;
using UnityEngine;

namespace Game.Management
{
    public class GlobalAssets : MonoBehaviour
    {
        [SerializeField] private Material _testMaterial;
        [SerializeField] private EnemySeeEnemyArrow _enemySeeEnemyLine;

        public Material TestMaterial => _testMaterial;
        public EnemySeeEnemyArrow EnemySeeEnemyLine => _enemySeeEnemyLine;
    }
}
