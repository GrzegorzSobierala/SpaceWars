using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Dialogues
{
    public class ChoiceButton : MonoBehaviour
    {
        [Inject] private ChoiceHubDialogueDisplay _choiceHubDialogueDisplay;

        public Button Button;

        [HideInInspector] public string ChoiceText;
        [HideInInspector] public DialogueSequence ChoiceSequence;

        [SerializeField] private TextMeshProUGUI TMP;

        private void Start()
        {
            TMP.text = ChoiceText;

            if (ChoiceSequence != null)
            {
                Button.onClick.AddListener(() => { 
                    _choiceHubDialogueDisplay.DisplayChosenSequenceOnClick(ChoiceSequence); });
            }
        }
    }
}
