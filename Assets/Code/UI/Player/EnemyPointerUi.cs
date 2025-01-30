using UnityEngine;
using System.Collections.Generic;
using Zenject;
using Game.Management;
using Game.Room;
using Game.Room.Enemy;
using Game.Utility;

namespace Game.Player.Ui
{
    public class EnemyPointerUi : MonoBehaviour
    {
        [Inject] private PlayerManager _playerManager;
        [Inject] private PlayerSceneManager _sceneManager;

        public GameObject pointer;
        public Canvas canvas;
        public float visibilityThreshold = 0.1f;

        private Transform nearestEnemy;

        private List<EnemyBase> enemyPositions => _sceneManager.RoomEnemies;

        private void Update()
        {
            // Update nearest enemy
            nearestEnemy = GetNearestEnemy();

            // Show or hide pointer based on nearest enemy and its visibility
            if (nearestEnemy != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(nearestEnemy.position);

                if (IsEnemyOnScreen(screenPos))
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

        Transform GetNearestEnemy()
        {
            if(enemyPositions == null)
            {
                return null;
            }

            EnemyBase targetEnemy = null;
            float targetEnemyMagnitude = float.MaxValue;

            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            Plane[] neededPlanes = {frustumPlanes[0], frustumPlanes[1],
                frustumPlanes[2], frustumPlanes[3]};

            Vector3 cameraPos = Camera.main.transform.position;
            Vector2 playerPos = _playerManager.PlayerBody.transform.position;

            int planeIndexMove = 4;

            for (int enemyIndex = 0; enemyIndex < enemyPositions.Count; enemyIndex++)
            {
                EnemyBase enemy = enemyPositions[enemyIndex];
                if (!enemy)
                {
                    continue;
                }

                Vector3 enemyPos = enemy.transform.position;
                Vector3 targetPos = Utils.GetVector(cameraPos, enemyPos);
                bool isInside = true;

                for (int i = 0; i < neededPlanes.Length; i++)
                {
                    if (planeIndexMove == 4)
                    {
                        planeIndexMove = 0;
                    }
                    else
                    {
                        planeIndexMove++;
                    }

                    if (Vector3.Dot(neededPlanes[i].normal, targetPos) >= 0)
                    {
                        isInside = false;
                        break;
                    }
                }

                if (isInside)
                    continue;

                float magnitude = new Vector2(
                    playerPos.x - enemy.transform.position.x,
                    playerPos.y - enemy.transform.position.y).sqrMagnitude;

                if (targetEnemyMagnitude < magnitude)
                    continue;

                targetEnemy = enemy;
                targetEnemyMagnitude = magnitude;
            }

            return targetEnemy.transform;
        }

        bool IsEnemyOnScreen(Vector3 screenPos)
        {
            return screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height;
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
            Vector3 dirToEnemy = (nearestEnemy.position - Camera.main.transform.position).normalized;

            // Calculate rotation to point towards the nearest enemy
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, dirToEnemy);
            pointer.transform.rotation = targetRotation;
        }
    }
}
