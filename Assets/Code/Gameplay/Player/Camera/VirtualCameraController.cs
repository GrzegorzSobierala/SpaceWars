using Cinemachine;
using UnityEngine;
using Zenject;

namespace Game.Player.VirtualCamera
{
    public class VirtualCameraController : MonoBehaviour
    {
        [Inject] private Rigidbody2D _body;
        [Inject] private CinemachineVirtualCamera _vCamera;

        private CinemachineTransposer _transposer;

        //private void Start()
        //{
        //    Init();
        //}

        //private void Update()
        //{
        //    Vector3 velocityOffset = new Vector3(_body.velocity.x, _body.velocity.y, 0);
        //    velocityOffset += new Vector3(0, 0, _transposer.m_FollowOffset.z);
        //    _transposer.m_FollowOffset = velocityOffset;

            
        //}

        //private void Init()
        //{
        //    _transposer = _vCamera.GetCinemachineComponent<CinemachineTransposer>();

        //    if (_transposer == null)
        //    {
        //        Debug.LogError("No transposer found on virtual camera");
        //    }
        //}


    }
}
