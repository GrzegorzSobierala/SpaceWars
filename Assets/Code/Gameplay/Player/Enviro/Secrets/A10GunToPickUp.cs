using Game.Player.Ship;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game.Room
{
    public class A10GunToPickUp : MonoBehaviour
    {
        [SerializeField] GunModuleBase A10GunModulePrefab;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.attachedRigidbody == null) 
                return;

            if (!collision.attachedRigidbody.TryGetComponent(out ModuleCreator playerModuleCreator))
                return;

            playerModuleCreator.ReplaceGun(A10GunModulePrefab);
            Destroy(gameObject);
        }
    }
}
