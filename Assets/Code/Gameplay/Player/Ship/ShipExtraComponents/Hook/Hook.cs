using Game.Utility;
using Game.Utility.Globals;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    [RequireComponent(typeof(LineRenderer))]
    public class Hook : MonoBehaviour
    {
        [Inject] SpringJoint2D _joint;
        [Inject] private CenterOfMass _centerOfMass;
        [Inject] private Rigidbody2D _body;

        [SerializeField] private float _maxDistance = 200.0f;
        [SerializeField] private float _dampingRatio = 0.6f;
        [SerializeField] private float _frequency = 0.8f;
        [SerializeField] private float _dampingRatioNoForce = 0;
        [SerializeField] private float _frequencyNoForce = 0.01f;
        [SerializeField] private float _colorMulti = 0.7f;
        [SerializeField] private float _anchorOffset = 5f;
        [SerializeField] private float _anchorOffsetSidesY = -2.5f;
        [SerializeField] private float _connectDistanceOffset = -5f;
        [SerializeField] private float _maxSpeedBoostMulti = 1.5f;
        [SerializeField] private float _minDistanceToBust = -5;
        [SerializeField] private float _maxDistanceToBust = 10;

        private LineRenderer _lineRenderer;
        private Transform _connectedAnchorTransform;
        private Material _lineMaterial;
        private ContactFilter2D _rayFilter;
        private float _distanceToEdge;
        private float _speedBoostValue = 1;

        public float MaxDistance => _maxDistance;
        public bool IsConnected => _joint.enabled;

        public float CurrentSpeedBoostMulti => _joint.isActiveAndEnabled ? _speedBoostValue : 1;

        private Vector2 AnchorPointWorld => transform.TransformPoint(_joint.anchor);
        private Vector2 ConnectedAnchorPointWorld => _connectedAnchorTransform.
            TransformPoint(_joint.connectedAnchor);

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();

            _rayFilter = new ContactFilter2D
            {
                useTriggers = false,
                useLayerMask = true,
                layerMask = LayerMask.GetMask(Layers.Enemy, Layers.Obstacle, Layers.ObstacleShootAbove,
                Layers.SmallObstacle)
            };
        }

        private void Start()
        {
            _lineMaterial = _lineRenderer.material;
            _lineRenderer.enabled = false;
        }

        private void Update()
        {
            CheckConnection();

            if (!IsConnected)
                return;

            UpdateRenderer();
        }

        private void FixedUpdate()
        {
            CheckConnection();

            if (!IsConnected)
                return;

            UpdateJoint();
        }

        private void OnJointBreak2D(Joint2D _)
        {
            Disconnect();
        }

        public void Connect(Rigidbody2D connectedBody, Vector2 hitPoint)
        {
            _connectedAnchorTransform = connectedBody.transform;
           
            _joint.connectedBody = connectedBody;
            _joint.connectedAnchor = connectedBody.transform.InverseTransformPoint(hitPoint);

            Vector2 dirToConnect = ConnectedAnchorPointWorld - _centerOfMass.Position;
            Vector2 dirToConnectLocal = transform.InverseTransformDirection(dirToConnect).normalized;
            float angle = Utils.AngleDirected(dirToConnectLocal);
            if (angle >= -180 && angle < -135)
            {
                _joint.anchor = new Vector2(0, -_anchorOffset);
            }
            else if (angle >= -135 && angle < -45)
            {
                _joint.anchor = new Vector2(_anchorOffset, _anchorOffsetSidesY);
            }
            else if (angle >= -45 && angle < 45)
            {
                _joint.anchor = new Vector2(0, _anchorOffset);
            }
            else if (angle >= 45 && angle < 135)
            {
                _joint.anchor = new Vector2(-_anchorOffset, _anchorOffsetSidesY);
            }
            else if (angle >= 135 && angle <= 180)
            {
                _joint.anchor = new Vector2(0, -_anchorOffset);
            }
            else
            {
                Debug.LogError("Angle is out of range");
            }

            _distanceToEdge = Vector2.Distance(AnchorPointWorld, ConnectedAnchorPointWorld);
            _joint.distance = math.clamp(_distanceToEdge - _connectDistanceOffset, 1, _maxDistance);

            _joint.enabled = true;

            foreach (var iHooked in connectedBody.GetComponents<IHookedCallBack>())
            {
                iHooked.OnHooked();
            }
            foreach (var iHooked in connectedBody.GetComponentsInChildren<IHookedCallBack>())
            {
                iHooked.OnHooked();
            }


            TryUpdateHook();
        }

        private void UpdateJointDistance(Rigidbody2D connectedBody)
        {
            Vector2 connectedPos;

            if(connectedBody.bodyType == RigidbodyType2D.Static)
            {
                connectedPos = Utils.CalculateWorldCenterOfMass(connectedBody);
            }
            else
            {
                connectedPos = connectedBody.worldCenterOfMass;
            }

            float distanceToCenter = Vector2.Distance(connectedPos, AnchorPointWorld);

            Vector2 dir = (connectedPos - AnchorPointWorld).normalized;

            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            int isHit = Physics2D.Raycast(AnchorPointWorld, dir, _rayFilter, hits, distanceToCenter);

            float minDist = float.MaxValue;
            Vector2 minPoint = Vector2.zero;
            bool minDistFound = false;
            foreach (var hit in hits)
            {
                if(hit.rigidbody != connectedBody)
                {
                    continue;
                }

                if(hit.distance < minDist)
                {
                    minDistFound = true;
                    minDist = hit.distance < minDist ? hit.distance : minDist;
                    minPoint = hit.point;
                }
            }

            if(!minDistFound)
            {
                Debug.LogError("No body hit", gameObject);
                return;
            }

            float edgeCenterDist = Vector2.Distance(connectedPos, minPoint);
            Vector2 targetDir = (AnchorPointWorld - connectedPos).normalized;

            Vector2 targetPoint = connectedPos + (targetDir * (edgeCenterDist + _distanceToEdge));
            float distance = Vector2.Distance(ConnectedAnchorPointWorld, targetPoint);
            distance = math.clamp(distance + _connectDistanceOffset, 1, _maxDistance);

            UpdateBoost(targetDir);

            _joint.distance = distance;
        }

        private void UpdateBoost(Vector2 targetDir)
        {
            float dot = Vector2.Dot(targetDir, _body.transform.up);

            float minRemaped = 1;
            float maxRemaped = _maxSpeedBoostMulti;

            _speedBoostValue = Utils.Remap(math.abs(dot), 0.7071f, 0, minRemaped, _maxSpeedBoostMulti);
            _speedBoostValue = math.clamp(_speedBoostValue, minRemaped, maxRemaped);

            float distance = Vector2.Distance(AnchorPointWorld, ConnectedAnchorPointWorld);

            float minDistance = _joint.distance + _minDistanceToBust;
            float maxDistance = _joint.distance + _maxDistanceToBust;

            _speedBoostValue = Utils.Remap(distance, minDistance, maxDistance, minRemaped, 
                _speedBoostValue);
            _speedBoostValue = math.clamp(_speedBoostValue, minRemaped, maxRemaped);
        }

        public void Disconnect()
        {
            _joint.connectedBody = null;
            _joint.enabled = false;
            _lineRenderer.enabled = false;
            _speedBoostValue = 1;
        }

        private void TryUpdateHook()
        {
            UpdateRenderer();
        }

        private void UpdateJoint()
        {
            if (Vector2.Distance(AnchorPointWorld, ConnectedAnchorPointWorld) > _joint.distance)
            {
                _joint.dampingRatio = _dampingRatio;
                _joint.frequency = _frequency;
            }
            else
            {
                _joint.dampingRatio = _dampingRatioNoForce;
                _joint.frequency = _frequencyNoForce;
            }

            UpdateJointDistance(_joint.connectedBody);
        }

        private void UpdateRenderer()
        {
            _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(0, AnchorPointWorld);
            _lineRenderer.SetPosition(1, ConnectedAnchorPointWorld);

            float anchorsDist = Vector2.Distance(AnchorPointWorld, ConnectedAnchorPointWorld);

            float redColor = Utils.Remap(anchorsDist, _joint.distance * _colorMulti,
                _joint.distance * (2 - _colorMulti), 0, 2);
            redColor = math.clamp(redColor, 0, 1);

            _lineMaterial.color = Color.Lerp(Color.white, Color.red, redColor);
        }

        private void CheckConnection()
        {
            if (IsConnected && _joint.connectedBody == null)
            {
                Disconnect();
            }
        }
    }
}
