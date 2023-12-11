using Game.Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Room.Enemy
{
    public abstract class EnemyGunBase : MonoBehaviour
    {
        protected Action OnAimTarget;

        public abstract void StartShooting();

        public abstract void StopShooting();

        public abstract void AimAt(Transform target);

        public abstract void AimAt(Vector2 worldPosition);

        public abstract void AimAt(float localRotation);

        public void SubscribeOnAimTarget(Action onAimTarget)
        {
            OnAimTarget += onAimTarget;
        }

        public void Unsubscribe(Action onAimTarget)
        {
            OnAimTarget -= onAimTarget;
        }
    }
}
