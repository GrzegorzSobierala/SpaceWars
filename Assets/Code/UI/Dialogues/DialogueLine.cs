using FMODUnity;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Dialogues
{
    [CreateAssetMenu(fileName = "DialogueLine", menuName = "Dialogues/DialogueLine")]
    public class DialogueLine : ScriptableObject
    {
        [HideInInspector] public DialogueLineType LineType;

        [ShowIf(nameof(OverrideEmptyAndTypeNotDesc)), ReadOnly, Label("Character")]
        public CharacterType CharacterType;
        [ShowIf(nameof(OverrideNotEmptyAndTypeNotDesc)), ReadOnly, Label("Character"), Tooltip("Not used if blank.")]
        public string OverrideCharacterName;
        [HideInInspector] public Sprite CharacterSprite;

        [HideInInspector] public EventReference VoiceEventRef;
        [HideInInspector, Tooltip("Not used if null.")] public EventReference SoundEventRef;
        [HideInInspector] public float SoundStartTime;

        [TextArea(1, 20), Label("")]
        public string LineText;

        private bool OverrideIsEmpty => (OverrideCharacterName == "");
        private bool TypeIsDescription => (LineType == DialogueLineType.DescriptionLine);
        private bool OverrideEmptyAndTypeNotDesc => (OverrideIsEmpty && !TypeIsDescription);
        private bool OverrideNotEmptyAndTypeNotDesc => (!OverrideIsEmpty && !TypeIsDescription);
    }
}