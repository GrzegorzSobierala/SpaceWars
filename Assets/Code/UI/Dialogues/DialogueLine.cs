using FMODUnity;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player.Ui
{
    [CreateAssetMenu(fileName = "DialogueLine", menuName = "Dialogues/DialogueLine")]
    public class DialogueLine : ScriptableObject
    {
        [BoxGroup("Type")] 
        public DialogueLineType LineType;

        [BoxGroup("Character")] [HideIf("TypeIsDescription")] 
        public CharacterType CharacterType;
        [BoxGroup("Character")] [HideIf("TypeIsDescription")] [Tooltip("Not used if blank.")] 
        public string OverrideCharacterName;
        [BoxGroup("Character")] [HideIf("TypeIsDescription")] [ShowAssetPreview] 
        public Sprite CharacterSprite;

        [BoxGroup("Text")] [TextArea(1, 20)] 
        public string LineText;

        [BoxGroup("Audio")] [HideIf("TypeIsDescription")] 
        public EventReference VoiceEventRef;
        [BoxGroup("Audio")] [Tooltip("Not used if null.")] 
        public EventReference SoundEventRef;
        [BoxGroup("Audio")] 
        public float SoundStartTime;

        private bool TypeIsDescription()
        {
            return LineType == DialogueLineType.DescriptionLine;
        }
    }
}
