using Game.Input.System;
using System;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Game.Dialogues
{
    public abstract class DialogueDisplayBase : MonoBehaviour
    {
        [Inject] protected InputProvider _inputProvider;

        [field: SerializeField] public DialogueSequenceType SequenceType { get; private set; }
        public DialogueSequence CurrentSequence { get; protected set; }
        public DialogueLine CurrentLine { get; protected set; }

        [SerializeField] protected CharacterSpriteDisplay _characterSpriteDisplay;
        [SerializeField] protected DialogueTextDisplay _dialogueTextDisplay;

        protected int _currentLineIndex;
        protected Action _onDialogueEnd;

        protected PlayerControls.DialoguesActions Input => _inputProvider.PlayerControls.Dialogues;

        public void DisplayDialogue(DialogueSequence dialogueSequence, Action onDialogueEnd)
        {
            CurrentSequence = dialogueSequence;
            _currentLineIndex = 0;
            CurrentLine = CurrentSequence.DialogueLines[_currentLineIndex];
            _onDialogueEnd += () => { onDialogueEnd?.Invoke(); _onDialogueEnd = null; };

            gameObject.SetActive(true);

            StartCoroutine(DisplayCurrentDialogueLine());
        }

        protected virtual IEnumerator DisplayCurrentDialogueLine()
        {
            SetupDisplays();

            while (!Input.Skip.WasPerformedThisFrame())
            {
                yield return null;
            }
            yield return null;

            ManageDisplayingNextLine();
        }


        protected virtual void SetupDisplays()
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

        protected virtual void ClearDisplays()
        {
            _characterSpriteDisplay.ClearDisplay();
            _dialogueTextDisplay.ClearDisplay();
        }

        protected virtual void ManageDisplayingNextLine()
        {
            if (_currentLineIndex + 1 <= CurrentSequence.DialogueLines.Count - 1)
            {
                DisplayNextLine();
            }
            else
            {
                EndDialogue();
            }
        }

        protected void DisplayNextLine()
        {
            _currentLineIndex++;
            CurrentLine = CurrentSequence.DialogueLines[_currentLineIndex];
            StartCoroutine(DisplayCurrentDialogueLine());
        }

        protected virtual void EndDialogue()
        {
            ClearCurrentFields();
            gameObject.SetActive(false);
            _onDialogueEnd?.Invoke();
        }

        protected void ClearCurrentFields()
        {
            CurrentSequence = null;
            CurrentLine = null;
            _currentLineIndex = 0;
        }
    }
}
