using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Player.Ui
{
    public class TutorialUi : MonoBehaviour
    {
        [SerializeField] private GameObject _panelsParent;
        [SerializeField] private Button _previousPageButton;
        [SerializeField] private Button _nextPageButton;

        private int currentPage = 0;

        private List<GameObject> panles = new List<GameObject>();

        private void Awake()
        {
            foreach (Transform child in _panelsParent.transform)
            {
                panles.Add(child.gameObject);
            }
        }

        private void Start()
        {
            _nextPageButton.onClick.AddListener(NextPage);
            _previousPageButton.onClick.AddListener(PreviousPage);
            UpdatePageButtonsInteractable();
        }

        private void OnDestroy()
        {
            _nextPageButton.onClick.RemoveListener(NextPage);
            _previousPageButton.onClick.RemoveListener(PreviousPage);
        }

        private void PreviousPage()
        {
            if (currentPage > 0)
            {
                panles[currentPage].SetActive(false);
                currentPage--;
                panles[currentPage].SetActive(true);
            }

            UpdatePageButtonsInteractable();
        }

        private void NextPage()
        {
            if (currentPage < panles.Count - 1)
            {
                panles[currentPage].SetActive(false);
                currentPage++;
                panles[currentPage].SetActive(true);
            }

            UpdatePageButtonsInteractable();
        }

        
        private void UpdatePageButtonsInteractable()
        {
            if (currentPage > 0)
            {
                _previousPageButton.interactable = true;
            }
            else
            {
                _previousPageButton.interactable = false;
            }

            if (currentPage < panles.Count - 1)
            {
                _nextPageButton.interactable = true;
            }
            else
            {
                _nextPageButton.interactable = false;
            }
        }
    }
}
