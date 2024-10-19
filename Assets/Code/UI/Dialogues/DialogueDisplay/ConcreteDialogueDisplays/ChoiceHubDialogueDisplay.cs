using System.Collections.Generic;
using UnityEngine;

namespace Game.Dialogues
{
    public class ChoiceHubDialogueDisplay : DialogueDisplayBase
    {
        public Dictionary<string, DialogueSequence> MainChoices { get; private set; }

        [SerializeField] private ChoicesDisplay _choicesDisplay;

        public void DisplayChosenSequenceOnClick(DialogueSequence dialogueSequence)
        {
            CurrentSequence = dialogueSequence;
            _currentLineIndex = 0;
            CurrentLine = CurrentSequence.DialogueLines[_currentLineIndex];
            StartCoroutine(DisplayCurrentDialogueLine());
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

            //enable if automatically show choices on last line
            /*
            if (_currentLineIndex == CurrentSequence.DialogueLines.Count - 1)
            {
                ManageDisplayingChoices();
            }
            */
        }

        protected override void ManageDisplayingNextLine()
        {
            if (_currentLineIndex + 1 <= CurrentSequence.DialogueLines.Count - 1)
            {
                DisplayNextLine();
            }
            //enable if show choices on last line after one more skip
            ///*
            else
            {
                ManageDisplayingChoices();
            }
            //*/
        }

        private void ManageDisplayingChoices()
        {
            if (MainChoices == null)
            {
                MainChoices = CurrentSequence.Choices;
                _onDialogueEnd += () => { MainChoices = null; };
                _choicesDisplay.DisplayChoices(MainChoices);
            }
            else if (CurrentSequence.SequenceType == DialogueSequenceType.HubSequence)
            {
                _choicesDisplay.DisplayChoices(MainChoices);
            }
            else if (CurrentSequence.SequenceType == DialogueSequenceType.ChoiceHubSequence)
            {
                _choicesDisplay.DisplayChoices(CurrentSequence.Choices);
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
