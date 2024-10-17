using Game.Input.System;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Game.Dialogues
{
    public abstract class DialogueDisplayBase : MonoBehaviour
    {
        [Inject] protected InputProvider _inputProvider;

        [field: SerializeField] public DialogueSequenceType SequenceType { get; protected set; }
        public DialogueSequence CurrentSequence { get; protected set; }
        public DialogueLine CurrentLine { get; protected set; }

        [SerializeField] protected CharacterSpriteDisplay _characterSpriteDisplay;
        [SerializeField] protected DialogueTextDisplay _dialogueTextDisplay;

        protected int _currentLineIndex;
        protected Action _onDialogueEnd;

        protected PlayerControls.DialoguesActions Input => _inputProvider.PlayerControls.Dialogues;

        public void DisplaySequence(DialogueSequence dialogueSequence, Action onDialogueEnd)
        {
            CurrentSequence = dialogueSequence;
            _currentLineIndex = 0;
            _onDialogueEnd += () => { onDialogueEnd?.Invoke(); _onDialogueEnd = null; };

            gameObject.SetActive(true);

            StartCoroutine(DisplayCurrentDialogueLine());
        }

        protected virtual IEnumerator DisplayCurrentDialogueLine()
        {
            ClearDisplay();

            CurrentLine = CurrentSequence.DialogueLines[_currentLineIndex];

            SetupDisplays();

            yield return null;
            while (!Input.Skip.WasPerformedThisFrame())
            {
                yield return null;
            }
            yield return null;

            if (_currentLineIndex <= CurrentSequence.DialogueLines.Count - 2)
            {
                _currentLineIndex++;
                StartCoroutine(DisplayCurrentDialogueLine());
            }
            else
            {
                EndSequence();
            }
        }

        protected void SetupDisplays()
        {
            ClearDisplay();

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

        protected virtual void ClearDisplay()
        {
            _characterSpriteDisplay.ClearDisplay();
            _dialogueTextDisplay.ClearDisplay();
        }

        protected void ClearCurrentFields()
        {
            CurrentSequence = null;
            CurrentLine = null;
            _currentLineIndex = 0;
        }

        protected virtual void EndSequence()
        {
            ClearCurrentFields();
            gameObject.SetActive(false);
            _onDialogueEnd?.Invoke();
        }
    }
}
