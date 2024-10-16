using AYellowpaper.SerializedCollections;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Dialogues
{
    [CreateAssetMenu(fileName = "DialogueSequence", menuName = "Dialogues/DialogueSequence")]
    public class DialogueSequence : ScriptableObject
    {
        [BoxGroup("Type")]
        public DialogueSequenceType SequenceType;

        [BoxGroup("Lines in this sequence"), Expandable, ReorderableList] 
        public List<DialogueLine> DialogueLines;

        [BoxGroup("Options after this sequence"), ShowIf(nameof(_typeIsOptions)), 
        SerializedDictionary("Option text", "Option sequence")]
        public SerializedDictionary<string, DialogueSequence> Options;

        private bool _typeIsOptions => SequenceType == DialogueSequenceType.OptionsSequence;
    }
}
