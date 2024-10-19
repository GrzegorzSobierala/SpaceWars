using System.Collections;
using UnityEngine;

namespace Game.Dialogues
{
    public class BackgroundMissionDialogueDisplay : DialogueDisplayBase
    {
        [Space]
        [Header("Display time parameters")]
        [SerializeField] private float _backupTime = 3f;
        [SerializeField] private float _charsPerSecond = 15f;

        protected override IEnumerator DisplayCurrentDialogueLine()
        {
            SetupDisplays();

            yield return new WaitForSeconds(LineDisplayLength(CurrentLine));

            ManageDisplayingNextLine();
        }

        private float LineDisplayLength(DialogueLine dialogueLine)
        {
            float lineDisplayLength = _backupTime;
            int countChars = dialogueLine.LineText.Replace(" ", null).Length;
            lineDisplayLength += countChars / _charsPerSecond;

            return lineDisplayLength;
        }
    }
}