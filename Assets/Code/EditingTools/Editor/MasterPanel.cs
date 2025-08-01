using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Game.Testing;
using Zenject;
using UnityEngine.SceneManagement;
using Unity.Mathematics;
using Game.Player.Ship;
using Game.Room.Enemy;
using System.Collections.Generic;
using Game.Management;

namespace Game.Editor
{
    public class MasterPanel : ZenjectEditorWindow
    {
        [Inject] private TestingSettings _settings;
        [Inject] private TestingSettingsInstaller _settingsInstaller;
        [Inject] private ScenesData _scenesData;

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
            ScenesData.InstallFromResource(Container);
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
            ShowEnemyGuardPointsToggle();
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
            TryShowEnemyGuardPoints();
        }

        private void SceneButtons()
        {
            GUILayout.Label("Scene managment", EditorStyles.boldLabel);

            if (GUILayout.Button("Main menu"))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                LoadSceneGroup(new string[] {_scenesData.MainMeneScene});
            }

            if (GUILayout.Button("Loading"))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                LoadSceneGroup(new string[] {_scenesData.LoadingScene});
            }

            if (GUILayout.Button("Hub"))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                LoadSceneGroup(new string[] { _scenesData.PlayerScene, _scenesData.HubScene});

                Scene scene = SceneManager.GetSceneByName(_scenesData.PlayerScene);
                SceneManager.SetActiveScene(scene);
            }

            if (GUILayout.Button("Room scene"))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                if(_scenesData.RoomScenes.Length <= _settings.RoomSceneIndex)
                {
                    _settings.RoomSceneIndex = 0;
                }    

                LoadSceneGroup(new string[] {_scenesData.PlayerScene,
                    _scenesData.RoomScenes[_settings.RoomSceneIndex]});

                Scene scene = SceneManager.GetSceneByName(_scenesData.PlayerScene);
                SceneManager.SetActiveScene(scene);
            }

            int index = EditorGUILayout.Popup(
                "Room scene to load", _settings.RoomSceneIndex, _scenesData.RoomScenes);

            if(index != _settings.RoomSceneIndex)
            {
                _settings.RoomSceneIndex = index;
                _settingsInstaller.MarkDirty();
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
                bool hullneeded = Application.isPlaying;

                HullModuleBase hull = null;
                if (hullneeded)
                {
                    hull = FindObjectOfType<HullModuleBase>();
                }

                if (hullneeded && !hull)
                    return;

                playerHp = math.clamp(playerHp, 1, 9999);
                _settings.PlayerHp = playerHp.ToString();
                _currentPlayerHp = _settings.PlayerHp;
                _settingsInstaller.MarkDirty();

                if (Application.isPlaying)
                {
                    hull.DEBUG_TrySetHp(_currentPlayerHp);
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
                List<DEPRECATED_EnemyFieldOfView> fovs = new();
                foreach (var go in Selection.gameObjects)
                {
                    if (go.TryGetComponent(out DEPRECATED_EnemyFieldOfView fov))
                    {
                        if (!fovs.Contains(fov))
                        {
                            fovs.Add(fov);
                        }
                    }
                    foreach (var childFov in go.GetComponentsInChildren<DEPRECATED_EnemyFieldOfView>())
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

        private void ShowEnemyGuardPointsToggle()
        {
            bool newShowEnemiesFov = GUILayout.Toggle(_settings.ShowEnemyGuardPoints, "Show enemy guard points");

            if (newShowEnemiesFov != _settings.ShowEnemyGuardPoints)
            {
                _settings.ShowEnemyGuardPoints = newShowEnemiesFov;
                _settingsInstaller.MarkDirty();
            }
        }

        private void TryShowEnemyGuardPoints()
        {
            if (!_settings.ShowEnemyGuardPoints)
                return;

            List<(EnemyBase, PatrolController)> enemiesWithPatrol = new();
            if (Selection.gameObjects.Length == 0)
            {
                foreach (var enemy in FindObjectsOfType<EnemyBase>())
                {
                    PatrolController patrol = enemy.GetComponentInChildren<PatrolController>();
                    if (patrol == null)
                        continue;

                    enemiesWithPatrol.Add((enemy, patrol));
                }
            }
            else
            {
                foreach (var go in Selection.gameObjects)
                {
                    PatrolController patrol;

                    if (go.TryGetComponent(out patrol))
                    {
                        EnemyBase enemy = go.GetComponentInParent<EnemyBase>();

                        if (!enemy)
                        {
                            Debug.LogError($"PatrolController should be a child of {nameof(EnemyBase)}");
                            continue;
                        }

                        enemiesWithPatrol.Add((enemy, patrol));
                        continue;
                    }

                    foreach (var enemy in go.GetComponentsInChildren<EnemyBase>())
                    {
                        patrol = enemy.GetComponentInChildren<PatrolController>();
                        if (patrol == null)
                            continue;

                        enemiesWithPatrol.Add((enemy, patrol));
                    }
                }
            }

            foreach (var (enemy, patrol) in enemiesWithPatrol)
            {
                if (patrol.GuardPoints.Count < 2)
                {
                    Debug.LogError("There need to be at least 2 guard points");
                    continue;
                }
                for (int i = 0; i < patrol.GuardPoints.Count; ++i)
                {
                    Vector3 from = patrol.GuardPoints[i].position;
                    Vector3 to = patrol.GuardPoints[(i + 1) % patrol.GuardPoints.Count].position;
                    if(i == 0)
                    {
                        Debug.DrawLine(from, to, new Color(0.1f, 0.1f, 1, 1), 0);
                    }
                    else
                    {
                        Debug.DrawLine(from, to, new Color(1f, 1f, 1, 1), 0);
                    }
                }

                {
                    Vector3 from = enemy.transform.position;
                    Vector3 to = patrol.GuardPoints[0].position;
                    Debug.DrawLine(from, to, new Color(0, 0, 1, 1), 0);
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
