using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Dialogues
{
    public class DialogueTextDisplay : MonoBehaviour
    {
        [Inject] private BlockMissionDialogueDisplay _blockMissionDialogueDisplay;

        [SerializeField] private TextMeshProUGUI _characterNameTMP;
        [SerializeField] private TextMeshProUGUI _characterLineTMP;
        [SerializeField] private TextMeshProUGUI _descriptionTMP;

        private DialogueLine CurrentLine => _blockMissionDialogueDisplay.CurrentLine;

        public void DisplayCharacterName()
        {
            _characterNameTMP.gameObject.SetActive(true);
            if (CurrentLine.OverrideCharacterName == "")
            {
                _characterNameTMP.text = CurrentLine.CharacterType.ToString();
            }
            else
            {
                _characterNameTMP.text = CurrentLine.OverrideCharacterName;
            }
        }

        public void DisplayCharacterLineText()
        {
            _characterLineTMP.gameObject.SetActive(true);
            _characterLineTMP.text = CurrentLine.LineText;
        }

        public void DisplayDescriptionLineText()
        {
            _descriptionTMP.gameObject.SetActive(true);
            _descriptionTMP.text = CurrentLine.LineText;
        }

        public void ClearDisplay()
        {
            _characterNameTMP.gameObject.SetActive(false);
            _characterLineTMP.gameObject.SetActive(false);
            _descriptionTMP.gameObject.SetActive(false);
        }
    }
}
