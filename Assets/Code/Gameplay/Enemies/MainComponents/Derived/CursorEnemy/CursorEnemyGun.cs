using Game.Combat;
using Game.Management;
using Game.Room.Enemy;
using Game.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game
{
    public class CursorEnemyGun : EnemyGunBase
    {
        [Inject] private PlayerManager _playerManager;
        [Inject] private EnemyManager _enemyManager;
        [Inject] private Rigidbody2D _body;

        [SerializeField] private Transform _gunTransform;
        [SerializeField] private ShootableObjectBase _bulletPrototype;
        [SerializeField] private float _shotInterval = 0.5f;
        [SerializeField] private int _magCapasity = 5;
        [SerializeField] private float _reloadTime = 7f;
        [SerializeField] private float _gunTravers = 45f;

        private float _lastShotTime = 0f;
        private float _endReloadTime = 0f;
        private int _currenaMagAmmo = 0;

        public override void Prepare()
        {
            _lastShotTime = Time.time;
        }

        protected override void OnAimingAt(Transform target)
        {
            Vector2 gunPos = (Vector2)_gunTransform.position;

            Vector2 vectorToTarget = target.position - transform.position;
            float angleToTarget = Vector2.SignedAngle(_body.transform.up, vectorToTarget);

            float newAngle;

            if (angleToTarget >= -_gunTravers / 2 || angleToTarget <= _gunTravers / 2)
            {
                OnAimTarget?.Invoke();
            }

            newAngle = Mathf.Clamp(angleToTarget, -_gunTravers / 2, _gunTravers / 2);

            _gunTransform.localRotation = Quaternion.Euler(0, 0, newAngle);
        }


    }
}
