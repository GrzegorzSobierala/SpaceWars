using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

namespace Game.Audio
{
    public class FmodEvents : MonoBehaviour
    {
        [field: Header("SFX")]
        [field: SerializeField] public EventReference Alarm { get; private set; }

        [field: Header("Music")]
        [field: SerializeField] public EventReference Music { get; private set; }
    }
}
