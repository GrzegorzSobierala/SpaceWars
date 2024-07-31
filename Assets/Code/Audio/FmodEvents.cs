using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

namespace Game.Audio
{
    public class FmodEvents : MonoBehaviour
    {
        [field: Header("SFX")]
        [field: SerializeField] public EventReferenceScriptable Alarm { get; private set; }

        [field: Header("Music")]
        [field: SerializeField] public EventReferenceScriptable Music { get; private set; }
    }
}
