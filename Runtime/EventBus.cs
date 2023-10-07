using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ETdoFresh.UnityPackages.EventBusSystem
{
    public class EventBus : MonoBehaviourLazyLoadedSingleton<EventBus>
    {
        [SerializeField] private List<EventBusEntry> listeners = new();
        private static Dictionary<int, Dictionary<Type, List<EventBusEntry>>> listenersDictionary = new();

        public static void AddListener<T>(UnityAction<T> action, int channel = 0) where T : EventBusEvent
        {
            var type = typeof(T);
            var actionUnityObject = GetGameObject(action);
            var methodName = $"{action.Method.DeclaringType?.Name}.{action.Method.Name}";
            var messageName = actionUnityObject
                ? $"{type.Name}[{channel}] >> {actionUnityObject.name} {methodName}"
                : $"{type.Name}[{channel}] >> {methodName}";
            var listener = new EventBusEntry<T>
            {
                name = messageName,
                channel = channel,
                type = typeof(T),
                unityObject = actionUnityObject,
                action = action,
            };

            if (!listenersDictionary.ContainsKey(channel))
                listenersDictionary.Add(channel, new Dictionary<Type, List<EventBusEntry>>());
            
            if (!listenersDictionary[channel].ContainsKey(type))
                listenersDictionary[channel].Add(type, new List<EventBusEntry>());

            listenersDictionary[channel][type].Add(listener);

#if UNITY_EDITOR
            Instance.listeners.Add(listener);
            Instance.listeners.Sort((x, y) => string.Compare(x.name, y.name, StringComparison.Ordinal));
            listener.script = GetMonoScript(action);
#endif
        }

        public static void RemoveListener<T>(UnityAction<T> action, int channel = 0) where T : EventBusEvent
        {
            var type = typeof(T);
            if (!listenersDictionary.ContainsKey(channel)) return;
            if (!listenersDictionary[channel].ContainsKey(type)) return;

            var entry = listenersDictionary[channel][type]
                .Find(x => x is EventBusEntry<T> y && y.action == action);
            if (entry == null) return;

            listenersDictionary[channel][type].Remove(entry);
#if UNITY_EDITOR
            if (Instance) Instance.listeners.Remove(entry);
#endif
        }

        public static void Invoke<T>(T message, int channel = 0) where T : EventBusEvent
        {
            var type = typeof(T);
            if (!listenersDictionary.ContainsKey(channel)) return;

            var entries = listenersDictionary[channel][type];
            for (var i = 0; i < entries.Count; i++)
            {
                try
                {
                    if (entries[i] is EventBusEntry<T> listener)
                    {
                        listener.action.Invoke(message);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private static GameObject GetGameObject<T>(UnityAction<T> action) where T : EventBusEvent
        {
            return action.Target switch
            {
                GameObject gameObject => gameObject,
                Component component => component.gameObject,
                _ => null
            };
        }

#if UNITY_EDITOR
        private static UnityEditor.MonoScript GetMonoScript<T>(UnityAction<T> action) where T : EventBusEvent
        {
            return action.Target switch
            {
                GameObject gameObject => UnityEditor.MonoScript.FromMonoBehaviour(
                    gameObject.GetComponent(action.Method.DeclaringType) as MonoBehaviour),
                Component component => UnityEditor.MonoScript.FromMonoBehaviour(component as MonoBehaviour),
                _ => null
            };
        }
#endif
    }
}