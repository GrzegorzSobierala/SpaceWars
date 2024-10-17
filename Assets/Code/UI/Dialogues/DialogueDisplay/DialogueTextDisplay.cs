using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Dialogues
{
    public class DialogueTextDisplay : MonoBehaviour
    {
        [Inject] private DialogueDisplayBase _dialogueDisplay;

        [SerializeField] private TextMeshProUGUI _characterNameTMP;
        [SerializeField] private TextMeshProUGUI _characterLineTMP;
        [SerializeField] private TextMeshProUGUI _descriptionTMP;

        private DialogueLine _currentLine => _dialogueDisplay.CurrentLine;

        public void DisplayCharacterName()
        {
            _characterNameTMP.gameObject.SetActive(true);
            if (_currentLine.OverrideCharacterName == "")
            {
                _characterNameTMP.text = _currentLine.CharacterType.ToString();
            }
            else
            {
                _characterNameTMP.text = _currentLine.OverrideCharacterName;
            }
        }

        public void DisplayCharacterLineText()
        {
            _characterLineTMP.gameObject.SetActive(true);
            _characterLineTMP.text = _currentLine.LineText;
        }

        public void DisplayDescriptionLineText()
        {
            _descriptionTMP.gameObject.SetActive(true);
            _descriptionTMP.text = _currentLine.LineText;
        }

        public void ClearDisplay()
        {
            _characterNameTMP.gameObject.SetActive(false);
            _characterLineTMP.gameObject.SetActive(false);
            _descriptionTMP.gameObject.SetActive(false);
        }
    }
}
