using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using Game.Input.System;
using Game.Testing;
using Game.Management;
using Game.Room;

namespace Game.Player.UI
{
    public class TestResetUi : MonoBehaviour
    {
        [Inject] private PlayerSceneManager _testSceneManager;
        [Inject] private InputProvider _inputProvider;
        [Inject] private TestingSettings _testingSettings;
        [Inject] private TestAlarmUI _alarmUI;
        [Inject] private PlayerManager _playerManager;
        [Inject] private TestingSettings _testing;

        [SerializeField] private Button _onOffButton;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private TextMeshProUGUI _currentTimerText;
        [SerializeField] private TextMeshProUGUI _timerListText;

        private float _startRoomTime = 0;
        private List<float> _winTimes = new List<float>();

        private void Start()
        {
            if (_testingSettings.AutoLoadRoom)
            {
                _inputProvider.SetGameplayInput();
                _testSceneManager.Load();
            }
            else
            {
                OnPanel();
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            UpdateCurrentRoomTimer();
        }

        private void UpdateCurrentRoomTimer()
        {
            float currentRoomTime = Time.time - _startRoomTime;

            _currentTimerText.text = currentRoomTime.ToString("0.0");
        }

        private void ExitGame()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void Subscribe()
        {
            _onOffButton.onClick.AddListener(OnOffPanel);
            _restartButton.onClick.AddListener(Restart);
            _exitButton.onClick.AddListener(ExitGame);
            _playerManager.OnPlayerDied += OnDeadPlayer;
            _testSceneManager.OnRoomMainObjectiveCompleted += OnRoomClear;
        }

        private void Unsubscribe()
        {
            _onOffButton.onClick.RemoveListener(OnOffPanel);
            _restartButton.onClick.RemoveListener(Restart);
            _exitButton.onClick.RemoveListener(ExitGame);
            _playerManager.OnPlayerDied -= OnDeadPlayer;
            _testSceneManager.OnRoomMainObjectiveCompleted -= OnRoomClear;
        }

        private void OnOffPanel()
        {
            if(_panel.activeSelf)
            {
                OffPanel();
            }
            else
            {
                OnPanel();
            }
        }

        private void OffPanel()
        {
            if(float.TryParse(_testing.TimeScale, out float testScale))
            {
                Time.timeScale = testScale;
            }
            else
            {
                Time.timeScale = 1;
            }


            _inputProvider.SetGameplayInput();
            _panel.SetActive(false);
        }

        private void OnPanel()
        {
            Time.timeScale = 0;
            _inputProvider.SetGameplayInput();
            _panel.SetActive(true);
        }

        private void Restart()
        {
            _startRoomTime = Time.time;
            _testSceneManager.RestartRoom();
            OffPanel();
            _alarmUI.Deactivate();
            _onOffButton.gameObject.SetActive(true);
            _messageText.text = "hello man";
            _playerManager.ModuleHandler.CurrentHull.SetStartHP();
        }

        private void OnDeadPlayer()
        {
            OnPanel();
            _onOffButton.gameObject.SetActive(false);
            _messageText.text = "u ded :((";
        }

        private void OnRoomClear()
        {
            float currentRoomTime = Time.time - _startRoomTime;
            _winTimes.Add(currentRoomTime);
            OnPanel();
            _onOffButton.gameObject.SetActive(false);
            _messageText.text = "wiktory rojale";

            _winTimes.Sort();

            string timesText = "";
            foreach (var time in _winTimes) 
            {
                timesText += time.ToString("0.0") + "\n";
            }

            _timerListText.text = timesText;
        }
    }
}
