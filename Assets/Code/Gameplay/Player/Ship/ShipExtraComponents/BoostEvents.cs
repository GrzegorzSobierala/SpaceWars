using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Game.Player.Ship
{
    public class BoostEvents : MonoBehaviour
    {
        [Inject] PlayerMovement2D _playerMovement;

        [SerializeField] UnityEvent boostForward;
        [SerializeField] UnityEvent boostBack;
        [SerializeField] UnityEvent boostRight;
        [SerializeField] UnityEvent boostLeft;

        private void Awake()
        {
            _playerMovement.OnBoost += InvokeCorrectBoostEvent;
        }

        private void OnDestroy()
        {
            _playerMovement.OnBoost -= InvokeCorrectBoostEvent;
        }

        private void InvokeCorrectBoostEvent(Vector2 boostVector)
        {
            if (boostVector.y > 0)
            {
                boostForward?.Invoke();
            }
            else if (boostVector.y < 0)
            {
                boostBack?.Invoke();
            }

            if (boostVector.x > 0)
            {
                boostRight?.Invoke();
            }
            else if(boostVector.x < 0)
            {
                boostLeft?.Invoke();
            }
        }
    }
}
