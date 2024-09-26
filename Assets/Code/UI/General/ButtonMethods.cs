using UnityEngine;

namespace Game.Ui
{
    public class ButtonMethods : MonoBehaviour
    {
        public void ToogleGameObject(GameObject go)
        {
            go.SetActive(!go.activeSelf);
        }
    }
}
