using UnityEditor;
using UnityEngine;
using Zenject;

namespace Game.Testing
{
    [CreateAssetMenu(fileName = "MasterPanelSettingsInstaller", menuName = "Installers/MasterPanelSettingsInstaller")]
    public class TestingSettingsInstaller : ScriptableObjectInstaller<TestingSettingsInstaller>
    {
        private const string settingsPath = "Assets/Data/Testing/Resources/Installers/";
        private const string settingsName = "TestingSettingsInstaller.asset";


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

        public static void CheckResources()
        {
            TestingSettingsInstaller installer =
                Resources.Load<TestingSettingsInstaller>("Installers/TestingSettingsInstaller");

            if (installer == null)
            {
                installer = CreateInstance<TestingSettingsInstaller>();

                AssetDatabase.CreateAsset(installer, settingsPath + settingsName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("Created a new TestingSettingsInstaller and saved it to"
                    + settingsPath + settingsName);

            }
        }
    }
}