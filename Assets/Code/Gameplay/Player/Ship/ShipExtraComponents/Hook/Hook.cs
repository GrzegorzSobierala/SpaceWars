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

        [SerializeField] private float _maxDistance = 200.0f;
        [SerializeField] private float _dampingRatio = 0.6f;
        [SerializeField] private float _frequency = 0.8f;
        [SerializeField] private float _dampingRatioNoForce = 0;
        [SerializeField] private float _frequencyNoForce = 0.01f;
        [SerializeField] private float _colorMulti = 0.7f;
        [SerializeField] private float _anchorOffset = 5f;
        [SerializeField] private float _anchorOffsetSidesY = -2.5f;
        [SerializeField] private float _connectDistanceOffset = -5f;

        private LineRenderer _lineRenderer;
        private Transform _connectedAnchorTransform;
        private Material _lineMaterial;
        private ContactFilter2D _rayFilter;
        private float _distanceToEdge;

        public float MaxDistance => _maxDistance;
        public bool IsConnected => _joint.enabled;

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
            if (!IsConnected)
                return;

            UpdateRenderer();
        }

        private void FixedUpdate()
        {
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
            _joint.distance = math.clamp(_distanceToEdge, 1, _maxDistance);

            _joint.enabled = true;

            TryUpdateHook();
        }

        private void UpdateJointDistance(Rigidbody2D connectedBody)
        {
            Vector2 connectedPos = Utils.CalculateWorldCenterOfMass(connectedBody);
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
            _joint.distance = distance;
        }

        public void Disconnect()
        {
            _joint.connectedBody = null;
            _joint.enabled = false;
            _lineRenderer.enabled = false;
        }

        private void TryUpdateHook()
        {
            UpdateRenderer();
        }

        private void UpdateJoint()
        {
            UpdateJointDistance(_joint.connectedBody);

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
    }
}
