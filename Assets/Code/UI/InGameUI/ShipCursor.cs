using Game.Input.System;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Player.UI
{
    public class ShipCursor : MonoBehaviour
    {
        [Inject] private InputProvider _inputProvider;

        [SerializeField, AutoFill] private Rigidbody2D aaaaaaaaasasaaaa;
        [SerializeField, AutoFill] private Rigidbody2D aaaaaaaaasasaaaaasasa;
        [SerializeField, AutoFill] private Rigidbody2D aaaaaaaaasasaaaaasadassasa;

        private void Start()
        {
        }
    }
}
