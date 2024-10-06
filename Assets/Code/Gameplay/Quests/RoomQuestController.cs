using Game.Room;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Objectives
{
    public class RoomQuestController : MonoBehaviour
    {
        public event Action OnMainQuestEnds;

        [Inject] private PlayerQuestsUiController _uiController;
        [Inject] private PlayerSceneManager _testSceneManager;

        private List<Quest> allQuests = new(); 
        private List<Quest> mainQuests = new();
        private List<Quest> sideQuests = new();

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            CreateQuests();
            StartQuest(mainQuests[0]);
        }

        private void Init()
        {
            foreach (Quest quest in GetComponentsInChildren<Quest>()) 
            { 
                if(!quest)
                {
                    Debug.LogError("Quest is null", this);
                    continue;
                }

                allQuests.Add(quest);

                if(quest.IsMain)
                {
                    mainQuests.Add(quest);
                }
                else
                {
                    sideQuests.Add(quest);
                }
            }
        }

        private void CreateQuests()
        {
            _uiController.CreateQuests(allQuests);
        }

        private void StartQuest(Quest quest)
        {
            quest.OnEndEvent += (bool value) => StartNextQuests(value, quest);
            quest.StartQuest();
        }

        private void StartNextQuests(bool isSuccess, Quest quest)
        {
            if (isSuccess)
            {
                foreach (var nextQuest in quest.NextQuestsOnSuccess)
                {
                    nextQuest.StartQuest();
                }
            }
            else
            {
                foreach (var nextQuest in quest.NextQuestsOnFailure)
                {
                    nextQuest.StartQuest();
                }
            }

            foreach (var nextQuest in quest.NextQuestsOnEnd)
            {
                nextQuest.StartQuest();
            }

            if (quest.IsMain)
            {
                if (isSuccess && 
                    quest.NextQuestsOnSuccess.Length == 0 && quest.NextQuestsOnEnd.Length == 0)
                {
                    OnMainQuestEnds?.Invoke();
                    _testSceneManager.EndRoom();
                }
            }
        }
    }
}
