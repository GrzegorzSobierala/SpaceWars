using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Dialogues
{
    public class ChoiceButton : MonoBehaviour
    {
        [Inject] private ChoiceHubDialogueDisplay _choiceHubDialogueDisplay;

        [field: SerializeField] public Button Button { get; set; }

        public string ChoiceText { get; set; }
        public DialogueSequence ChoiceSequence { get; set; }

        [SerializeField] private TextMeshProUGUI TMP;

        private void Start()
        {
            TMP.text = ChoiceText;

            if (ChoiceSequence != null)
            {
                Button.onClick.AddListener(() => { _choiceHubDialogueDisplay.DisplayChosenSequenceOnClick(ChoiceSequence); });
            }
        }
    }
}
