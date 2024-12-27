using UnityEngine;
using Zenject;
using Game.Testing;

public class TestingInstaller : MonoInstaller
{
    [SerializeField] private TestingSettingsInstaller testingSettingsInstaller;
    [SerializeField] private bool _runInBackgroundEditor = true;

    private void Awake()
    {
        Init();
    }

    public override void InstallBindings()
    {
#if UNITY_EDITOR
        TestingSettingsInstaller.CheckResources();
        TestingSettingsInstaller.InstallFromResource(Container);
#else
        Container.Inject(testingSettingsInstaller);
        testingSettingsInstaller.InstallBindings();
#endif
    }

    private void Init()
    {
#if UNITY_EDITOR
        Application.runInBackground = _runInBackgroundEditor;
#endif
    }
}