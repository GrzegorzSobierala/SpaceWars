using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Room.Enemy
{   
    public interface IDocking 
    {
        public abstract event Func<bool> CanUndock;

        public abstract event Action OnObjectDestroy;

        public abstract Rigidbody2D Body { get; }

        public abstract float DistanceBeforeDock { get; }

        public abstract void OnStartDocking();

        public abstract void OnEndDocking();

        public abstract void OnStartUnDocking();

        public abstract void OnEndUnDocking();

        public abstract void OnDockDestroy();
    }
}
