using System.Collections.Generic;
using Lvc.Singleton;
using UnityEngine;
using UnityEngine.Events;

public interface IBase
{
}

namespace Lvc.EventModel
{
    public sealed class EventCenter : Single<EventCenter>, IEventHelp
    {
        private EventCenter()
        {
        }

        private Dictionary<string, IBase> _eventDic = new Dictionary<string, IBase>();


        public bool HaveEvent(string eventName)
        {
            return _eventDic.ContainsKey(eventName);
        }

        public void AddListener(string eventName, UnityAction callback)
        {
            if (_eventDic.TryGetValue(eventName, out IBase value))
            {
                ((value as IEventHelp.EventInfo)!).Actions += callback;
            }
            else
            {
                _eventDic.Add(eventName, new IEventHelp.EventInfo(callback));
            }
        }

        public void AddListener<T>(string eventName, UnityAction<T> callback)
        {
            if (_eventDic.TryGetValue(eventName, out IBase value))
            {
                ((value as IEventHelp.EventInfo<T>)!).Actions += callback;
            }
            else
            {
                _eventDic.Add(eventName, new IEventHelp.EventInfo<T>(callback));
            }
        }

        public void RemoveListener(string eventName, UnityAction callback)
        {
            if (_eventDic.TryGetValue(eventName, out IBase value))
            {
                ((value as IEventHelp.EventInfo)!).Actions -= callback;
            }
        }

        public void RemoveListener<T>(string eventName, UnityAction<T> callback)
        {
            if (_eventDic.TryGetValue(eventName, out IBase value))
            {
                ((value as IEventHelp.EventInfo<T>)!).Actions -= callback;
            }
        }


        public void EventTrigger(string eventName)
        {
            if (_eventDic.TryGetValue(eventName, out IBase value))
            {
                (value as IEventHelp.EventInfo)?.Execute();
            }
            else
            {
                UnityEngine.Debug.LogError($"{eventName} -- 该事件不存在");
            }
        }

        public void EventTrigger<T>(string eventName, T value)
        {
            if (_eventDic.TryGetValue(eventName, out IBase eventInfo))
            {
                (eventInfo as IEventHelp.EventInfo<T>)?.Execute(value);
            }
            else
            {
                UnityEngine.Debug.LogError($"{eventName} -- 该事件不存在");
            }
        }

        public void Clear()
        {
            _eventDic.Clear();
        }

        public void Remove(string eventName)
        {
            if (_eventDic.TryGetValue(eventName, out var value))
            {
                _eventDic.Remove(eventName);
            }
        }
    }
}