using AYellowpaper;
using Game.Player.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Game.Objectives
{
    public class DestroyTargetsQuest : Quest
    {
        [Inject] private MissionPoinerUi missionPoinerUi;

        [Space]
        [SerializeField] private InterfaceReference<IDefeatedCallback>[] targets;

        Dictionary<IDefeatedCallback, Action> currentTargets = new();

        protected override void OnStartQuest()
        {
            Subscribe();
            missionPoinerUi.SetCurrentTarget(currentTargets.First().Key.MainTransform);
        }

        protected override void OnSuccess()
        {
            Debug.Log($"Success {Name}");
        }

        protected override void OnFailure()
        {
            Debug.LogError("No failure implemented");
        }

        protected override void OnEnd()
        {
            Unsubscribe();
        }

        private void StartAnim(Action onEnd)
        {
            onEnd.Invoke();
        }

        private void Subscribe()
        {
            foreach (var target in targets)
            {
                Action action = () => RemoveTarget(target.Value);
                currentTargets.Add(target.Value, action);
                target.Value.OnDefeated += action;
            }
        }

        private void Unsubscribe()
        {
            foreach(var target in currentTargets)
            {
                target.Key.OnDefeated -= target.Value;
            }
        }

        private void RemoveTarget(IDefeatedCallback removeTarget)
        {
            RemoveNullKeysFromDictionary();

            if (currentTargets.ContainsKey(removeTarget))
            {
                removeTarget.OnDefeated -= currentTargets[removeTarget];
                currentTargets.Remove(removeTarget);
            }

            if(currentTargets.Count == 0) 
            {
                Success();
            }
            else
            {
                missionPoinerUi.SetCurrentTarget(currentTargets.First().Key.MainTransform);
            }
        }

        private void RemoveNullKeysFromDictionary()
        {
            // Create a list to store keys that need to be removed
            List<IDefeatedCallback> keysToRemove = new List<IDefeatedCallback>();

            // Iterate over the dictionary
            foreach (var kvp in currentTargets)
            {
                IDefeatedCallback key = kvp.Key;

                // If the key is null (Unity destroyed object check)
                if (key == null || key.Equals(null))
                {
                    keysToRemove.Add(key); // Mark the key for removal
                }
            }

            // Remove all marked keys
            foreach (var key in keysToRemove)
            {
                currentTargets.Remove(key);
                Debug.LogError("Removed null key from dictionary.");
            }
        }
    }
}
