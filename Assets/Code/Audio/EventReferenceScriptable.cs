using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Audio
{
    [CreateAssetMenu(fileName = "EventReferenceScriptable", menuName = "EventReferenceScriptable")]
    public class EventReferenceScriptable : ScriptableObject
    {
        public EventReference EventReference;
    }
}
