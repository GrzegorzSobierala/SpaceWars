using System;
using System.Collections;
using UnityEngine;

namespace Game.Dialogues
{
    public abstract class DialogueDisplayBase : MonoBehaviour
    {
        [field: SerializeField] public DialogueSequenceType SequenceType { get; private set; }

        public abstract void DisplaySequence(DialogueSequence dialogueSequence, Action onDialogueEnd);
    }
}
