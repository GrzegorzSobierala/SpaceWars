using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Tools
{
    public class AllCollidersComposeSetter : MonoBehaviour
    {
        [SerializeField] bool inThisGameObject = false;
        [SerializeField] bool inChildrens = true;
        
        private void OnValidate()
        {
            if (inThisGameObject)
            {
                foreach (var collider in GetComponents<Collider2D>())
                {
                    if(!collider.usedByComposite)
                        collider.usedByComposite = true;
                }
            }

            if (inChildrens)
            {
                foreach (var collider in GetComponentsInChildren<Collider2D>())
                {
                    if (!collider.usedByComposite)
                        collider.usedByComposite = true;
                }
            }
        }
    }
}
