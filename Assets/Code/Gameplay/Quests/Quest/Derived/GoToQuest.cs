using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Objectives
{
    public class GoToQuest : Quest
    {
        protected override void OnStartQuest()
        {
            Debug.LogError("No failure implemented");
        }

        protected override void OnSuccess()
        {
            Debug.LogError("No failure implemented");
        }

        protected override void OnFailure()
        {
            Debug.LogError("No failure implemented");
        }

        protected override void OnEnd()
        {
            Debug.LogError("No failure implemented");
        }
    }
}
