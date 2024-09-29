using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game
{
    public class QuestUi : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [Space]
        [SerializeField] private Color successColor;
        [SerializeField] private Color failureColor;
        [SerializeField] private Color activeColor;
        [SerializeField] private Color futureColor;

        public QuestUi Instantiate(Transform parent)
        {
            QuestUi instance = Instantiate(this, parent);
            return instance;
        }

        public void SetSuccess()
        {
            text.color = successColor;
            text.fontStyle = FontStyles.Strikethrough;
        }

        public void SetFailure()
        {
            text.color = failureColor;
            text.fontStyle = FontStyles.Strikethrough;
        }

        public void SetActive() 
        {
            text.color = activeColor;
            text.fontStyle = FontStyles.Normal;
        }

        public void SetFuture()
        {
            text.color = futureColor;
            text.fontStyle = FontStyles.Normal;
        }
    }
}
