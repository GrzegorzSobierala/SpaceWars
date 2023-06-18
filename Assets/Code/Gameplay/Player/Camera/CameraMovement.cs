using UnityEngine;

namespace Game.Player
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody playerBody;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float speed;
        [SerializeField] private float maxCameraDistance = 15;


        #region MonoBehaviour

        private void Update()
        {
            MoveCamera2();
        }

        #endregion

        void MoveCamera2()
        {
            Vector3 playerPositionInCameraY = playerTransform.position;
            playerPositionInCameraY.y = transform.position.y;

            Vector3 moveVector = (playerPositionInCameraY - transform.position) * speed / 1000;

            transform.position += moveVector;
        }
    }
}