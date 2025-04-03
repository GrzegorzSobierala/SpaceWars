using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Objectives
{
    public interface IDefeatedCallback
    {
        public event Action OnDefeated;

        public Transform MainTransform { get; }
    }
}
