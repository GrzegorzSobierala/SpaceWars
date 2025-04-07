using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using Game.Input.System;
using Game.Testing;
using Game.Management;
using Game.Room;
using UnityEngine.InputSystem;

namespace Game.Player.Ui
{
    public class TestResetUi : MonoBehaviour
    {
        [Inject] private PlayerSceneManager _playerSceneManager;
        [Inject] private InputProvider _input;
        [Inject] private AlarmUi _alarmUI;
        [Inject] private PlayerManager _playerManager;
        [Inject] private TestingSettings _testing;

        [SerializeField] private Button _onOffButton;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _tutorialOpenButton;
        [SerializeField] private Button _tutorialCloseButton;
        [SerializeField] private GameObject _tutorialPanel;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private TextMeshProUGUI _currentTimerText;
        [SerializeField] private TextMeshProUGUI _timerListText;

        private float _startRoomTime = 0;
        private List<float> _winTimes = new List<float>();

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            _messageText.text = "";
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
            _continueButton.onClick.AddListener(OnOffPanel);
            _restartButton.onClick.AddListener(Restart);
            _exitButton.onClick.AddListener(ExitGame);
            _playerManager.OnPlayerDied += OnDeadPlayer;
            _playerSceneManager.OnEndRoom += OnEndRoom;
            _input.PlayerControls.Gameplay.Pasue.performed += OnOffPanelForInput;
            _input.PlayerControls.Ui.Back.performed += OnOffPanelForInput;
            _tutorialOpenButton.onClick.AddListener(OpenTutorial);
            _tutorialCloseButton.onClick.AddListener(CloseTutorial);
        }

        private void Unsubscribe()
        {
            _onOffButton.onClick.RemoveListener(OnOffPanel);
            _continueButton.onClick.RemoveListener(OnOffPanel);
            _restartButton.onClick.RemoveListener(Restart);
            _exitButton.onClick.RemoveListener(ExitGame);
            _playerManager.OnPlayerDied -= OnDeadPlayer;
            _playerSceneManager.OnEndRoom -= OnEndRoom;
            _input.PlayerControls.Gameplay.Pasue.performed -= OnOffPanelForInput;
            _input.PlayerControls.Ui.Back.performed -= OnOffPanelForInput;
            _tutorialOpenButton.onClick.RemoveListener(OpenTutorial);
            _tutorialCloseButton.onClick.RemoveListener(CloseTutorial);
        }

        private void OnOffPanel()
        {
            if(_panel.activeSelf)
            {
                if(_tutorialPanel.activeSelf)
                {
                    CloseTutorial();
                }
                else
                {
                    OffPanel();
                }
            }
            else
            {
                OnPanel();
            }
        }

        private void OnOffPanelForInput(InputAction.CallbackContext _)
        {
            OnOffPanel();
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

            _input.SwitchActionMap(_input.PlayerControls.Gameplay);
            _panel.SetActive(false);
        }

        private void OnPanel()
        {
            Time.timeScale = 0;
            _input.SwitchActionMap(_input.PlayerControls.Ui);
            _panel.SetActive(true);
        }

        private void Restart()
        {
            _playerSceneManager.RestartRoom(OnRestarted);
        }

        private void OnRestarted()
        {
            _startRoomTime = Time.time;
            OffPanel();
            _alarmUI.Deactivate();
            _onOffButton.interactable = true;
            _continueButton.interactable = true;
            _messageText.text = "";
            _playerManager.ModuleHandler.CurrentHull.SetStartHP();
        }

        private void OnDeadPlayer()
        {
            OnPanel();
            _onOffButton.interactable = false;
            _continueButton.interactable = false;
            _messageText.text = "u ded :((, try again :D?";
        }

        private void OnEndRoom()
        {
            float currentRoomTime = Time.time - _startRoomTime;
            _winTimes.Add(currentRoomTime);
            OnPanel();
            _onOffButton.interactable = false;
            _continueButton.interactable = false;
            _messageText.text = "TURBO WIN!!!";

            _winTimes.Sort();

            string timesText = "";
            foreach (var time in _winTimes) 
            {
                timesText += time.ToString("0.0") + "\n";
            }

            _timerListText.text = timesText;
        }

        private void OpenTutorial()
        {
            _tutorialPanel.SetActive(true);
        }

        private void CloseTutorial()
        {
            _tutorialPanel.SetActive(false);
        }

#if UNITY_EDITOR
#else
        private static bool wasTutorialShownFirstTime = false;
#endif

        public void OpenTutorialOnceForBuild()
        {
#if UNITY_EDITOR
#else
            if (!wasTutorialShownFirstTime)
            {
                wasTutorialShownFirstTime = true;
                OnOffPanel();
                OpenTutorial();
            }
#endif
        }
    }
}
