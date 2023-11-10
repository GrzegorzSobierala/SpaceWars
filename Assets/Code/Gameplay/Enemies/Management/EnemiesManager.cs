using Game.Management;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game
{
    public class EnemiesManager : MonoBehaviour
    {
        [Inject] PlayerManager playerManager;
    }
}
