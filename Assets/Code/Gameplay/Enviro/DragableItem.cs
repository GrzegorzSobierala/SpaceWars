using Game.Management;
using Game.Utility;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using Zenject;

namespace Game.Room.Enviro
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpringJoint2D), typeof(LineRenderer))]
    public class DragableItem : MonoBehaviour
    {
        [Inject] private PlayerManager _playerManager;

        [SerializeField] private Transform _DEBUG_anchorPoint;
        [SerializeField] private Transform _DEBUG_connectedAnchorPoint;
        [SerializeField] private float _dampingRatio = 0;
        [SerializeField] private float _frequency = 0.8f;
        [SerializeField] private float _dampingRatioNoForce = 0;
        [SerializeField] private float _frequencyNoForce = 0.1f;
        [SerializeField] private float _colorMulti = 0.7f;

        private Rigidbody2D _body;
        private SpringJoint2D _joint;
        private LineRenderer _lineRenderer;
        private Vector2 _anchorPoint;
        private Transform _connectedAnchorTransform;
        private Vector2 _connectedAnchorPoint;
        private Material _lineMaterial;

        private Vector2 AnchorPointWorld => transform.TransformPoint(_anchorPoint);
        private Vector2 ConnectedAnchorPointWorld => _connectedAnchorTransform.
            TransformPoint(_connectedAnchorPoint);

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _joint = GetComponent<SpringJoint2D>();
            _lineRenderer = GetComponent<LineRenderer>();

            _lineMaterial = _lineRenderer.material;
        }

        [Button]
        private void Start()
        {
            Connect(_playerManager.PlayerBody, _DEBUG_anchorPoint.position, 
                _DEBUG_connectedAnchorPoint.position);
        }

        private void Update()
        {
            UpdateJoint();
            UpdateRenderer();
        }

        private void Connect(Rigidbody2D body, Vector2 anchorPoint, Vector2 anchorConnectedPoint)
        {
            _anchorPoint = transform.InverseTransformPoint(anchorPoint);
            _connectedAnchorPoint = _playerManager.PlayerBody.transform.
                InverseTransformPoint(anchorConnectedPoint);
            _connectedAnchorTransform = body.transform;

            _joint.connectedBody = body;
            _joint.anchor = _anchorPoint;
            _joint.connectedAnchor = _connectedAnchorPoint;

            UpdateJoint();
            UpdateRenderer();
        }

        private void UpdateJoint()
        {
            if (!_joint.connectedBody)
            {
                return;
            }

            if(Vector2.Distance(AnchorPointWorld, ConnectedAnchorPointWorld) > _joint.distance)
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
            if (!_joint.connectedBody)
            {
                _lineRenderer.enabled = false;
                return;
            }

            _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(0, AnchorPointWorld);
            _lineRenderer.SetPosition(1, ConnectedAnchorPointWorld);

            float anchorsDist = Vector2.Distance(AnchorPointWorld, ConnectedAnchorPointWorld);

            float redColor = Utils.Remap(anchorsDist, _joint.distance * _colorMulti, 
                _joint.distance * (2 - _colorMulti), 0, 1);
            redColor = math.clamp(redColor, 0, 1);

            _lineMaterial.color = Color.Lerp(Color.white, Color.red, redColor);
        }
    }
}
