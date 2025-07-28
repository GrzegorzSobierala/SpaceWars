using TMPro;
using UnityEngine;

namespace Game.Room.Enemy
{
    [RequireComponent(typeof(TextMeshPro))]
    public class AlarmActivatorTimer : MonoBehaviour
    {
        public float ActivationTime => _activatingTime;

        [SerializeField] private float _activatingTime = 8;

        private TextMeshPro _textMesh;

        private void Awake()
        {
            Init();
        }

        private void OnValidate()
        {
            GetComponent<TextMeshPro>().text = _activatingTime.ToString("0");
        }

        private void Init()
        {
            _textMesh = GetComponent<TextMeshPro>();
        }

        public void UpadteTimeLeft(float timeLeft)
        {
            _textMesh.text = timeLeft.ToString("0");
        }

        public void Activate()
        {
            if(!gameObject.activeSelf)
                return;

            _textMesh.gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            if (!gameObject.activeSelf)
                return;

            _textMesh.gameObject.SetActive(false);
        }
    }
}
