using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Dialogues
{
    public class ChoiceHubDialogueDisplay : DialogueDisplayBase
    {
        public Dictionary<string, DialogueSequence> MainChoices { get; private set; }

        [SerializeField] private ChoicesDisplay _choicesDisplay;

        private Dictionary<string, DialogueSequence> _currentChoices;
        private Stack<Dictionary<string, DialogueSequence>> _choicesStack;

        public void DisplayChosenSequenceOnClick(DialogueSequence dialogueSequence)
        {
            _choicesStack.Push(_currentChoices);
            CurrentSequence = dialogueSequence;
            _currentLineIndex = 0;
            CurrentLine = CurrentSequence.DialogueLines[_currentLineIndex];
            StartCoroutine(DisplayCurrentDialogueLine());
        }

        public void DisplayPreviousChoicesOnClick()
        {
            _currentChoices = _choicesStack.Pop();
            _choicesDisplay.DisplayChoices(_currentChoices);
        }

        public void EndDialogueOnClick()
        {
            EndDialogue();
        }

        protected override void SetupDisplays()
        {
            ClearDisplays();

            if (CurrentLine.LineType != DialogueLineType.DescriptionLine)
            {
                _characterSpriteDisplay.DisplayCharacterSprite();
                _dialogueTextDisplay.DisplayCharacterName();
                _dialogueTextDisplay.DisplayCharacterLineText();
            }
            else
            {
                _dialogueTextDisplay.DisplayDescriptionLineText();
            }
        }

        protected override void ManageDisplayingNextLine()
        {
            if (_currentLineIndex + 1 <= CurrentSequence.DialogueLines.Count - 1)
            {
                DisplayNextLine();
            }
            else
            {
                ManageDisplayingChoices();
            }
        }

        protected override void ClearCurrentFields()
        {
            CurrentSequence = null;
            CurrentLine = null;
            _currentLineIndex = 0;

            MainChoices = null;
            _currentChoices = null;
        }

        private void ManageDisplayingChoices()
        {
            if (MainChoices == null)
            {
                MainChoices = CurrentSequence.Choices;
                _choicesStack = new();
            }

            if (CurrentSequence.SequenceType == DialogueSequenceType.ChoiceHubSequence)
            {
                _currentChoices = CurrentSequence.Choices;
                _choicesDisplay.DisplayChoices(_currentChoices);
            }
            else if (CurrentSequence.SequenceType == DialogueSequenceType.HubSequence)
            {
                _choicesDisplay.DisplayChoices(_choicesStack.Pop());
            }
            else
            {
                Debug.LogError("Current sequence was not ChoiceHubSequence nor HubSequence. " +
                    "Subsequences in ChoiceHubSequence should only be on of these two." +
                    "Dialogue ended because of this error.");
                EndDialogue();
            }
        }
    }
}
