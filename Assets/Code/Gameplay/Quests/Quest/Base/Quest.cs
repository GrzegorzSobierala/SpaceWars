using System;
using UnityEngine;

namespace Game.Objectives
{
    public abstract class Quest : MonoBehaviour
    {
        public event Action OnStartQuestEvent;
        public event Action OnSuccessEvent;
        public event Action OnFailureEvent;
        public event Action</*isSuccess*/ bool> OnEndEvent;

        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public bool IsMain { get; private set; } = true;
        [field: SerializeField] public Quest[] NextQuestsOnEnd { get; private set; }
        [field: SerializeField] public Quest[] NextQuestsOnSuccess { get; private set; }
        [field: SerializeField] public Quest[] NextQuestsOnFailure { get; private set; }

        public void StartQuest()
        {
            gameObject.SetActive(true);
            OnStartQuest();
            OnStartQuestEvent?.Invoke();
        }

        public void Success()
        {
            OnSuccess();
            OnSuccessEvent?.Invoke();
            End(true);
        }

        public void Failure()
        {
            OnFailure();
            OnFailureEvent?.Invoke();
            End(false);
        }

        private void End(bool isSuccess)
        {
            gameObject.SetActive(false);
            OnEnd();
            OnEndEvent?.Invoke(isSuccess);
        }

        protected abstract void OnStartQuest();
        protected abstract void OnSuccess();
        protected abstract void OnFailure();
        protected abstract void OnEnd();
    }
}
