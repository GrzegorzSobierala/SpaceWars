using UnityEngine;
using Zenject;
using Game.Room;
using Game.Management;

namespace Game.Player.Ui
{
    public class MissionPoinerUi : MonoBehaviour
    {
        [Inject] private PlayerSceneManager _sceneManager;
        [Inject] private PlayerManager _playerManager;

        public GameObject pointer;
        public Canvas canvas;
        public float visibilityThreshold = 0.1f;

        private Transform currentTarget;

        private void Update()
        {
            // Show or hide pointer based on nearest enemy and its visibility
            if (currentTarget != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(currentTarget.position);

                if (IsEnemyOnScreen(screenPos, visibilityThreshold))
                {
                    pointer.SetActive(false); // Nearest enemy is on screen, hide the pointer
                }
                else
                {
                    // Nearest enemy is off-screen, show the pointer and position it at the edge of the screen pointing towards the nearest enemy
                    pointer.SetActive(true);
                    PositionPointer(screenPos);
                }
            }
            else
            {
                pointer.SetActive(false); // No enemies to show, hide the pointer
            }
        }

        public void SetCurrentTarget(Transform target)
        {
            currentTarget = target;
        }

        bool IsEnemyOnScreen(Vector3 screenPos, float treshoald)
        {
            float widthTreshoald = Screen.width * treshoald;
            float heightTreshoald = Screen.height * treshoald;

            return screenPos.x > widthTreshoald && screenPos.x < Screen.width - widthTreshoald && 
                screenPos.y > heightTreshoald && screenPos.y < Screen.height - widthTreshoald;
        }

        void PositionPointer(Vector3 screenPos)
        {
            // Convert screen position to local position in the canvas's coordinate system
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPos, canvas.worldCamera, out Vector2 localPointerPos);

            // Get canvas size
            Vector2 canvasSize = (canvas.transform as RectTransform).sizeDelta;

            // Calculate the half size of the pointer to adjust for its pivot point
            Vector2 pointerHalfSize = (pointer.transform as RectTransform).sizeDelta * 0.5f;

            // Clamp the local position within the canvas boundaries
            localPointerPos.x = Mathf.Clamp(localPointerPos.x, -canvasSize.x * 0.5f + pointerHalfSize.x, canvasSize.x * 0.5f - pointerHalfSize.x);
            localPointerPos.y = Mathf.Clamp(localPointerPos.y, -canvasSize.y * 0.5f + pointerHalfSize.y, canvasSize.y * 0.5f - pointerHalfSize.y);

            // Set the pointer's local position
            pointer.transform.localPosition = localPointerPos;

            // Calculate direction to the nearest enemy
            Vector3 dirToEnemy = (currentTarget.position - 
                _playerManager.PlayerBody.transform.position).normalized;

            // Calculate rotation to point towards the nearest enemy
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, dirToEnemy);
            pointer.transform.rotation = targetRotation;
        }
    }
}
