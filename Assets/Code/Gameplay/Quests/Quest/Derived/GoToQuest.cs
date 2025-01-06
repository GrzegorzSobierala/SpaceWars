using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Objectives
{
    public class GoToQuest : Quest
    {
        [Space]
        [SerializeField] GoToTarget _goToTarget;

        protected override void OnStartQuest()
        {
            _goToTarget.OnPlayerReachedTarget += Success;
            _goToTarget.gameObject.SetActive(true);
        }

        protected override void OnSuccess()
        {
        }

        protected override void OnFailure()
        {
            Debug.LogError("No failure implemented");
        }

        protected override void OnEnd()
        {
            _goToTarget.OnPlayerReachedTarget -= Success;
            _goToTarget.gameObject.SetActive(false);
        }
    }
}
