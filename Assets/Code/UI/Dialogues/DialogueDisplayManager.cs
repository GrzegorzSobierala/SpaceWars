using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Dialogues
{
    public class DialogueDisplayManager : MonoBehaviour
    {
        [SerializeField] private List<DialogueDisplayBase> _displayControllers;

        private bool _dialogueIsDisplayed = false;

        public void StartDialogue(DialogueSequence dialogueSequence, Action onDialogueEnd)
        {
            if (!_dialogueIsDisplayed)
            {
                foreach (DialogueDisplayBase displayController in _displayControllers)
                {
                    if (dialogueSequence.SequenceType == displayController.SequenceType)
                    {
                        _dialogueIsDisplayed = true;
                        onDialogueEnd += () => { _dialogueIsDisplayed = false; };
                        StartCoroutine(displayController.DisplaySequence(dialogueSequence, onDialogueEnd));
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
