using Game.Input.System;
using System;
using System.Collections;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace Game.Dialogues
{
    public class BlockMissionDialogueDisplay : DialogueDisplayBase
    {
        public DialogueSequence CurrentSequence { get; private set; }
        public DialogueLine CurrentLine { get; private set; }
        public int CurrentLineIndex { get; private set; }

        [Inject] private InputProvider _inputProvider;

        [SerializeField] private CharacterSpriteDisplay _characterSpriteDisplay;
        [SerializeField] private DialogueTextDisplay _dialogueTextDisplay;

        private PlayerControls.GameplayActions Input => _inputProvider.PlayerControls.Gameplay;
        private InputAction Skip => Input.SwitchGun;

        public override void DisplaySequence(DialogueSequence dialogueSequence, Action onDialogueEnd)
        {
            CurrentSequence = dialogueSequence;
            CurrentLineIndex = 0;
            gameObject.SetActive(true);

            StartCoroutine(DisplayCurrentDialogueLine(onDialogueEnd));
        }

        private IEnumerator DisplayCurrentDialogueLine(Action onDialogueEnd)
        {
            ClearDisplay();

            CurrentLine = CurrentSequence.DialogueLines[CurrentLineIndex];

            SetupDisplays();

            yield return null;
            while (!Skip.WasPerformedThisFrame())
            {
                yield return null;
            }
            yield return null;

            if (CurrentLineIndex <= CurrentSequence.DialogueLines.Count - 2)
            {
                CurrentLineIndex++;
                StartCoroutine(DisplayCurrentDialogueLine(onDialogueEnd));
            }
            else
            {
                ClearCurrentFields();
                gameObject.SetActive(false);
                onDialogueEnd?.Invoke();
            }
        }

        private void SetupDisplays()
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

        private void ClearDisplay()
        {
            _characterSpriteDisplay.ClearDisplay();
            _dialogueTextDisplay.ClearDisplay();
        }

        private void ClearCurrentFields()
        {
            CurrentSequence = null;
            CurrentLine = null;
            CurrentLineIndex = 0;
        }
    }
}

