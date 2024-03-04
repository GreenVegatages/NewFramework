using UnityEngine;
using UnityEngine.Events;

namespace Lvc.EventModel
{
    public interface IEventHelp
    {
        
        protected class EventInfo : IBase
        {
            public event UnityAction Actions;

            public EventInfo(UnityAction callback)
            {
                Actions = null;
                Actions += callback;
            }

            public void Execute()
            {
                Actions?.Invoke();
            }
        }

        protected class EventInfo<T> : IBase
        {
            public event UnityAction<T> Actions;

            public EventInfo(UnityAction<T> callback)
            {
                Actions = null;
                Actions += callback;
            }

            public void Execute(T value)
            {
                Actions?.Invoke(value);
            }
        }
        

    }
}