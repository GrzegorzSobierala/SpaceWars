using UnityEngine;

namespace Game.Management
{
    public class GlobalAssets : MonoBehaviour
    {
        [SerializeField] private Material _testMaterial;

        public Material TestMaterial => _testMaterial;
    }
}
