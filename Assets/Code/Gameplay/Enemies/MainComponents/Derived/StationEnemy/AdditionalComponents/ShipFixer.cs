using Game.Utility;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class ShipFixer : MonoBehaviour
    {
        [Inject] private DockPlace _dockPlace;

        [SerializeField] private float _healPerSec = 1.5f;
        [SerializeField] private float _maxHealPerDock = 50;

        private Coroutine _fixingCoroutine;

        private float _thisDockHealAmount = 0;

        private void Start()
        {
            _dockPlace.OnDock += StartFixing;
            _dockPlace.OnUndock += EndFixing;
        }

        private void StartFixing(IDocking ship)
        {
            if (!ship.Body.transform.TryGetComponent(out EnemyBase enemy))
            {
                Debug.LogError("No " + nameof(EnemyBase));
                return;
            }

            _thisDockHealAmount = 0;
            ship.CanUndock += () => IsFixingDone(enemy);

            _fixingCoroutine = StartCoroutine(Fixing(enemy));
        }

        private IEnumerator Fixing(EnemyBase enemy)
        {
            while (true)
            {
                if(!IsFixingDone(enemy))
                {
                    float heal = _healPerSec * Time.deltaTime;
                    enemy.GetHeal(heal);
                    _thisDockHealAmount += heal;
                }

                yield return null;
            }
        }

        private bool IsFixingDone(EnemyBase enemy)
        {
            bool isFullHp = enemy.CurrentHp >= enemy.MaxHp;
            bool maxHealDone = _thisDockHealAmount >= _maxHealPerDock;
            return isFullHp || maxHealDone;
        }

        private void EndFixing(IDocking _)
        {
            this.StopAndClearCoroutine(ref _fixingCoroutine);
        }
    }
}
