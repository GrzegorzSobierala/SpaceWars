using Game.Dialogues;
using Game.Input.System;
using NaughtyAttributes;
using System;
using UnityEngine;
using Zenject;

namespace Game
{
    public class TestingDialogues : MonoBehaviour
    {
        [SerializeField] private DialogueDisplayManager _dialogueDisplayManager;
        [Header("Sequences")]
        [SerializeField] private DialogueSequence _hubSequence;
        [SerializeField] private DialogueSequence _choiceSequence;
        [SerializeField] private DialogueSequence _blockSequence;
        [SerializeField] private DialogueSequence _backgroundSequence;
        [Inject] private InputProvider _inputProvider;
        private Action _onDialogueEnd;

        [Button]
        private void HubDialogueTest()
        {
            _onDialogueEnd = null;
            _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Dialogues);
            _onDialogueEnd += () => { _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Gameplay); };
            _dialogueDisplayManager.StartDialogue(_hubSequence, _onDialogueEnd);
        }
        [Button]
        private void ChoiceDialogueTest()
        {
            _onDialogueEnd = null;
            _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Dialogues);
            _onDialogueEnd += () => { _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Gameplay); };
            _dialogueDisplayManager.StartDialogue(_choiceSequence, _onDialogueEnd);
        }
        [Button]
        private void BlockDialogueTest()
        {
            _onDialogueEnd = null;
            _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Dialogues);
            _onDialogueEnd += () => { _inputProvider.SwitchActionMap(_inputProvider.PlayerControls.Gameplay); };
            _dialogueDisplayManager.StartDialogue(_blockSequence, _onDialogueEnd);
        }
        [Button]
        private void BackgroundDialogueTest()
        {
            _onDialogueEnd = null;
            _dialogueDisplayManager.StartDialogue(_backgroundSequence, _onDialogueEnd);
        }
    }
}
