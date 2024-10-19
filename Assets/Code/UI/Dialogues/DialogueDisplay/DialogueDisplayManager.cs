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

                        dialogueDisplay.DisplayDialogue(dialogueSequence, onDialogueEnd);

                        return;
                    }
                }

                Debug.LogError("No display implemented for " + dialogueSequence.SequenceType.ToString() + " type.");
            }
            else
            {
                Debug.LogError("Trying to display dialogue while other is displayed.");
            }
        }



        //-----------------------------------------Testing
        [HorizontalLine(color: EColor.Gray)]
        [Header("Testing")]
        [SerializeField] private DialogueSequence _hubSequence;
        [SerializeField] private DialogueSequence _choiceSequence;
        [SerializeField] private DialogueSequence _blockSequence;
        [SerializeField] private DialogueSequence _backgroundSequence;
        [Inject] private InputProvider _inputProvider;
        private Action _onDialogueEnd;

        [Button]
        private void HubDialogueTest()
        {
            _onDialogueEnd = null;
            _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Dialogues);
            _onDialogueEnd += () => { _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Gameplay);  };
            StartDialogue(_hubSequence, _onDialogueEnd);
        }
        [Button]
        private void ChoiceDialogueTest()
        {
            _onDialogueEnd = null;
            _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Dialogues);
            _onDialogueEnd += () => { _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Gameplay); };
            StartDialogue(_choiceSequence, _onDialogueEnd);
        }
        [Button]
        private void BlockDialogueTest()
        {
            _onDialogueEnd = null;
            _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Dialogues);
            _onDialogueEnd += () => { _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Gameplay); };
            StartDialogue(_blockSequence, _onDialogueEnd);
        }
        [Button]
        private void BackgroundDialogueTest()
        {
            _onDialogueEnd = null;
            StartDialogue(_backgroundSequence, _onDialogueEnd);
        }
    }
}
