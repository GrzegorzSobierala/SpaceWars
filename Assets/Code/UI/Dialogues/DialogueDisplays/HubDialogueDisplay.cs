using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Dialogues
{
    public class HubDialogueDisplay : DialogueDisplayBase
    {
        public override IEnumerator DisplaySequence(DialogueSequence dialogueSequence, Action onDialogueEnd)
        {
            yield return new WaitForSeconds(2f);
            onDialogueEnd?.Invoke();
        }
    }
}
