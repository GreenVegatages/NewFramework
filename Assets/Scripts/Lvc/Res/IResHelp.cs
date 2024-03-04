using System.Collections;
using System.Collections.Generic;
using Lvc.EventModel;
using Lvc.MonoModel;
using UnityEngine;
using UnityEngine.Events;

namespace Lvc.ResModel.Help
{
    public interface IResHelp : IRes
    {
        private class ResLoad
        {
            public static IEnumerator ReallyLoad<T>(string resPath, string eventName) where T : UnityEngine.Object
            {
                ResourceRequest rq = Resources.LoadAsync<T>(resPath);
                yield return rq;
                var obj = rq.asset;
                if (obj != null)
                {
                    EventCenter.Instance.EventTrigger<T>(eventName, obj as T);
                    EventCenter.Instance.Remove(eventName);
                }
            }
        }

        protected class ResHelp
        {
            private Dictionary<string, IBase> _resDic = new Dictionary<string, IBase>();

            public T Load<T>(string resPath) where T : UnityEngine.Object
            {
                string path = $"{resPath}_{typeof(T)}";
                if (_resDic.TryGetValue(path, out IBase resInfo))
                {
                    return (resInfo as ResInfo<T>)?.Res;
                }

                T res = Resources.Load<T>(resPath);
                _resDic.Add(path, new ResInfo<T>(res));
                return res;
            }

            public void LoadAsync<T>(string resPath, UnityAction<T> callback) where T : UnityEngine.Object
            {
                string path = $"{resPath}_{typeof(T)}";
                if (_resDic.TryGetValue(path, out IBase resInfo))
                {
                    var info = resInfo as ResInfo<T>;
                    if (info?.Res != null)
                    {
                        callback?.Invoke(info.Res);
                    }
                    else
                    {
                        EventCenter.Instance.AddListener<T>(path, callback);
                    }
                }
                else
                {
                    _resDic.Add(path, new ResInfo<T>(path));
                    EventCenter.Instance.AddListener<T>(path, callback);
                    MonoMgr.Instance.StartCoroutine(IResHelp.ResLoad.ReallyLoad<T>(resPath, path));
                }
            }

            public void Unload<T>(string resPath) where T : UnityEngine.Object
            {
                string path = $"{resPath}_{typeof(T)}";
                if (_resDic.TryGetValue(path, out IBase resInfo))
                {
                    Resources.UnloadAsset((resInfo as ResInfo<T>)?.Res);
                    _resDic.Remove(path);
                }
            }

            public void UnloadAll()
            {
                _resDic.Clear();
                Resources.UnloadUnusedAssets();
            }
        }
    }
}