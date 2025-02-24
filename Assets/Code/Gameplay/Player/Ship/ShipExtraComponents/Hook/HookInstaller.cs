using Game.Utility;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public class HookInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Utils.BindGetComponent<SpringJoint2D>(Container, gameObject);
            Utils.BindGetComponent<Hook>(Container, gameObject);
        }
    }
}
