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
    }
}
