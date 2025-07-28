using UnityEngine;

namespace Game.Ui
{
    public class ButtonMethods : MonoBehaviour
    {
        public void ToogleGameObject(GameObject go)
        {
            go.SetActive(!go.activeSelf);
        }

        public void ExitGame()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
