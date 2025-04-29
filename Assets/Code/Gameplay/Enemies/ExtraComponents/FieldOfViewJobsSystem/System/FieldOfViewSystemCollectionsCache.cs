using Game.Room.Enemy;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Physics
{
    public class FieldOfViewSystemCollectionsCache
    {
        /// <summary>
        /// Key: ColliderId, Value: Tuple containing Collider2D and enter count.
        /// </summary>
        public Dictionary<int, (Collider2D, int)> collidersUnprepared = new();
        /// <summary>
        /// Key: EntityId, Value: FieldOfViewEntity.
        /// </summary>
        public Dictionary<int, FieldOfViewEntity> entities = new();
        /// <summary>
        /// Key: Enemy's ColliderId, Value: IGuardStateDetectable.
        /// </summary>
        public Dictionary<int, IGuardStateDetectable> collidersDetectable = new();
        /// <summary>
        /// Key: EntityId, Value: FieldOfViewEntitiesController.
        /// </summary>
        public Dictionary<int, FieldOfViewEntitiesController> entitiesController = new();
        /// <summary>
        /// Gets the set of controllers.
        /// </summary>
        public HashSet<FieldOfViewEntitiesController> controllers = new();
        /// <summary>
        /// Key: EntityId, Value: ColliderId.
        /// </summary>
        public NativeMultiHashMap<int, int> entitiesColliders;
        /// <summary>
        /// A cache of composite path points used for field of view calculations.
        /// </summary>
        public Vector2[] pathPointsCompositeCache = new Vector2[10];
        /// <summary>
        /// A list of unprepared collider data before processing for raycasting.
        /// </summary>
        public NativeList<ColliderDataUnprepared> datasUnprep;
        /// <summary>
        /// A list of unprepared vertices for collider data.
        /// </summary>
        public NativeList<float2> vertsUnprep;
        /// <summary>
        /// A hash map containing prepared collider data ready for raycasting.
        /// Key: ColliderId, Value: ColliderDataReady.
        /// </summary>
        public NativeHashMap<int, ColliderDataReady> datasRdy;
        /// <summary>
        /// A list of prepared vertices for collider data.
        /// </summary>
        public NativeList<float2> vertsRdy;
        /// <summary>
        /// A hash map containing field of view entity data.
        /// Key: EntityId, Value: FovEntityData.
        /// </summary>
        public NativeHashMap<int, FovEntityData> fovDatas;
        /// <summary>
        /// An array of vertices representing world positions of the field of view mesh.
        /// </summary>
        public NativeArray<float3> verticies;
        /// <summary>
        /// An array of triangle indices for the field of view mesh.
        /// </summary>
        public NativeArray<int> triangles;
        /// <summary>
        /// A hash map tracking whether enemies have been hit by the player.
        /// Key: EnemyId, Value: Boolean indicating hit status.
        /// </summary>
        public NativeHashMap<int, bool> enemiesPlayerHit;
        /// <summary>
        /// A hash map tracking whether enemies have been hit by other enemies.
        /// Key: EnemyHitData, Value: Boolean indicating hit status.
        /// </summary>
        public NativeHashMap<EnemyHitData, bool> enemiesEnemyHit;
        /// <summary>  
        /// Gets or sets a value indicating whether the entities dictionary has changed.  
        /// </summary>  
        public bool wasEntitiesDicChanged;

        private bool _areCollectionsInitialized = false;

        public void IncreaseCapasityOfEntitiesCollidersIfNeeded(int capIncrease)
        {
            if (entitiesColliders.Capacity >= entities.Count + 1)
            {
                return;
            }

            int newCapacity = entitiesColliders.Capacity + capIncrease;
            NativeMultiHashMap<int, int> newMap = new(newCapacity, Allocator.Persistent);

            var nonUniqueKeys = entitiesColliders.GetKeyArray(Allocator.Temp);
            NativeHashSet<int> uniqueKeys = new(nonUniqueKeys.Length, Allocator.Temp);
            foreach (var key in nonUniqueKeys)
            {
                if (uniqueKeys.Contains(key))
                    continue;

                uniqueKeys.Add(key);
            }
            nonUniqueKeys.Dispose();

            foreach (var key in uniqueKeys)
            {
                if (entitiesColliders.TryGetFirstValue(key, out int value, 
                    out NativeMultiHashMapIterator<int> iterator))
                {
                    do
                    {
                        newMap.Add(key, value);
                    }
                    while (entitiesColliders.TryGetNextValue(out value, ref iterator));
                }
            }

            uniqueKeys.Dispose();

            // Dispose the old map and assign the new one.
            entitiesColliders.Dispose();
            entitiesColliders = newMap;
        }

        public void InitCollections()
        {
            if(_areCollectionsInitialized)
            {
                Debug.LogError("Collections already initialized");
                return;
            }

            _areCollectionsInitialized = true;

            entitiesColliders = new NativeMultiHashMap<int, int>(10, Allocator.Persistent);
            datasUnprep = new(10, Allocator.Persistent);
            vertsUnprep = new(50, Allocator.Persistent);
            datasRdy = new(10, Allocator.Persistent);
            vertsRdy = new(50, Allocator.Persistent);
            fovDatas = new(5, Allocator.Persistent);
            verticies = new(0, Allocator.Persistent);
            triangles = new(0, Allocator.Persistent);
            enemiesPlayerHit = new(5, Allocator.Persistent);
            enemiesEnemyHit = new(25, Allocator.Persistent);
        }

        public void DisposeCollections()
        {
            if(!_areCollectionsInitialized)
            {
                Debug.LogError("Collections not initialized");
                return;
            }

            entitiesColliders.Dispose();
            datasUnprep.Dispose();
            vertsUnprep.Dispose();
            datasRdy.Dispose();
            vertsRdy.Dispose();
            fovDatas.Dispose();
            verticies.Dispose();
            triangles.Dispose();
            enemiesPlayerHit.Dispose();
            enemiesEnemyHit.Dispose();
        }
    }
}
