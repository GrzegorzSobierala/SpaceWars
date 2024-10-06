using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Objectives
{
    public class PlayerQuestsUiController : MonoBehaviour
    {
        [SerializeField] private QuestUi _questUiPrefab;
        [SerializeField] private Transform _groupTransform;
        [SerializeField] private Transform _mainQuestsTextTransform;
        [SerializeField] private Transform _sideQuestsTextTransform;

        private Dictionary<Quest, QuestUi> _questAndUiDict = new();

        public Dictionary<Quest, QuestUi> QuestAndUiDict => _questAndUiDict;

        public void CreateQuests(ICollection<Quest> quests)
        {
            foreach (Quest quest in quests)
            {
                CreateQuest(quest);
            }
        }

        public void CreateQuest(Quest quest)
        {
            QuestUi instance = _questUiPrefab.Instantiate(_groupTransform);

            if(quest.IsMain)
            {
                int index = _sideQuestsTextTransform.GetSiblingIndex();
                instance.transform.SetSiblingIndex(index);
            }
            else
            {
                int index = _groupTransform.childCount - 1;
                instance.transform.SetSiblingIndex(index);
            }

            quest.OnStartQuestEvent += instance.SetActive;
            quest.OnSuccessEvent += instance.SetSuccess;
            quest.OnFailureEvent += instance.SetFailure;

            instance.SetNameText(quest.Name);

            _questAndUiDict.Add(quest, instance);
        }
    }
}
