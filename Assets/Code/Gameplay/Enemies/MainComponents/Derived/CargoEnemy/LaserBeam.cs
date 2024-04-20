using Game.Utility.Globals;
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
        [SerializeField] private float _reloadTime = 5f;
        [SerializeField] private float _range = 300f;
        [SerializeField] private float _chargingWidth = 0.3f;
        [SerializeField] private float _fireWidth = 2f;
        [Space]
        [SerializeField] private LayerMask _blockAimLayerMask;

        private LineRenderer _lineRenderer;
        private bool _isFiring = false;
        private ContactFilter2D _contactFilter;
        private float _startChargingTime = -100;
        private float _startShootingTime = -100;
        private float _startReloadingTime = -100;

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            TryFire();
        }

        public void StartFire()
        {
            if (_isFiring || _startReloadingTime + _reloadTime > Time.time)
                return;

            _isFiring = true;
            _lineRenderer.enabled = true;

            _startChargingTime = Time.time;
            _startShootingTime = Time.time + _chargingTime;
        }

        public void StopFire()
        {
            if (!_isFiring)
                return;

            _isFiring = false;
            _lineRenderer.enabled = false;
            _startReloadingTime = Time.time;
        }

        private void TryFire()
        {
            if (_startReloadingTime + _reloadTime > Time.time)
            {
                StopFire();
                Debug.Log("reload");
            }
            else if(_startChargingTime + _chargingTime > Time.time)
            {
                _lineRenderer.startWidth = _chargingWidth;
                _lineRenderer.endWidth = _chargingWidth;
                Debug.Log("charge");
            }
            else if(_startShootingTime + _shootTime > Time.time)
            {
                _lineRenderer.startWidth = _fireWidth;
                _lineRenderer.endWidth = _fireWidth;
                Debug.Log("fire");
            }
            else
            {
                StopFire();
                Debug.Log("nothing");
            }

            Vector3 pos = transform.position;
            Vector2 dir = transform.forward;

            RaycastHit2D[] raycastHits = new RaycastHit2D[1];
            Physics2D.Raycast(pos, dir, _contactFilter, raycastHits, _range);

            Vector2 targetPos;
            if (raycastHits[0].collider != null)
            {
                targetPos = raycastHits[0].point;
            }
            else
            {
                targetPos = transform.position + transform.forward * _range;
            }

            Vector3[] positions = new Vector3[2];
            positions[0] = transform.position;
            positions[1] = targetPos;

            _lineRenderer.SetPositions(positions);
        }


        private void Initialize()
        {
            _lineRenderer = GetComponent<LineRenderer>();

            _contactFilter = new ContactFilter2D
            {
                useTriggers = false,
                layerMask = _blockAimLayerMask,
                useLayerMask = true,
            };
        }
    }
}
