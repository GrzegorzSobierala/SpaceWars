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
        [Inject] ModuleCreator _moduleCreator;
        [Inject] Rigidbody2D _body;
        [Inject] PlayerMovement2D _movement;

        public ModuleHandler ModuleHandler => _moduleHandler;
        public ModuleCreator ModuleCreator => _moduleCreator;
        public Rigidbody2D PlayerBody => _body;
        public PlayerMovement2D PlayerMovement => _movement;
    }
}
