using UnityEditor;
using UnityEngine;

namespace Game.Dialogues
{
    [CustomEditor(typeof(DialogueLine))]
    public class DialogueLineEditor : UnityEditor.Editor
    {
        #region SerializedProperties
        SerializedProperty LineType;

        SerializedProperty CharacterType;
        SerializedProperty OverrideCharacterName;
        SerializedProperty CharacterSprite;

        SerializedProperty VoiceEventRef;
        SerializedProperty SoundEventRef;
        SerializedProperty SoundStartTime;

        SerializedProperty LineText;
        #endregion

        private void OnEnable()
        {
            LineType = serializedObject.FindProperty(nameof(DialogueLine.LineType));

            CharacterType = serializedObject.FindProperty(nameof(DialogueLine.CharacterType));
            OverrideCharacterName = serializedObject.FindProperty(nameof(DialogueLine.OverrideCharacterName));
            CharacterSprite = serializedObject.FindProperty(nameof(DialogueLine.CharacterSprite));

            VoiceEventRef = serializedObject.FindProperty(nameof(DialogueLine.VoiceEventRef));
            SoundEventRef = serializedObject.FindProperty(nameof(DialogueLine.SoundEventRef));
            SoundStartTime = serializedObject.FindProperty(nameof(DialogueLine.SoundStartTime));

            LineText = serializedObject.FindProperty(nameof(DialogueLine.LineText));
        }

        DialogueLine dialogueLine;
        GUIStyle textAreaStyle;
        GUIStyle titleStyle;
        private float _spaceSizeBig = 30f;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            dialogueLine = (DialogueLine)target;

            SetupStyleOptions();
            DisplayTypeField();


            if (dialogueLine.LineType == DialogueLineType.DescriptionLine)
            {
                DisplayAudioFields(displayVoiceField: false);
                DisplayTextArea();
            }
            else
            {
                DisplayCharacterFields();
                DisplayAudioFields(displayVoiceField: true);
                DisplayTextArea();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void SetupStyleOptions()
        {
            EditorGUIUtility.labelWidth = 150;

            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            textAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };
        }

        private void DisplayTypeField()
        {
            GUILayout.Label("Type", titleStyle);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(LineType);
        }

        private void DisplayCharacterFields()
        {
            EditorGUILayout.Space(_spaceSizeBig);
            GUILayout.Label("Character", titleStyle);
            EditorGUILayout.Space();
            if (dialogueLine.CharacterSprite == null)
            {
                EditorGUILayout.PropertyField(CharacterType);
                EditorGUILayout.PropertyField(OverrideCharacterName);
                EditorGUILayout.PropertyField(CharacterSprite);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.PropertyField(CharacterType);
                EditorGUILayout.PropertyField(OverrideCharacterName);
                EditorGUILayout.PropertyField(CharacterSprite);
                EditorGUILayout.EndVertical();
                GUILayout.Box(dialogueLine.CharacterSprite.texture, GUILayout.Width(150), GUILayout.Height(200));
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DisplayAudioFields(bool displayVoiceField)
        {
            EditorGUILayout.Space(_spaceSizeBig);
            GUILayout.Label("Audio", titleStyle);
            EditorGUILayout.Space();

            if (displayVoiceField)
            {
                EditorGUILayout.PropertyField(VoiceEventRef);
            }

            EditorGUILayout.PropertyField(SoundEventRef);
            EditorGUILayout.PropertyField(SoundStartTime);
        }

        private void DisplayTextArea()
        {
            EditorGUILayout.Space(_spaceSizeBig);
            GUILayout.Label("Text", titleStyle);
            EditorGUILayout.Space();
            dialogueLine.LineText = EditorGUILayout.TextArea(dialogueLine.LineText, textAreaStyle);
        }
    }
}
