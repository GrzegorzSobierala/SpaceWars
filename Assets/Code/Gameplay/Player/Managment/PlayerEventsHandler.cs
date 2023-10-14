using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.Combat;

namespace Game.Player
{
    public class PlayerEventsHandler : MonoBehaviour
    {
        public Action<Collision2D> OnCollisionEnter;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            OnCollisionEnter?.Invoke(collision);
        }
    }
}
