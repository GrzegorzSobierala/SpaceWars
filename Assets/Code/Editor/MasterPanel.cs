using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Linq;
using Game.Utility;

namespace Game.Editor
{
    public class MasterPanel : EditorWindow
    {
        static string scenePath = "Assets/Scenes/";
        static Vector2 scroll;

        [MenuItem("Window/MasterPanel")]
        static void Init()
        {
            MasterPanel window = (MasterPanel)GetWindow(typeof(MasterPanel));
            window.titleContent = new GUIContent("Master Panel");
            window.Show();
        }

        void OnGUI()
        {
            scroll = GUILayout.BeginScrollView(scroll);
            if (!Application.isPlaying)
                OnGUI_Editor();

            GUILayout.EndScrollView();
        }

        void OnGUI_Editor()
        {
            GUILayout.Label("SCENE MANAGEMENT", EditorStyles.boldLabel);

            if (GUILayout.Button("Start up"))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                LoadSceneGroup(Scenes.MainManagment);
            }

            if (GUILayout.Button("Main menu"))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                LoadSceneGroup(Scenes.MainMenu);
            }

            if (GUILayout.Button("Testing"))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                LoadSceneGroup(Scenes.Testing);
            }

            GUILayout.Space(15);
        }

        void LoadSceneGroup(string[] scenes)
        {
            for (int i = 0; i < EditorSceneManager.sceneCount; ++i)
                EditorSceneManager.CloseScene(EditorSceneManager.GetSceneAt(i), true);

            for (int i = 0; i < scenes.Length; ++i)
            {
                string path = scenePath + scenes[i] + ".unity";
                EditorSceneManager.OpenScene(path, i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
            }

            EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByName(scenes[scenes.Length - 1]));
        }
    }
}
