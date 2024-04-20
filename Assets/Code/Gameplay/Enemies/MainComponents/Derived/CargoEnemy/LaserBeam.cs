using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Room.Enemy
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserBeam : MonoBehaviour
    {
        [SerializeField] private float _chargingTime = 0.3f;
        [SerializeField] private float _shootTime = 5f;
        [Space]
        [SerializeField] private Transform _shootPoint;

        private LineRenderer _lineRenderer;

        private void Awake()
        {
            Initialize();
        }

        public void Fire()
        {

        }

        public void StopFire()
        {

        }

        private void Initialize()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }
    }
}
