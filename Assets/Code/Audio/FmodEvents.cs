using UnityEngine;
using FMODUnity;

namespace Game.Audio
{
    public class FMODEvents : MonoBehaviour
    {
        [field: Header("SFX")]
        [field: SerializeField] public EventReference Alarm { get; private set; }
        [field: SerializeField] public EventReference Rain { get; private set; }

        [field: Header("Music")]
        [field: SerializeField] public EventReference Music { get; private set; }
    }
}