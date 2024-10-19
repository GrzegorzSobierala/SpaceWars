using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Dialogues
{
    public class ChoicesDisplay : MonoBehaviour
    {
        [Inject] private ChoiceHubDialogueDisplay _choiceHubDialogueDisplay;
        [Inject] private DiContainer _container;

        [SerializeField] private string _exitButtonText = "See ya!";
        [SerializeField] private ChoiceButton _choiceButtonPrefab;

        private List<ChoiceButton> _buttons;

        public void DisplayChoices(Dictionary<string, DialogueSequence> choices)
        {
            _buttons = new List<ChoiceButton>();

            foreach (string key in choices.Keys)
            {
                ChoiceButton choiceButton = _container.InstantiatePrefabForComponent<ChoiceButton>(_choiceButtonPrefab, transform);
                _buttons.Add(choiceButton);
                choiceButton.ChoiceText = key;
                choiceButton.ChoiceSequence = choices[key];
                choiceButton.Button.onClick.AddListener(DestroyButtons);
            }

            if (choices == _choiceHubDialogueDisplay.MainChoices)
            {
                ChoiceButton exitButton = _container.InstantiatePrefabForComponent<ChoiceButton>(_choiceButtonPrefab, transform);
                _buttons.Add(exitButton);
                exitButton.ChoiceText = _exitButtonText;
                exitButton.Button.onClick.AddListener(DestroyButtons);
                exitButton.Button.onClick.AddListener(_choiceHubDialogueDisplay.EndDialogueOnClick);
            }
        }

        private void DestroyButtons()
        {
            foreach(ChoiceButton button in _buttons)
            {
                Destroy(button.gameObject);
            }
        }
    }
}
