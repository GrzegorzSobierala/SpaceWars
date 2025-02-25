using Game.Utility;
using Game.Utility.Globals;
using Unity.Mathematics;
using UnityEngine;
using Zenject;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

namespace Game.Player.Ship
{
    [RequireComponent(typeof(LineRenderer))]
    public class Hook : MonoBehaviour
    {
        [Inject] SpringJoint2D _joint;
        [Inject] private CenterOfMass _centerOfMass;

        [SerializeField] private float _maxDistance = 150.0f;
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

            //connectedBody.GetAttachedColliders
            float distanceToCenter = Vector2.Distance(connectedPos,
                AnchorPointWorld);

            Vector2 dir = (connectedPos - AnchorPointWorld).normalized;

            RaycastHit2D[] result = new RaycastHit2D[1];
            int isHit = Physics2D.Raycast(AnchorPointWorld, dir, _rayFilter, result,
                distanceToCenter);

            if (isHit == 0)
            {
                Debug.LogError("No body hit", gameObject);
                return;
            }

            if (result[0].rigidbody != connectedBody)
            {
                Debug.LogError("Wrong body hit", result[0].collider);
                return;
            }

            float edgeCenterDist = Vector2.Distance(connectedPos, result[0].point);
            Vector2 targetDir = (AnchorPointWorld - connectedPos).normalized;

            Vector2 targetPoint = connectedPos + (targetDir * (edgeCenterDist + _distanceToEdge));
            _joint.distance = Vector2.Distance(ConnectedAnchorPointWorld, targetPoint);
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

                //Debug.Log($"Force: {_joint.reactionForce.ToString("f2")} | {_joint.reactionTorque.ToString("f2")}");
            }
            else
            {
                _joint.dampingRatio = _dampingRatioNoForce;
                _joint.frequency = _frequencyNoForce;
            }
            //Vector2 dirToConnect = ConnectedAnchorPointWorld - AnchorPointWorld;
            //Vector2 dirToConnectLocal = transform.InverseTransformDirection(dirToConnect).normalized;

            //_joint.anchor = dirToConnectLocal * _anchorOffset;
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
