using Game.Input.System;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Player.UI
{
    public class ShipCursor : MonoBehaviour
    {
        [Inject] private InputProvider _inputProvider;

        [SerializeField] private GameObject _imagesParent;

        private void Start()
        {
            _imagesParent.SetActive(true);
        }

        private void Update()
        {
            
        }
    }
}
