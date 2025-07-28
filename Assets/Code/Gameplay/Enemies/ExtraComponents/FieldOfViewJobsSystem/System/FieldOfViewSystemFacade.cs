using Game.Room.Enemy;
using System;
using Unity.Collections;
using UnityEngine;

namespace Game.Physics
{
    public class FieldOfViewSystemFacade
    {
        private Action<Action> _doWhenJobCompletedAction;
        private Func<int> _getEnemyLayer;
        private FieldOfViewSystemCollectionsCache _collections;

        public FieldOfViewSystemFacade(FieldOfViewSystemCollectionsCache collections ,
            Action<Action> doWhenJobCompletedAction, Func<int> getEnemyLayer)
        {
            _collections = collections;
            _doWhenJobCompletedAction = doWhenJobCompletedAction;
            _getEnemyLayer = getEnemyLayer;
        }
        
        public void AddCollider(FieldOfViewEntity entity, Collider2D collider)
        {
            if (collider.isTrigger)
                return;

            int entityId = entity.GetInstanceID();
            int colliderId = collider.GetInstanceID();

            if (!_collections.entities.ContainsKey(entityId))
            {
                Debug.LogError("Add collider failed, entity isn't added", entity);
                return;
            }

            _collections.IncreaseCapasityOfEntitiesCollidersIfNeeded(10);
            _collections.entitiesColliders.Add(entityId, colliderId);

            if (_collections.collidersUnprepared.ContainsKey(colliderId))
            {
                var current = _collections.collidersUnprepared[colliderId];
                _collections.collidersUnprepared[colliderId] = (collider, current.Item2 + 1);
            }
            else
            {
                _collections.collidersUnprepared.Add(colliderId, (collider, 1));
            }

            if (collider.gameObject.layer == _getEnemyLayer.Invoke() && 
                !_collections.collidersDetectable.ContainsKey(colliderId))
            {
                if (collider.TryGetComponent(out IGuardStateDetectable detectable))
                {
                    _collections.collidersDetectable.Add(colliderId, detectable);
                }
                else
                {
                    Debug.LogError("Collider doesn't have IGuardStateDetectable", collider);
                }
            }
        }

        public void RemoveCollider(FieldOfViewEntity entity, Collider2D collider)
        {
            if (collider.isTrigger)
                return;

            if (!entity.isActiveAndEnabled)
            {
                return;
            }

            int entityId = entity.GetInstanceID();
            int colliderId = collider.GetInstanceID();

            if (!_collections.entities.ContainsKey(entityId))
            {
                Debug.LogError("Remove collider failed, entity isn't added", entity);
                return;
            }

            if (_collections.entitiesColliders.TryGetFirstValue(entityId, out int currentColliderId,
                out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    if (currentColliderId == colliderId)
                    {
                        _collections.entitiesColliders.Remove(iterator);
                        break;
                    }
                } while (_collections.entitiesColliders.TryGetNextValue(out currentColliderId,
                    ref iterator));
            }
            else
            {
                Debug.LogError("There is no entity to remove collider", entity);
                return;
            }

            if (_collections.collidersUnprepared.ContainsKey(colliderId))
            {
                if (_collections.collidersUnprepared[colliderId].Item2 <= 1)
                {
                    if (_collections.collidersUnprepared[colliderId].Item2 <= 0)
                    {
                        Debug.LogError("_collidersToUnprepare[colliderId].Item2 <= 0 - entity ref", entity);
                        Debug.LogError("_collidersToUnprepare[colliderId].Item2 <= 0 - collider ref", collider);
                    }

                    _collections.collidersUnprepared.Remove(colliderId);
                }
                else
                {
                    var current = _collections.collidersUnprepared[colliderId];
                    _collections.collidersUnprepared[colliderId] = (collider, current.Item2 - 1);
                }
            }
            else
            {
                Debug.LogError("There is no collider to remove from _collidersToUnprepare - entity ref", entity);
                Debug.LogError("There is no collider to remove from _collidersToUnprepare - collider ref", collider);
            }
        }

        public void AddEntity(FieldOfViewEntity entity)
        {
            int entityId = entity.GetInstanceID();
            if (!_collections.entities.TryAdd(entityId, entity))
            {
                //Debug.LogError("Entity already added", entity);
                return;
            }

            if (_collections.entitiesColliders.ContainsKey(entityId))
            {
                Debug.LogError("Found in _entitiesColliders", entity);
                return;
            }

            _collections.wasEntitiesDicChanged = true;

            _collections.IncreaseCapasityOfEntitiesCollidersIfNeeded(10);
            _collections.entitiesColliders.Add(entityId, FieldOfViewSystem.EMPTY_COLLIDER_ID);
        }

        public void RemoveEntity(FieldOfViewEntity entity)
        {
            int entityId = entity.GetInstanceID();

            if (!_collections.entities.ContainsKey(entityId))
            {
                //Debug.LogError("Entity not found in _entityes", entity);
                return;
            }

            if (!_collections.entitiesColliders.ContainsKey(entityId))
            {
                Debug.LogError("Entity not found in _entitiesColliders", entity);
            }

            if (_collections.entitiesColliders.TryGetFirstValue(entityId, out int colliderId,
                out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    if (_collections.collidersUnprepared.ContainsKey(colliderId))
                    {
                        var current = _collections.collidersUnprepared[colliderId];
                        if (current.Item2 <= 1)
                        {
                            _collections.collidersUnprepared.Remove(colliderId);
                        }
                        else
                        {
                            _collections.collidersUnprepared[colliderId] = (current.Item1, current.Item2 - 1);
                        }
                    }
                } while (_collections.entitiesColliders.TryGetNextValue(out colliderId, ref iterator));
            }

            _collections.entities.Remove(entityId);
            _collections.entitiesColliders.Remove(entityId);

            _collections.wasEntitiesDicChanged = true;
        }

        public void AddController(FieldOfViewEntitiesController controller, FieldOfViewEntity entity)
        {
            if (!_collections.entitiesController.ContainsValue(controller))
            {
                _collections.controllers.Add(controller);
            }

            _collections.entitiesController.Add(entity.GetInstanceID(), controller);
        }

        public void RemoveController(FieldOfViewEntitiesController controller, FieldOfViewEntity entity)
        {
            _collections.entitiesController.Remove(entity.GetInstanceID());

            if (!_collections.entitiesController.ContainsValue(controller))
            {
                _collections.controllers.Remove(controller);
            }
        }

        public void OnEntityDataChange()
        {
            _collections.wasEntitiesDicChanged = true;
        }

        public void DoWhenJobCompleted(Action action)
        {
            _doWhenJobCompletedAction?.Invoke(action);
        }
    }
}
