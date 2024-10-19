using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Dialogues
{
    public class CharacterSpriteDisplay : MonoBehaviour
    {
        [Inject] private DialogueDisplayBase _dialogueDisplay;

        [SerializeField] private Image _characterLeftImage;
        [SerializeField] private Image _characterRightImage;
        [SerializeField] private Image _nonShipCharacterImage;

        private DialogueLine CurrentLine => _dialogueDisplay.CurrentLine;

        public void DisplayCharacterSprite()
        {
            if (CurrentLine.LineType == DialogueLineType.RegularCharacterLine)
            {
                if (CurrentLine.CharacterType == CharacterType.MainCharacter
                    && CurrentLine.OverrideCharacterName == "")
                {
                    _characterRightImage.gameObject.SetActive(true);
                    _characterRightImage.sprite = CurrentLine.CharacterSprite;
                }
                else
                {
                    _characterLeftImage.gameObject.SetActive(true);
                    _characterLeftImage.sprite = CurrentLine.CharacterSprite;
                }
            }
            else if (CurrentLine.LineType == DialogueLineType.NonShipCharacterLine)
            {
                _nonShipCharacterImage.gameObject.SetActive(true);
                _nonShipCharacterImage.sprite = CurrentLine.CharacterSprite;
            }
        }

        public void ClearDisplay()
        {
            _characterLeftImage.gameObject.SetActive(false);
            _characterRightImage.gameObject.SetActive(false);
            _nonShipCharacterImage.gameObject.SetActive(false);
        }
    }
}
