using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Dialogues
{
    public class CharacterSpriteDisplay : MonoBehaviour
    {
        [Inject] private BlockMissionDialogueDisplay _blockMissionDialogueDisplay;

        [SerializeField] private Image _characterLeftSprite;
        [SerializeField] private Image _characterRightSprite;
        [SerializeField] private Image _nonShipCharacterSprite;

        private DialogueLine CurrentLine => _blockMissionDialogueDisplay.CurrentLine;

        public void DisplayCharacterSprite()
        {
            if (CurrentLine.LineType == DialogueLineType.RegularCharacterLine)
            {
                if (CurrentLine.CharacterType == CharacterType.MainCharacter
                    && CurrentLine.OverrideCharacterName == "")
                {
                    _characterRightSprite.gameObject.SetActive(true);
                    _characterRightSprite.sprite = CurrentLine.CharacterSprite;
                }
                else
                {
                    _characterLeftSprite.gameObject.SetActive(true);
                    _characterLeftSprite.sprite = CurrentLine.CharacterSprite;
                }
            }
            else if (CurrentLine.LineType == DialogueLineType.NonShipCharacterLine)
            {
                _nonShipCharacterSprite.gameObject.SetActive(true);
                _nonShipCharacterSprite.sprite = CurrentLine.CharacterSprite;
            }
        }

        public void ClearDisplay()
        {
            _characterLeftSprite.gameObject.SetActive(false);
            _characterRightSprite.gameObject.SetActive(false);
            _nonShipCharacterSprite.gameObject.SetActive(false);
        }
    }
}
