using Game.Input.System;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Dialogues
{
    public class DialogueDisplayManager : MonoBehaviour
    {
        [SerializeField] private List<DialogueDisplayBase> _dialogueDisplays;

        private bool _dialogueIsDisplayed = false;

        public void StartDialogue(DialogueSequence dialogueSequence, Action onDialogueEnd)
        {
            if (!_dialogueIsDisplayed)
            {
                foreach (DialogueDisplayBase dialogueDisplay in _dialogueDisplays)
                {
                    if (dialogueSequence.SequenceType == dialogueDisplay.SequenceType)
                    {
                        _dialogueIsDisplayed = true;
                        onDialogueEnd += () => { _dialogueIsDisplayed = false; };

                        dialogueDisplay.DisplaySequence(dialogueSequence, onDialogueEnd);

                        return;
                    }
                }

                Debug.LogWarning("No display implemented for " + dialogueSequence.SequenceType.ToString() + " type.");
            }
            else
            {
                Debug.LogWarning("Trying to display dialogue while other is displayed.");
            }
        }






        //TESTING------------------------------------------------------------------
        [Space(100)]
        [HorizontalLine(color: EColor.Gray)]
        [Header("Testing")]
        [SerializeField] private DialogueSequence _hubSequence;
        [SerializeField] private DialogueSequence _choiceSequence;
        [SerializeField] private DialogueSequence _blockSequence;
        [SerializeField] private DialogueSequence _backgroundSequence;
        [Inject] private InputProvider _inputProvider;
        private Action _onDialogueEnd;

        [Button]
        private void TestDialogueHub()
        {
            _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Dialogues);
            _onDialogueEnd = null;
            _onDialogueEnd += () => { _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Gameplay);  };
            _onDialogueEnd += () => { Debug.Log("Dialogue ended"); };
            StartDialogue(_hubSequence, _onDialogueEnd);
        }

        [Button]
        private void TestDialogueChoice()
        {
            _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Dialogues);
            _onDialogueEnd = null;
            _onDialogueEnd += () => { _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Gameplay); };
            _onDialogueEnd += () => { Debug.Log("Dialogue ended"); };
            StartDialogue(_choiceSequence, _onDialogueEnd);
        }

        [Button]
        private void TestDialogueBlock()
        {
            _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Dialogues);
            _onDialogueEnd = null;
            _onDialogueEnd += () => { _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Gameplay); };
            _onDialogueEnd += () => { Debug.Log("Dialogue ended"); };
            StartDialogue(_blockSequence, _onDialogueEnd);
        }

        [Button]
        private void TestDialogueBackground()
        {
            _onDialogueEnd = null;
            _onDialogueEnd += () => { Debug.Log("Dialogue ended"); };
            StartDialogue(_backgroundSequence, _onDialogueEnd);
        }
    }
}
