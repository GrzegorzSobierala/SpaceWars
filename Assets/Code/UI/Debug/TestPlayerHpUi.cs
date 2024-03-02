using Game.Management;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Player.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TestPlayerHpUi : MonoBehaviour
    {
        [Inject] private PlayerManager playerManager;

        private TextMeshProUGUI _textMesh;

        private void Awake()
        {
            _textMesh = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            _textMesh.text = playerManager.ModuleHandler.CurrentHull.CurrentHp.ToString();
        }
    }
}
