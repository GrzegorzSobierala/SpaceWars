using Game.Management;
using UnityEngine;
using Zenject;

public class PlayerManagerInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<PlayerManager>().FromComponentOn(gameObject).AsSingle();
    }
}