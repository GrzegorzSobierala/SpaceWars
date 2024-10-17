using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Dialogues
{
    public class ChoiceButton : MonoBehaviour
    {
        [Inject] private ChoiceHubDialogueDisplay _choiceHubDialogueDisplay;

        public Button Button;

        [HideInInspector] public string ChoiceText;
        [HideInInspector] public DialogueSequence ChoiceSequence;

        [SerializeField] private TextMeshProUGUI TMP;

        private void Start()
        {
            TMP.text = ChoiceText;

            if (ChoiceSequence != null)
            {
                Button.onClick.AddListener(DisplaySequence);
            }
            else
            {
                Button.onClick.AddListener(() => { Debug.LogWarning("Choice sequence not assigned."); });
            }
        }

        private void DisplaySequence()
        {
            Debug.Log(_choiceHubDialogueDisplay);
            _choiceHubDialogueDisplay.DisplayChoiceSequence(ChoiceSequence);
        }
    }
}
