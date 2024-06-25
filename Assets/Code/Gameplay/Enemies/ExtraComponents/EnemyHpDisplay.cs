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

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Init()
        {
            _textMesh = GetComponent<TextMeshPro>();
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
