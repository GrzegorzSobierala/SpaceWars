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
using System.Collections.Generic;

namespace Game.Editor
{
    public class MasterPanel : ZenjectEditorWindow
    {
        [Inject] private TestingSettings _settings;
        [Inject] private TestingSettingsInstaller _settingsInstaller;

        private static Vector2 scroll;
        private string _currentTimeScaleText = "";
        private string _currentPlayerHp = "";
        private bool _isEnemyMovement = true;
        private bool _wasAppPlayLastFrame = false;
        private bool _isFirstFrameOfAppPlay = false;
        private Dictionary<EnemyMovementBase, float> speedByMovement = new();

        [MenuItem("SpaceWars/MasterPanel")]
        private static void Init()
        {
            MasterPanel window = (MasterPanel)GetWindow(typeof(MasterPanel));
            window.titleContent = new GUIContent("Master Panel");
            window.Show();
        }

        public override void InstallBindings()
        {
            TestingSettingsInstaller.CheckResources();
            TestingSettingsInstaller.InstallFromResource(Container);
        }

        public override void OnEnable()
        {
            base.OnEnable();

            _currentTimeScaleText = _settings.TimeScale;
            _currentPlayerHp = _settings.PlayerHp;
            _wasAppPlayLastFrame = Application.isPlaying;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public override void OnDisable()
        {
            _settings.EnemySpeedMulti = 1;
            SceneView.duringSceneGui -= OnSceneGUI;
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
            GUILayout.Label("EDIT MODE PROPERTIES", EditorStyles.boldLabel);
            SceneButtons();
            ShowSelectedFovToogle();
        }

        private void OnGuiAlways()
        {
            GUILayout.Space(10);
            GUILayout.Label("ALWAYS ON PROPERTIES", EditorStyles.boldLabel);
            TestingProperies();
            TimeScaleTextInput();
            PlayerHpInput();
        }

        private void OnGuiPlayMode()
        {
            GUILayout.Space(10);
            GUILayout.Label("PLAY MODE PROPERTIES", EditorStyles.boldLabel);
            EnemyMovementToggle();
            EnemyShootingToogle();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            TryShowFovs();
        }

        private void SceneButtons()
        {
            GUILayout.Label("Scene managment", EditorStyles.boldLabel);

            if (GUILayout.Button("Start up"))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                LoadSceneGroup(Scenes.GameInitMulti);
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

                Scene scene = SceneManager.GetSceneByName(Scenes.Player);
                SceneManager.SetActiveScene(scene);
            }
            GUILayout.Space(10);
        }


        private void TestingProperies()
        {
            bool newAutoLoadRoom = GUILayout.Toggle(_settings.AutoLoadRoom, "Auto load room");
            if (newAutoLoadRoom != _settings.AutoLoadRoom)
            {
                _settings.AutoLoadRoom = newAutoLoadRoom;
                _settingsInstaller.MarkDirty();
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
                _settings.TimeScale = _currentTimeScaleText;
                _settingsInstaller.MarkDirty();
            }
            else if ((_currentTimeScaleText != timeScaleText || _isFirstFrameOfAppPlay) && 
                float.TryParse(timeScaleText, out float timeScale))
            {
                timeScale = math.clamp(timeScale, 0f, 10f);
                _settings.TimeScale = timeScale.ToString();
                _currentTimeScaleText = _settings.TimeScale;
                _settingsInstaller.MarkDirty();

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
                _settings.PlayerHp = _currentPlayerHp;
                _settingsInstaller.MarkDirty();
            }
            else if ((_currentPlayerHp != playerHpText || _isFirstFrameOfAppPlay) &&
                int.TryParse(playerHpText, out int playerHp))
            {
                playerHp = math.clamp(playerHp, 1, 9999);
                _settings.PlayerHp = playerHp.ToString();
                _currentPlayerHp = _settings.PlayerHp;
                _settingsInstaller.MarkDirty();

                if (Application.isPlaying)
                {
                    FindObjectOfType<HullModuleBase>().DEBUG_TrySetHp(_currentPlayerHp);
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
                        _settings.EnemySpeedMulti = 0;
                    }
                    else if(movement.CurrentSpeedModifier == 0 && speedByMovement.ContainsKey(movement))
                    {
                        movement.SetSpeedModifier(speedByMovement[movement]);
                        _settings.EnemySpeedMulti = 1;
                    }
                }
            }
        }

        private void ShowSelectedFovToogle()
        {
            bool newShowEnemiesFov = GUILayout.Toggle(_settings.ShowEnemiesFov, "Show selected enemies FOV");

            if(newShowEnemiesFov != _settings.ShowEnemiesFov)
            {
                _settings.ShowEnemiesFov = newShowEnemiesFov;
                _settingsInstaller.MarkDirty();
            }
        }

        private void TryShowFovs()
        {
            if (_settings.ShowEnemiesFov)
            {
                List<EnemyFieldOfView> fovs = new();
                foreach (var go in Selection.gameObjects)
                {
                    if (go.TryGetComponent(out EnemyFieldOfView fov))
                    {
                        if (!fovs.Contains(fov))
                        {
                            fovs.Add(fov);
                        }
                    }
                    foreach (var childFov in go.GetComponentsInChildren<EnemyFieldOfView>())
                    {
                        if (!fovs.Contains(childFov))
                        {
                            fovs.Add(childFov);
                        }
                    }
                }
                foreach (var fov in fovs)
                {
                    fov.DrawViewGizmos();
                }
            }
        }

        private void EnemyShootingToogle()
        {
            if (_isFirstFrameOfAppPlay)
            {
                _settings.EnableEnemyShooting = true;
            }

            bool newDisableShooting = GUILayout.Toggle(_settings.EnableEnemyShooting, "EnemyShooting");

            if (newDisableShooting != _settings.EnableEnemyShooting)
            {
                _settings.EnableEnemyShooting = newDisableShooting;
                _settingsInstaller.MarkDirty();
            }
        }

        private void LoadSceneGroup(string[] scenes)
        {
            for (int i = 0; i < scenes.Length; ++i)
            {
                string path = GetScenePathByName(scenes[i]);
                EditorSceneManager.OpenScene(path, i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
            }

            EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByName(scenes[scenes.Length - 1]));
        }

        private string GetScenePathByName(string sceneName)
        {
            string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene");

            foreach (string guid in sceneGUIDs)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                string sceneAssetName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneAssetName == sceneName)
                {
                    return scenePath; 
                }
            }
            Debug.LogError($"Scene with name '{sceneName}' not found in the project.");
            return null;
        }
    }
}
