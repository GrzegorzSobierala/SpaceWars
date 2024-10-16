using FMODUnity;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Dialogues
{
    [CreateAssetMenu(fileName = "DialogueLine", menuName = "Dialogues/DialogueLine")]
    public class DialogueLine : ScriptableObject
    {
        [HideInInspector] public DialogueLineType LineType;

        [ShowIf(nameof(_overrideEmptyAndTypeNotDesc)), ReadOnly, Label("Character")]
        public CharacterType CharacterType;
        [ShowIf(nameof(_overrideNotEmptyAndTypeNotDesc)), ReadOnly, Label("Character"), Tooltip("Not used if blank.")]
        public string OverrideCharacterName;
        [HideInInspector] public Sprite CharacterSprite;

        [HideInInspector] public EventReference VoiceEventRef;
        [HideInInspector, Tooltip("Not used if null.")] public EventReference SoundEventRef;
        [HideInInspector] public float SoundStartTime;

        [TextArea(1, 20), Label("")]
        public string LineText;

        private bool _overrideIsEmpty => (OverrideCharacterName == "");
        private bool _typeIsDescription => (LineType == DialogueLineType.DescriptionLine);
        private bool _overrideEmptyAndTypeNotDesc => (_overrideIsEmpty && !_typeIsDescription);
        private bool _overrideNotEmptyAndTypeNotDesc => (!_overrideIsEmpty && !_typeIsDescription);

    }
}