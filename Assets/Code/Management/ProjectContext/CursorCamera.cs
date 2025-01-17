using Game.Utility;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.Player.Control
{
    public class CursorCamera : MonoBehaviour
    {
        [SerializeField, ReadOnly] private Camera _camera;

        private void Start()
        {
            Init();
        }

        /// <summary>
        /// Screan point to 2D physic plane intersection point
        /// </summary>
        public Vector2 ScreanPositionOn2DIntersection(Vector2 position)
        {
            Vector3 planePoint = Vector3.zero;
            Vector3 planeNormal = Vector3.back;

            Ray ray = Utils.ScreenPointToRay(_camera, position);

            Vector3 difference = planePoint - ray.origin;
            float product1 = Vector3.Dot(difference, planeNormal); ;
            float product2 = Vector3.Dot(ray.direction, planeNormal);
            float distanceFromOriginToPlane = product1 / product2;
            Vector3 intersection = ray.origin + distanceFromOriginToPlane * ray.direction;

            Vector3 mainVector = Utils.ChangeVector3Z(Camera.main.transform.position, 0);
            Vector3 mainIntersectioon = intersection + mainVector;

            return mainIntersectioon;
        }

        public void UpdateCameraSettings()
        {
            if(_camera)
            {
                Destroy(_camera.gameObject);
            }

            Vector3 pos = Utils.ChangeVector3Z(Vector3.zero, Camera.main.transform.position.z);
            Quaternion rot = Camera.main.transform.rotation;
            _camera = Instantiate(Camera.main, pos, rot, transform);
            _camera.enabled = false;
            _camera.gameObject.tag = gameObject.tag;
            _camera.gameObject.layer = gameObject.layer;
            _camera.gameObject.name = "CursorCamera";

            List<Transform> cameraChilds = new();
            for (int i = 0; i < _camera.transform.childCount; i++)
            {
                cameraChilds.Add(_camera.transform.GetChild(i));
            }

            for (int i = 0; i < _camera.transform.childCount; i++)
            {
                Destroy(cameraChilds[i].gameObject);
            }

            Component[] cameraComponents = _camera.gameObject.GetComponents<Component>();

            for (int i = 0; i < cameraComponents.Length; i++)
            {
                if (cameraComponents[i] is Camera 
                    || cameraComponents[i] is UniversalAdditionalCameraData
                    || cameraComponents[i] is Transform)
                {
                    continue;
                }

                Destroy(cameraComponents[i]);
            }
        }

        public void CopyPosAndRotFromMainCam()
        {
            Vector3 pos = Utils.ChangeVector3Z(Vector3.zero, Camera.main.transform.position.z);
            Quaternion rot = Camera.main.transform.rotation;

            _camera.transform.position = pos;
            _camera.transform.rotation = rot;
        }

        private void Init()
        {
            UpdateCameraSettings();
        }
    }
}
