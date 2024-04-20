using Game.Combat;
using Game.Player.VirtualCamera;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public abstract class HullBase : ShipPart, IHittable
    {
        [Inject] VirtualCameraController _cameraController;
        [Inject] Rigidbody2D _body;

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
