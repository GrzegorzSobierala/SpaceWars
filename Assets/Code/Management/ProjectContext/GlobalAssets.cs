using Game.Room.Enemy;
using UnityEngine;

namespace Game.Management
{
    public class GlobalAssets : MonoBehaviour
    {
        [SerializeField] private Material _testMaterial;
        [SerializeField] private Material _getDamageMaterial;
        [SerializeField] private EnemySeeEnemyArrow _enemySeeEnemyLine;
        [SerializeField] private float _getDamageVisualEfectTime = 0.5f;

        public Material TestMaterial => _testMaterial;
        public Material GetDamageMaterial => _getDamageMaterial;
        public EnemySeeEnemyArrow EnemySeeEnemyLine => _enemySeeEnemyLine;
        public float GetDamageVisualEfectTime => _getDamageVisualEfectTime;
    }
}
