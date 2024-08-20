using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Game.Utility.Globals;
using Game.Testing;
using Zenject;
using UnityEngine.SceneManagement;
using Unity.Mathematics;

namespace Game.Editor
{
    public class MasterPanel : ZenjectEditorWindow
    {
        [Inject] private TestingSettings settings;
        [Inject] private TestingSettingsInstaller settingsInstaller;

        static string scenePath = "Assets/Scenes/";
        static Vector2 scroll;
        string _currentTimeScaleText = "";
        bool _wasAppPlayLastFrame = false;
        bool _isFirstFrameOfAppPlay = false;

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
            window._currentTimeScaleText = window.settings.TimeScale.ToString();
            window._wasAppPlayLastFrame = Application.isPlaying;
        }

        
        public override void OnGUI()
        {
            _isFirstFrameOfAppPlay = !_wasAppPlayLastFrame && Application.isPlaying;

            base.OnGUI();

            scroll = GUILayout.BeginScrollView(scroll);

            if (!Application.isPlaying)
                OnGuiNotPlayMode();

            OnGuiAlways();

            if (Application.isPlaying)
                OnGuiPlayMode();

            GUILayout.EndScrollView();

            _wasAppPlayLastFrame = Application.isPlaying;
        }

        private void OnGuiNotPlayMode()
        {
            SceneButtons();
        }

        private void OnGuiAlways()
        {
            TestingProperies();
            TimeScaleTextInput();
        }

        private void OnGuiPlayMode()
        {
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

                Scene scene = SceneManager.GetSceneByName(Scenes.PlayerTesting);
                SceneManager.SetActiveScene(scene);
            }
        }


        private void TestingProperies()
        {
            GUILayout.Space(10);
            GUILayout.Label("TESTING", EditorStyles.boldLabel);
            settings.AutoLoadRoom = GUILayout.Toggle(settings.AutoLoadRoom, "Auto load room");
        }

        private void TimeScaleTextInput()
        {
            GUILayout.BeginHorizontal();
            string timeScaleText = GUILayout.TextField(_currentTimeScaleText, GUILayout.Width(30));
            GUILayout.Label("TimeScale (0-10)", GUILayout.Width(110));
            GUILayout.EndHorizontal();
            if (timeScaleText == "")
            {
                _currentTimeScaleText = "";
            }
            else if ((_currentTimeScaleText != timeScaleText || _isFirstFrameOfAppPlay) && 
                float.TryParse(timeScaleText, out float timeScale))
            {
                timeScale = math.clamp(timeScale, 0f, 10f);
                settings.TimeScale = timeScale;
                _currentTimeScaleText = timeScaleText;

                if (Application.isPlaying)
                {
                    Time.timeScale = timeScale;
                }
            }
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
