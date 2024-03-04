using Lvc.Singleton;
using UnityEngine.Events;


namespace Lvc.MonoModel
{
    public sealed class MonoMgr : SingleMono<MonoMgr>
    {
        private UnityAction _update;
        private UnityAction _fixupdate;
        private UnityAction _lateupdate;
        private bool _openUpdate = true;

        private void Update()
        {
            if (!_openUpdate)
                return;

            _update?.Invoke();
        }

        private void FixedUpdate()
        {
            if (!_openUpdate)
                return;
            _fixupdate?.Invoke();
        }

        private void LateUpdate()
        {
            if (!_openUpdate)
                return;
            _lateupdate?.Invoke();
        }

        public void Clear()
        {
            _update = null;
            _fixupdate = null;
            _lateupdate = null;
        }

        public void SetUpdate(bool isOn)
        {
            _openUpdate = isOn;
        }

        public void AddUpdateEvent(UnityAction updateEvent)
        {
            _update += updateEvent;
        }

        public void RemoveUpdateEvent(UnityAction updateEvent)
        {
            _update -= updateEvent;
        }

        public void AddFixUpdateEvent(UnityAction fixupdateEvent)
        {
            _fixupdate += fixupdateEvent;
        }

        public void RemoveFixUpdateEvent(UnityAction fixupdateEvent)
        {
            _fixupdate -= fixupdateEvent;
        }

        public void AddLateUpdateEvent(UnityAction lateupdateEvent)
        {
            _lateupdate += lateupdateEvent;
        }

        public void RemoveLateUpdateEvent(UnityAction lateupdateEvent)
        {
            _lateupdate -= lateupdateEvent;
        }
    }
}