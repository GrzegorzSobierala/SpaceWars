using UnityEngine;

    namespace Game
    {
    public class HideUI : MonoBehaviour
    {
        [SerializeField] private GameObject _uiRoot;

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F9))
            {
                _uiRoot.SetActive(!_uiRoot.activeSelf);
            }
        }

    }
}
