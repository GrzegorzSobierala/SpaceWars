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

        [BoxGroup("Choices after this sequence"), ShowIf(nameof(_typeIsChoice)), 
        SerializedDictionary("Choice text", "Choice sequence")]
        public SerializedDictionary<string, DialogueSequence> Choices;

        private bool _typeIsChoice => SequenceType == DialogueSequenceType.ChoiceHubSequence;
    }
}
