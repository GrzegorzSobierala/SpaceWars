using Game.Combat;
using Game.Player.Control;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public class ScavengerHull : HullModuleBase
    {
        [Inject] private PlayerMovement2D _playerMovement;

        [SerializeField] private GameObject _frontParticles;
        [SerializeField] private GameObject _backParticles;
        [SerializeField] private GameObject _rightParticles;
        [SerializeField] private GameObject _lefttParticles;

        private void Start()
        {
            Subscribe();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Unsubscribe();
        }
    

        public override void OnGetHit(DamageData damage)
        {
            ChangeCurrentHp(-damage.BaseDamage);
        }

        protected override void Defeated()
        {
        }

        private void ChangeHorizontalParticles(int value)
        {
            if (value == 0)
            {
                _rightParticles.SetActive(false);
                _lefttParticles.SetActive(false);
            }
            else if (value == 1)
            {
                _rightParticles.SetActive(false);
                _lefttParticles.SetActive(true);
            }
            else if (value == -1)
            {
                _rightParticles.SetActive(true);
                _lefttParticles.SetActive(false);
            }
            else
            {
                Debug.LogError("Wrong value");
            }
        }

        private void ChangeVerdicalParticles(int value)
        {
            if (value == 0)
            {
                _frontParticles.SetActive(false);
                _backParticles.SetActive(false);
            }
            else if (value == 1)
            {
                _frontParticles.SetActive(false);
                _backParticles.SetActive(true);
            }
            else if (value == -1)
            {
                _frontParticles.SetActive(true);
                _backParticles.SetActive(false);
            }
            else
            {
                Debug.LogError("Wrong value");
            }
        }

        private void Subscribe()
        {
            _playerMovement.OnHorizontalMove += ChangeHorizontalParticles;
            _playerMovement.OnVerdicalMove += ChangeVerdicalParticles;
        }

        private void Unsubscribe()
        {
            _playerMovement.OnHorizontalMove -= ChangeHorizontalParticles;
            _playerMovement.OnVerdicalMove -= ChangeVerdicalParticles;
        }
    }
}
