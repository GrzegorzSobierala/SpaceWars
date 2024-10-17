using System.Collections.Generic;
using UnityEngine;

namespace Game.Dialogues
{
    public class ChoiceHubDialogueDisplay : DialogueDisplayBase
    {
        public Dictionary<string, DialogueSequence> MainChoices { get; private set; }

        [SerializeField] private ChoicesDisplay _choicesDisplay;

        public void DisplayChoiceSequence(DialogueSequence dialogueSequence)
        {
            CurrentSequence = dialogueSequence;
            _currentLineIndex = 0;
            StartCoroutine(DisplayCurrentDialogueLine());
        }

        public void EndDialogue()
        {
            ClearCurrentFields();
            gameObject.SetActive(false);
            _onDialogueEnd?.Invoke();
        }

        protected override void EndSequence()
        {
            if (MainChoices == null)
            {
                MainChoices = CurrentSequence.Choices;
                _onDialogueEnd += () => { MainChoices = null; };
                _choicesDisplay.DisplayOptions(MainChoices);
            }
            else if (CurrentSequence.SequenceType == DialogueSequenceType.HubSequence)
            {
                _choicesDisplay.DisplayOptions(MainChoices);
            }
            else if (CurrentSequence.SequenceType == DialogueSequenceType.ChoiceHubSequence)
            {
                _choicesDisplay.DisplayOptions(CurrentSequence.Choices);
            }
            else
            {
                Debug.LogWarning("One of the choices is not a ChoiceHubSequence nor HubSequence.");
                EndDialogue();
            }
        }
    }
}
