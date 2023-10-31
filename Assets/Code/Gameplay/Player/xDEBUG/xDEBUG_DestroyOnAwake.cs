using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class xDEBUG_DestroyOnAwake : MonoBehaviour
    {
        private void Awake()
        {
            Destroy(gameObject);
        }
    }
}
