using System.Collections;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class ShipFixer : MonoBehaviour
    {
        [Inject] private DockPlace _dockPlace;

        [SerializeField] private float _healPerSec = 1.5f;

        Coroutine _fixingCoroutine;

        private void Start()
        {
            _dockPlace.OnDock += StartFixing;
        }

        private void StartFixing(IDocking ship)
        {
            EnemyBase enemy = ship.Body.transform.GetComponent<EnemyBase>();

            if (enemy == null)
            {
                Debug.LogError("No " + nameof(EnemyBase));
                return;
            }

            ship.CanUndock += () => IsEnemyFixed(enemy);

            _fixingCoroutine = StartCoroutine(Fixing(enemy));
        }

        private IEnumerator Fixing(EnemyBase enemy)
        {
            while(!IsEnemyFixed(enemy))
            {
                enemy.GetHeal(_healPerSec * Time.deltaTime);
                yield return null;
            }

            _fixingCoroutine = null;
        }

        private bool IsEnemyFixed(EnemyBase enemy)
        {
            return enemy.CurrentHp >= enemy.MaxHp;
        }
    }
}
