using Lvc.EventModel;
using UnityEngine;

namespace Lvc.ResModel.Help
{
    public interface IRes
    {
        protected class ResInfo<T> : IBase where T : UnityEngine.Object
        {
            public T Res;

            public ResInfo(T res)
            {
                Res = res;
            }


            public ResInfo(string eventName)
            {
                EventCenter.Instance.AddListener<T>(eventName, (res) => { Res = res; });
            }
        }
    }
}