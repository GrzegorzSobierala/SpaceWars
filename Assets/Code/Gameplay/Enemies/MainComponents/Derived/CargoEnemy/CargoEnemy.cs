using Game.Combat;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class CargoEnemy : EnemyBase
    {
        [SerializeField] Color _emisiveColor;
        [SerializeField] float _emisiveIntesvity = 20f;
        [Space]
        [SerializeField] private MeshRenderer _shipMeshRenderer;

        private Material _emisiveMaterial;
        private Color _baseColor;
        private int _emisionID;

        protected override void Awake()
        {
            base.Awake();

            _emisionID = Shader.PropertyToID("_EmissionColor");
            _emisiveMaterial = _shipMeshRenderer.materials[0];
            _baseColor = _emisiveMaterial.GetColor(_emisionID);
        }

        public override void GetDamage(DamageData damage)
        {
            ChangeCurrentHp(-damage.BaseDamage);
        }

        public void ChangeToReloaded()
        {
            _emisiveMaterial.SetColor(_emisionID, Color.red * _emisiveIntesvity);
        }

        public void ChangeToReloading()
        {
            _emisiveMaterial.SetColor(_emisionID, _baseColor);
        }
    }
}
