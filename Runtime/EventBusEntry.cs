using System;
using UnityEngine.Events;

namespace ETdoFresh.UnityPackages.EventBusSystem
{
    [Serializable]
    internal class EventBusEntry
    {
        public string name;
        public int channel;
        
#if UNITY_EDITOR
        public UnityEditor.MonoScript script;
#endif
        
        public Type type;
        public UnityEngine.Object unityObject;
    }
    
    [Serializable]
    internal class EventBusEntry<T> : EventBusEntry where T : EventBusEvent
    {
        public UnityAction<T> action;
    }
}