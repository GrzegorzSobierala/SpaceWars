using Game.Utility.Globals;
using UnityEngine;

namespace Game
{
    public class RemoveEnemyFOVFromColliderCallbacks : MonoBehaviour
    {
        void Awake()
        {
            // Get the layer index for "EnemyFieldOfView"
            int enemyFOVLayer = LayerMask.NameToLayer(Layers.EnemyFieldOfView);
            // Convert the layer index into a bitmask
            int enemyFOVMask = 1 << enemyFOVLayer;

            // Find all Collider2D components in the scene.
            Collider2D[] colliders = FindObjectsOfType<Collider2D>();

            foreach (Collider2D col in colliders)
            {
                // Skip colliders that belong to the EnemyFieldOfView layer.
                if (col.gameObject.layer == enemyFOVLayer)
                    continue;

                // Remove the EnemyFieldOfView layer from the collider's CallbackLayers bitmask.
                col.callbackLayers &= ~enemyFOVMask;
            }
        }
    }
}
