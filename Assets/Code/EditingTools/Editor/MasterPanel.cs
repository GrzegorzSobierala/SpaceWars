using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Game.Utility.Globals;
using Game.Testing;
using Zenject;
using UnityEngine.SceneManagement;
using Unity.Mathematics;
using Game.Player.Ship;
using Game.Room.Enemy;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;

namespace Game.Editor
{
    public class MasterPanel : ZenjectEditorWindow
    {
        [Inject] private TestingSettings settings;
        [Inject] private TestingSettingsInstaller settingsInstaller;

        private static string scenePath = "Assets/Scenes/";
        private static Vector2 scroll;
        private string _currentTimeScaleText = "";
        private string _currentPlayerHp = "";
        private bool _isEnemyMovement = true;
        private bool _wasAppPlayLastFrame = false;
        private bool _isFirstFrameOfAppPlay = false;
        private Dictionary<EnemyMovementBase, float> speedByMovement = new();

        public override void OnEnable()
        {
            base.OnEnable();

            _currentTimeScaleText = settings.TimeScale;
            _currentPlayerHp = settings.PlayerHp;
            _wasAppPlayLastFrame = Application.isPlaying;
        }

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
            PlayerHpInput();
        }

        private void OnGuiPlayMode()
        {
            EnemyMovementToggle();
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
            bool newAutoLoadRoom = GUILayout.Toggle(settings.AutoLoadRoom, "Auto load room");
            if (newAutoLoadRoom != settings.AutoLoadRoom)
            {
                settings.AutoLoadRoom = newAutoLoadRoom;
                settingsInstaller.MarkDirty();
            }
        }

        private void TimeScaleTextInput()
        {
            GUILayout.BeginHorizontal();
            string timeScaleText = GUILayout.TextField(_currentTimeScaleText, GUILayout.Width(30));
            GUILayout.Label("TimeScale (0-10)", GUILayout.Width(110));
            GUILayout.EndHorizontal();

            if (_currentTimeScaleText != timeScaleText && timeScaleText == "")
            {
                _currentTimeScaleText = "";
                settings.TimeScale = _currentTimeScaleText;
                settingsInstaller.MarkDirty();
            }
            else if ((_currentTimeScaleText != timeScaleText || _isFirstFrameOfAppPlay) && 
                float.TryParse(timeScaleText, out float timeScale))
            {
                timeScale = math.clamp(timeScale, 0f, 10f);
                settings.TimeScale = timeScale.ToString();
                _currentTimeScaleText = settings.TimeScale;
                settingsInstaller.MarkDirty();

                if (Application.isPlaying)
                {
                    Time.timeScale = timeScale;
                }
            }
        }

        private void PlayerHpInput()
        {
            GUILayout.BeginHorizontal();
            string playerHpText = GUILayout.TextField(_currentPlayerHp, GUILayout.Width(40));
            GUILayout.Label("Player HP (1 - 9999)", GUILayout.Width(130));
            GUILayout.EndHorizontal();

            if (_currentPlayerHp != playerHpText && playerHpText == "")
            {
                _currentPlayerHp = "";
                settings.PlayerHp = _currentPlayerHp;
                settingsInstaller.MarkDirty();
            }
            else if ((_currentPlayerHp != playerHpText || _isFirstFrameOfAppPlay) &&
                int.TryParse(playerHpText, out int playerHp))
            {
                playerHp = math.clamp(playerHp, 1, 9999);
                settings.PlayerHp = playerHp.ToString();
                _currentPlayerHp = settings.PlayerHp;
                settingsInstaller.MarkDirty();

                if (Application.isPlaying)
                {
                    FindObjectOfType<HullModuleBase>().DEBUG_SetHp(playerHp);
                }
            }
        }

        private void EnemyMovementToggle()
        {
            if(_isFirstFrameOfAppPlay)
            {
                _isEnemyMovement = true;
            }

            bool newIsEnemyMovement = GUILayout.Toggle(_isEnemyMovement, "Enemies movement");
            if(newIsEnemyMovement != _isEnemyMovement)
            {
                _isEnemyMovement = newIsEnemyMovement;

                if (!newIsEnemyMovement)
                {
                    speedByMovement.Clear();
                }

                foreach (var movement in FindObjectsOfType<EnemyMovementBase>())
                {
                    if(!newIsEnemyMovement)
                    {
                        speedByMovement.Add(movement, movement.CurrentSpeedModifier);
                        movement.SetSpeedModifier(0);
                    }
                    else if(movement.CurrentSpeedModifier == 0 && speedByMovement.ContainsKey(movement))
                    {
                        movement.SetSpeedModifier(speedByMovement[movement]);
                    }
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
