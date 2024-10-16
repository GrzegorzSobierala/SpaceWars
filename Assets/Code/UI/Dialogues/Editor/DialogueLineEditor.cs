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

        private float _spaceSizeBig = 30f;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DialogueLine dialogueLine = (DialogueLine)target;
            EditorGUIUtility.labelWidth = 150;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };


            GUILayout.Label("Type", titleStyle);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(LineType);


            if (dialogueLine.LineType == DialogueLineType.DescriptionLine)
            {
                EditorGUILayout.Space(_spaceSizeBig);
                GUILayout.Label("Audio", titleStyle);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(SoundEventRef);
                EditorGUILayout.PropertyField(SoundStartTime);

                EditorGUILayout.Space(_spaceSizeBig);
                GUILayout.Label("Text", titleStyle);
                EditorGUILayout.Space();
                dialogueLine.LineText = EditorGUILayout.TextArea(dialogueLine.LineText, textAreaStyle);
            }
            else
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


                EditorGUILayout.Space(_spaceSizeBig);
                GUILayout.Label("Audio", titleStyle);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(VoiceEventRef);
                EditorGUILayout.PropertyField(SoundEventRef);
                EditorGUILayout.PropertyField(SoundStartTime);


                EditorGUILayout.Space(_spaceSizeBig);
                GUILayout.Label("Text", titleStyle);
                EditorGUILayout.Space();
                dialogueLine.LineText = EditorGUILayout.TextArea(dialogueLine.LineText, textAreaStyle);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
