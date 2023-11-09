using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Player.Ship;
using Zenject;

namespace Game.Management
{
    public class PlayerManager : MonoBehaviour
    {
        [Inject] ModuleHandler _moduleHandler;
        [Inject] Rigidbody2D _body;

        public ModuleHandler ModuleHandler => _moduleHandler;
        public Rigidbody2D PlayerBody => _body;
    }
}
