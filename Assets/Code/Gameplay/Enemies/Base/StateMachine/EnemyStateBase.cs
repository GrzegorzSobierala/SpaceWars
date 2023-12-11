using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Room.Enemy
{
    public abstract class EnemyStateBase : MonoBehaviour
    {
        public void EnterState()
        {
            enabled = true;
            OnEnterState();
        }

        public void ExitState()
        {
            enabled = false;
            OnExitState();
        }

        protected abstract void OnEnterState();

        protected abstract void OnExitState();
    }
}
