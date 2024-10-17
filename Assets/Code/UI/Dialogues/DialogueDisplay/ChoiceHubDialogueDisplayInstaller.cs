using Game.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Dialogues
{
    public class ChoiceHubDialogueDisplayInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Utils.BindGetComponent<DialogueDisplayBase>(Container, gameObject);
            Utils.BindGetComponent<ChoiceHubDialogueDisplay>(Container, gameObject);
        }
    }
}
