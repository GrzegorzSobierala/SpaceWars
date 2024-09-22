using UnityEngine;

namespace Game.UI
{
    public class ButtonMethods : MonoBehaviour
    {
        public void ToogleGameObject(GameObject go)
        {
            go.SetActive(!go.activeSelf);
        }
    }
}
