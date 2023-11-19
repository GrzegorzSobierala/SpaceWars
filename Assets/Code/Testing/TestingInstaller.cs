using UnityEngine;
using Zenject;
using Game.Testing;

public class TestingInstaller : MonoInstaller
{
    [SerializeField] private TestingSettingsInstaller testingSettingsInstaller;

    public override void InstallBindings()
    {
#if UNITY_EDITOR
        TestingSettingsInstaller.CheckResources();
        TestingSettingsInstaller.InstallFromResource(Container);
#else
        testingSettingsInstaller.InstallBindings();
#endif

    }
}