using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Game.Utility.Globals;
using Game.Testing;
using Zenject;

namespace Game.Editor
{
    public class MasterPanel : ZenjectEditorWindow
    {
        [Inject] private TestingSettings settings;
        [Inject] private TestingSettingsInstaller settingsInstaller;

        static string scenePath = "Assets/Scenes/";
        static Vector2 scroll;
        
        public override void InstallBindings()
        {
            TestingSettingsInstaller.CheckResources();
            TestingSettingsInstaller.InstallFromResource(Container);
        }

        [MenuItem("SpaceWars/MasterPanel")]
        private static void Init()
        {
            MasterPanel window = (MasterPanel)GetWindow(typeof(MasterPanel));
            window.titleContent = new GUIContent("Master Panel");
            window.Show();
        }

        public override void OnGUI()
        {
            base.OnGUI();

            scroll = GUILayout.BeginScrollView(scroll);
            if (!Application.isPlaying)
                OnGuiNotPlayMode();

            OnGuiAlways();

            GUILayout.EndScrollView();
        }

        private void OnGuiNotPlayMode()
        {
            SceneButtons();
        }

        private void OnGuiAlways()
        {
            TestingProperies();
        }

        private void SceneButtons()
        {
            GUILayout.Label("SCENE MANAGEMENT", EditorStyles.boldLabel);

            if (GUILayout.Button("Start up"))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                LoadSceneGroup(Scenes.MainManagmentMulti);
            }

            if (GUILayout.Button("Main menu"))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                LoadSceneGroup(Scenes.MainMenuMulti);
            }

            if (GUILayout.Button("Testing"))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                LoadSceneGroup(Scenes.TestingMulti);
            }
        }

        private void TestingProperies()
        {
            GUILayout.Space(10);
            GUILayout.Label("TESTING", EditorStyles.boldLabel);

            settings.AutoLoadRoom = GUILayout.Toggle(settings.AutoLoadRoom, "Auto load room");
            settingsInstaller.MarkDirty();

        }

        private void LoadSceneGroup(string[] scenes)
        {
            for (int i = 0; i < scenes.Length; ++i)
            {
                string path = scenePath + scenes[i] + ".unity";
                EditorSceneManager.OpenScene(path, i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
            }

            EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByName(scenes[scenes.Length - 1]));
        }
    }
}
