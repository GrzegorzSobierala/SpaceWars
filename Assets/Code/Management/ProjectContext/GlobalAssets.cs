using Game.Room.Enemy;
using UnityEngine;

namespace Game.Management
{
    public class GlobalAssets : MonoBehaviour
    {
        [SerializeField] private Material _testMaterial;
        [SerializeField] private EnemySeeEnemyLine _enemySeeEnemyLine;

        public Material TestMaterial => _testMaterial;
        public EnemySeeEnemyLine EnemySeeEnemyLine => _enemySeeEnemyLine;
    }
}
