using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player.Ui
{
    public class PlayerUiController : MonoBehaviour
    {
        public void SetActive(bool active)
        {
            if (active == gameObject.activeSelf)
                return;

            if(active)
            {
                Enable();
            }
            else
            {
                Disable();
            }
        }

        private void Enable()
        {
            gameObject.SetActive(true);
        }

        private void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}
