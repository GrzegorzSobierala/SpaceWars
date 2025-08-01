using Game.Combat;
using Game.Player.Control;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public abstract class HullBase : ShipPart, IHittable
    {
        [Inject] protected VirtualCameraController _cameraController;
        [Inject] protected Rigidbody2D _body;

        [SerializeField] private float shakeMulti = 5f;

        public void GetHit(DamageData damage)
        {
            OnGetHit(damage);

            Vector2 shakeVector = _body.position - damage.HitPoint;
            shakeVector = shakeVector.normalized * damage.BaseDamage * shakeMulti;
            _cameraController.ShakeCamera(shakeVector);
        }

        public abstract void OnGetHit(DamageData damage);
    }
}
