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

            yield return new WaitForSecondsRealtime(LineDisplayTime(CurrentLine));

            ManageDisplayingNextLine();
        }

        private float LineDisplayTime(DialogueLine dialogueLine)
        {
            float lineDisplayTime = 0;
            int countChars = dialogueLine.LineText.Replace(" ", null).Length;
            lineDisplayTime += _backupTime + (countChars / _charsPerSecond);

            return lineDisplayTime;
        }
    }
}