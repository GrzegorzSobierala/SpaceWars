using UnityEngine;

namespace Game.Ui
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] private Vector3 rotateSpeed;

        private void Update()
        {
            transform.Rotate(rotateSpeed * Time.deltaTime);
        }
    }
}
