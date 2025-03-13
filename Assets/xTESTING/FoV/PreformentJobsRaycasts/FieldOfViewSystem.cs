using Game.Utility.Globals;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Game.Physics
{
    public class FieldOfViewSystem : MonoBehaviour
    {
        // int1 - ColliderId , int2 - enter count
        private Dictionary<int, (Collider2D, int)> _collidersToUnprepare; 
        // int - ColliderId
        private NativeHashMap<int, ColliderDataUnprepared> _datasUnprep;
        // int - EntityId
        private NativeHashMap<int, NativeHashSet<int>> _entityesColliders;

        private LayerMask _allLayerMask;
        private ContactFilter2D _contactFilter;

        public LayerMask AllLayerMask => _allLayerMask;
        public ContactFilter2D ContactFilter => _contactFilter;

        private void Awake()
        {
            _datasUnprep = new(10, Allocator.Persistent);
            _entityesColliders = new(0, Allocator.Persistent);


            _allLayerMask = LayerMask.GetMask(Layers.Player, Layers.Obstacle, Layers.Enemy);
            _contactFilter = new ContactFilter2D
            {
                useTriggers = false,
                layerMask = _allLayerMask,
                useLayerMask = true
            };
        }

        private void OnDestroy()
        {
            _datasUnprep.Dispose();
            foreach (var entity in _entityesColliders)
            {
                entity.Value.Dispose();
            }
            _entityesColliders.Dispose();
        }

        public void AddCollider(FieldOfViewEntity entity, Collider2D collider)
        {
            int entityId = entity.GetInstanceID();
            if (!_entityesColliders.ContainsKey(entityId))
            {
                _entityesColliders.Add(entityId, new NativeHashSet<int>(1, Allocator.Persistent));
            }

            int colliderId = collider.GetInstanceID();
            _entityesColliders[entityId].Add(colliderId);

            if(_collidersToUnprepare.ContainsKey(colliderId))
            {
                _collidersToUnprepare[colliderId] = (collider,
                    _collidersToUnprepare[colliderId].Item2 + 1);
            }
            else
            {
                _collidersToUnprepare.Add(colliderId, (collider, 1));
            }
        }

        public void RemoveCollider(FieldOfViewEntity entity, Collider2D collider)
        {
            int entityId = entity.GetInstanceID();
            
            if(!_entityesColliders.ContainsKey(entityId))
            {
                Debug.LogError("There is no entity to remove", entity);
                return;
            }

            int colliderId = collider.GetInstanceID();
            if (_entityesColliders[entityId].Contains(colliderId))
            {
                _entityesColliders[entityId].Remove(colliderId);
            }
            else
            {
                Debug.LogError("There is no collider to remove from _entityesColliders - entity ref", entity);
                Debug.LogError("There is no collider to remove from _entityesColliders - collider ref", collider);
            }

            if(_collidersToUnprepare.ContainsKey(colliderId))
            {
                if (_collidersToUnprepare[colliderId].Item2 <= 1)
                {
                    if(_collidersToUnprepare[colliderId].Item2 <= 0)
                    {
                        Debug.LogError("_collidersToUnprepare[collidersId].Item2 <= 0 - entity ref", entity);
                        Debug.LogError("_collidersToUnprepare[collidersId].Item2 <= 0 - collider ref", collider);
                    }

                    _collidersToUnprepare.Remove(colliderId);
                }
                else
                {
                    _collidersToUnprepare[colliderId] = (collider,
                        _collidersToUnprepare[colliderId].Item2 - 1);
                }
            }
            else
            {
                Debug.LogError("There is no collider to remove from _collidersToUnprepare - entity ref", entity);
                Debug.LogError("There is no collider to remove from _collidersToUnprepare - collider ref", collider);
            }
        }
    }
}
