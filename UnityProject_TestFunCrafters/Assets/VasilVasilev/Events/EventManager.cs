using System;
using System.Collections.Generic;
using UnityEngine;
using VasilVasilev.Core;

namespace VasilVasilev.Events
{
    public class EventManager : Singleton<EventManager>
    {

        private Dictionary<EventType, Delegate> eventDictionary = new Dictionary<EventType, Delegate>();

        public void AddListener(EventType eventKey, Action listener)
        {
            if (eventDictionary.TryGetValue(eventKey, out Delegate existingDelegate))
            {
                eventDictionary[eventKey] = Delegate.Combine(existingDelegate, listener);
            }
            else
            {
                eventDictionary[eventKey] = listener;
            }
        }

        public void AddListener<T>(EventType eventKey, Action<T> listener)
        {
            if (eventDictionary.TryGetValue(eventKey, out Delegate existingDelegate))
            {
                eventDictionary[eventKey] = Delegate.Combine(existingDelegate, listener);
            }
            else
            {
                eventDictionary[eventKey] = listener;
            }
        }
        
        public void RemoveListener(EventType eventKey, Action listener)
        {
            if (eventDictionary.TryGetValue(eventKey, out Delegate existingDelegate))
            {
                existingDelegate = Delegate.Remove(existingDelegate, listener);
                if (existingDelegate == null) eventDictionary.Remove(eventKey);
                else eventDictionary[eventKey] = existingDelegate;
            }
        }

        public void RemoveListener<T>(EventType eventKey, Action<T> listener)
        {
            if (eventDictionary.TryGetValue(eventKey, out Delegate existingDelegate))
            {
                existingDelegate = Delegate.Remove(existingDelegate, listener);
                if (existingDelegate == null) eventDictionary.Remove(eventKey);
                else eventDictionary[eventKey] = existingDelegate;
            }
        }
        
        public void TriggerEvent(EventType eventKey)
        {
            if (eventDictionary.TryGetValue(eventKey, out Delegate existingDelegate))
            {
                (existingDelegate as Action)?.Invoke();
            }
            else
            {
                Debug.LogWarning($"Trigger event {eventKey} not found");
            }
        }

        public void TriggerEvent<T>(EventType eventKey, T param)
        {
            if (eventDictionary.TryGetValue(eventKey, out Delegate existingDelegate))
            {
                (existingDelegate as Action<T>)?.Invoke(param);
            }
            else
            {
                Debug.LogWarning($"Trigger event {eventKey} not found");
            }
        }
        
    }
}
