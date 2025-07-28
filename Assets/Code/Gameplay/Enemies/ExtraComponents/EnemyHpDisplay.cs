using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    [RequireComponent(typeof(TextMeshPro))]
    public class EnemyHpDisplay : MonoBehaviour
    {
        [Inject] private EnemyBase _enemy;

        private TextMeshPro _textMesh;

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            Subscribe();
            UpdateText(_enemy.CurrentHp);
        }

        void LateUpdate()
        {
            UpdateRotationToCamera();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Init()
        {
            _textMesh = GetComponent<TextMeshPro>();
        }

        private void UpdateRotationToCamera()
        {
            float cameraRoll = Camera.main.transform.eulerAngles.z;

            Vector3 currentRotation = transform.eulerAngles;
            transform.eulerAngles = new Vector3(currentRotation.x, currentRotation.y, -cameraRoll);
        }

        private void Subscribe()
        {
            _enemy.OnHpChange += UpdateText;
        }

        private void Unsubscribe()
        {
            _enemy.OnHpChange -= UpdateText;
        }

        private void UpdateText(float value)
        {
            _textMesh.text = value.ToString("f1");
        }
    }
}
