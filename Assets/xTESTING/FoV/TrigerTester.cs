using UnityEngine;

namespace Game
{
    public class TrigerTester : MonoBehaviour
    {

        public string message;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log(message);
        }
    }
}
