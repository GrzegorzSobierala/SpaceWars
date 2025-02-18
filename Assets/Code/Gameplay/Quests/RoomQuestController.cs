using Game.Management;
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

        [SerializeField] private List<Quest> _startQuests;

        private readonly List<Quest> _allQuests = new(); 
        private readonly List<Quest> _mainQuests = new();
        private readonly List<Quest> _sideQuests = new();

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            CreateQuests();

            foreach (Quest quest in _startQuests)
            {
                StartQuest(quest);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.IsGameQuitungOrSceneUnloading(gameObject))
            {
                return;
            }

            _uiController.ClearAllQuests();
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

                _allQuests.Add(quest);

                if(quest.IsMain)
                {
                    _mainQuests.Add(quest);
                }
                else
                {
                    _sideQuests.Add(quest);
                }

                quest.InitByQuestController();
            }
        }

        private void CreateQuests()
        {
            _uiController.CreateQuests(_allQuests);
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
                    StartQuest(nextQuest);
                }
            }
            else
            {
                foreach (var nextQuest in quest.NextQuestsOnFailure)
                {
                    StartQuest(nextQuest);
                }
            }

            foreach (var nextQuest in quest.NextQuestsOnEnd)
            {
                StartQuest(nextQuest);
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
