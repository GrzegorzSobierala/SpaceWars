using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Dialogues
{
    public abstract class DialogueDisplayBase : MonoBehaviour
    {
        [field: SerializeField] public DialogueSequenceType SequenceType { get; private set; }

        public abstract IEnumerator DisplaySequence(DialogueSequence dialogueSequence, Action onDialogueEnd);
    }
}
