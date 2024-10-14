using AYellowpaper.SerializedCollections;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player.Ui
{
    [CreateAssetMenu(fileName = "DialogueSequence", menuName = "Dialogues/DialogueSequence")]
    public class DialogueSequence : ScriptableObject
    {
        [BoxGroup("Type")]
        public DialogueSequenceType SequenceType;

        [BoxGroup("Lines in this sequence")] [Expandable] [ReorderableList] 
        public List<DialogueLine> DialogueLines;

        [BoxGroup("Options after this sequence")] [SerializedDictionary("Option text", "Option sequence")]
        [ShowIf("TypeIsOptions")] public SerializedDictionary<string, DialogueSequence> Options;

        private bool TypeIsOptions()
        {
            return SequenceType == DialogueSequenceType.OptionsSequence;
        }
    }
}
