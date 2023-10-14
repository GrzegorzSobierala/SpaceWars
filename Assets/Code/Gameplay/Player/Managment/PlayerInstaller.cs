using UnityEngine;
using Zenject;
using Cinemachine;

namespace Game.Player
{
    public class PlayerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<PlayerEventsHandler>().FromComponentOn(gameObject).AsSingle();
            Container.Bind<Rigidbody2D>().FromComponentOn(gameObject).AsSingle();
        }
    }
}