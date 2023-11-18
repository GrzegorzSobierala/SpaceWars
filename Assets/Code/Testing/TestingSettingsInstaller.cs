using UnityEditor;
using UnityEngine;
using Zenject;

namespace Game.Testing
{
    [CreateAssetMenu(fileName = "MasterPanelSettingsInstaller", menuName = "Installers/MasterPanelSettingsInstaller")]
    public class TestingSettingsInstaller : ScriptableObjectInstaller<TestingSettingsInstaller>
    {
        public TestingSettings Settings;

        public override void InstallBindings()
        {
            Container.BindInstance(this);
            Container.BindInstance(Settings);
        }

        public void MarkDirty()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}