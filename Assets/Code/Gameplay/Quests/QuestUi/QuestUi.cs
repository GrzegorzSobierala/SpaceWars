using TMPro;
using UnityEngine;

namespace Game
{
    public class QuestUi : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [Space]
        [SerializeField] private Color _successColor;
        [SerializeField] private Color _failureColor;
        [SerializeField] private Color _activeColor;
        [SerializeField] private Color _futureColor;

        public QuestUi Instantiate(Transform parent)
        {
            QuestUi instance = Instantiate(this, parent);
            return instance;
        }

        public void SetSuccess()
        {
            gameObject.SetActive(true);
            _text.color = _successColor;
            _text.fontStyle = FontStyles.Strikethrough;
        }

        public void SetFailure()
        {
            gameObject.SetActive(true);
            _text.color = _failureColor;
            _text.fontStyle = FontStyles.Strikethrough;
        }

        public void SetActive() 
        {
            gameObject.SetActive(true);
            _text.color = _activeColor;
            _text.fontStyle = FontStyles.Normal;
        }

        public void SetFuture()
        {
            gameObject.SetActive(true);
            _text.color = _futureColor;
            _text.fontStyle = FontStyles.Normal;
        }

        public void SetInvisible()
        {
            gameObject.SetActive(false);
        }

        public void SetNameText(string name)
        {
            _text.name = name + "UiQuest";
            _text.text = name;
        }

        public void DestroyQuest()
        {
            if(gameObject)
            {
                Destroy(gameObject);
            }
        }
    }
}
