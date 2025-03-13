using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Physics
{
    public class FieldOfViewEntity : MonoBehaviour
    {
        private FieldOfViewSystem _system;
        //private Collider2D[] _overlapColliders;

        //private List<Collider2D> _overlapCashe = new();

        private void Awake()
        {
            _system = GetComponentInParent<FieldOfViewSystem>();
            //_overlapColliders = GetComponents<Collider2D>();
        }

        private void Start()
        {
            //foreach (var collider in _overlapColliders)
            //{
            //    collider.OverlapCollider(_system.ContactFilter, _overlapCashe);
            //    foreach (var overlaped in _overlapCashe)
            //    {
            //        _system.AddCollider(this, overlaped);
            //    }
            //}
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            _system.AddCollider(this, collision);
            Debug.Log("enter");
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            _system.RemoveCollider(this, collision);
            Debug.Log("exit");
        }
    }
}
