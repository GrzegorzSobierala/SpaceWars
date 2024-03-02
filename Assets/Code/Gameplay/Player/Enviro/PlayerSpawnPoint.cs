using Game.Management;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room
{
    public class PlayerSpawnPoint : MonoBehaviour
    {
        [Inject] private PlayerManager _playerManager;

        private void Start()
        {
            _playerManager.PlayerBody.position = transform.position;
            gameObject.SetActive(false);
        }
    }
}
