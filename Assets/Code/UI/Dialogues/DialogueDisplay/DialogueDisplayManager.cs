using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

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
        [Space(200)]
        [HorizontalLine(color: EColor.Gray)]
        [Header("Testing")]
        [SerializeField] private DialogueSequence _sequenceForTesting;
        private Action _onDialogueEnd;
        [Button]
        private void TestDialogue()
        {
            _onDialogueEnd = null;
            _onDialogueEnd += () => { Debug.Log("Dialogue ended!"); };
            StartDialogue(_sequenceForTesting, _onDialogueEnd);
        }
    }
}
