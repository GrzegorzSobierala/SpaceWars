using Game.Utility;
using NaughtyAttributes;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class FogEffectController : MonoBehaviour
    {
        [Inject] private Rigidbody2D _body;

        [SerializeField, MinMaxSlider(0, 1000)] private Vector2 _fogEffectMinMaxSpeed;
        [SerializeField, MinMaxSlider(0, 1)] private Vector2 _fogEffectMinMaxAlpha;
        [SerializeField] private ParticleSystem[] _fogEffects;
        [SerializeField] private bool _toggleFogEffectRot;

        [ShowNativeProperty] private float CurrentSpeed => _body ? _body.velocity.magnitude : 0f;


        private void Update()
        {
            float fogEffectStrenght = Utils.Remap(_body.velocity.magnitude, _fogEffectMinMaxSpeed.x,
                _fogEffectMinMaxSpeed.y, _fogEffectMinMaxAlpha.x, _fogEffectMinMaxAlpha.y);

            fogEffectStrenght = Mathf.Clamp(fogEffectStrenght, _fogEffectMinMaxAlpha.x,
                _fogEffectMinMaxAlpha.y);

            foreach (var effect in _fogEffects)
            {
                var color = effect.startColor;
                color.a = fogEffectStrenght;
                effect.startColor = color;

                if (_toggleFogEffectRot)
                {
                    effect.transform.rotation = Quaternion.LookRotation(-_body.velocity, Vector3.back);
                }
            }
        }
    }
}
