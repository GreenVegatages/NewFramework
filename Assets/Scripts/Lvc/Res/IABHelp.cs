using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Lvc.EventModel;
using Lvc.MonoModel;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Lvc.ResModel.Help
{
    public interface IABHelp : IRes
    {
        private class ABLoad
        {
            public static IEnumerator ReallyLoadDependencies(string path, string abName)
            {
                var ab = AssetBundle.LoadFromFileAsync(Path.Combine(path, abName));
                yield return ab;
                EventCenter.Instance.EventTrigger(abName, ab.assetBundle);
            }

            public static IEnumerator LoadRes(AssetBundle assetBundle, string resName, Type resType, string eventName)
            {
                var acr = assetBundle.LoadAssetAsync(resName, resType);
                yield return acr;
                if (acr != null)
                {
                    //执行资源加载完成的事件
                    EventCenter.Instance.EventTrigger(eventName, acr.asset);
                    //清空未加载时的所有事件
                    EventCenter.Instance.Remove(eventName);
                }
            }
        }

        private class ABInfo
        {
            public AssetBundle _ab { get; private set; }
            private Dictionary<string, IBase> _resDic = new Dictionary<string, IBase>();

            public ABInfo(string eventName)
            {
                //监听ab包的加载,加载完成设置AB包
                EventCenter.Instance.AddListener<AssetBundle>(eventName, (ab) => { _ab = ab; });
            }

            public ABInfo(string url, string abName)
            {
                _ab = AssetBundle.LoadFromFile(Path.Combine(url, abName));
            }

            #region 异步

            private IEnumerator Wait(string resName, Type resType, UnityAction<Object> action)
            {
                while (_ab == null)
                {
                    yield return null;
                }

                Execute(resName, resType, action);
            }

            public void Execute(string resName, Type resType, UnityAction<Object> action)
            {
                if (_ab == null)
                {
                    MonoMgr.Instance.StartCoroutine(Wait(resName, resType, action));
                    return;
                }

                string key = StrCombine(resName, resType);
                string eventName = EventStrCombine(key);
                //判断是不是已经加载过的资源
                if (_resDic.TryGetValue(key, out var value))
                {
                    var obj = (value as ResInfo<Object>)?.Res;
                    if (obj != null)
                    {
                        action?.Invoke(obj); //资源加载完成,执行回调
                    }
                    else
                    {
                        //资源未加载完成,存储回调
                        EventCenter.Instance.AddListener(eventName, action);
                    }
                }
                else
                {
                    //存入资源字典,避免重复加载
                    _resDic[key] = new ResInfo<Object>(eventName);
                    //存储回调
                    EventCenter.Instance.AddListener(eventName, action);
                    //开始加载资源
                    MonoMgr.Instance.StartCoroutine(ABLoad.LoadRes(_ab, resName, resType, eventName));
                }
            }

            #endregion

            #region 同步

            public void Unload()
            {
                _ab.Unload(false);
                _resDic.Clear();
                _ab = null;
                _resDic = null;
            }

            public Object GetRes(string resName, Type resType)
            {
                //得到字典 的键
                string combine = StrCombine(resName, resType);
                if (_resDic.TryGetValue(combine, out IBase Res) && (Res is ResInfo<Object> info))
                {
                    return info.Res;
                }

                string eventName = EventStrCombine(combine);
                //没有就去加载包中的资源
                Object res = _ab.LoadAsset(resName, resType);
                //添加进资源字典
                _resDic.Add(combine, new ResInfo<Object>(res));
                EventCenter.Instance.EventTrigger(eventName, res);
                //清空未加载时的所有事件
                EventCenter.Instance.Remove(eventName);

                return res;
            }

            #endregion

            private string StrCombine(string resName, Type resType) => $"{resName}_{resType.Name}";
            private string EventStrCombine(string str) => $"{_ab.name}_{str}";
        }

        protected class ABHelp
        {
            private Dictionary<string, ABInfo> _abDic = new Dictionary<string, ABInfo>();

            private AssetBundleManifest _manifest;

            private AssetBundle _mainAb;

            private string PathUrl
            {
                get { return Application.streamingAssetsPath; }
            }

            private string MainABName
            {
                get
                {
#if UNITY_IOS
      return "IOS";
#elif UNITY_ANDROID
        return "Android";
#else
                    return "PC";
#endif
                }
            }

            #region 同步

            private void LoadMainAb()
            {
                if (_mainAb == null)
                {
                    _mainAb = AssetBundle.LoadFromFile(Path.Combine(PathUrl, MainABName));
                    _manifest = _mainAb.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                }
            }

            private ABInfo LoadAb(string abName)
            {
                LoadMainAb();
                ABInfo ab = new ABInfo(PathUrl, abName);
                _abDic.TryAdd(abName, ab);
                string[] infos = _manifest.GetAllDependencies(abName);
                foreach (var info in infos)
                {
                    if (_abDic.ContainsKey(info))
                        continue;
                    _abDic.Add(info, new ABInfo(PathUrl, info));
                }

                return ab;
            }

            #region 加载资源

            /// <summary>
            /// 通过泛型加载资源
            /// </summary>
            /// <param name="abName"></param>
            /// <param name="resName"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public T Load<T>(string abName, string resName) where T : UnityEngine.Object
            {
                return Load(abName, resName, typeof(T)) as T;
            }

            /// <summary>
            /// 通过Type加载资源
            /// </summary>
            /// <param name="abName"></param>
            /// <param name="resName"></param>
            /// <param name="resType"></param>
            /// <returns></returns>
            public Object Load(string abName, string resName, Type resType)
            {
                abName = abName.ToLower();
                if (_abDic.TryGetValue(abName, out var abInfo))
                {
                    return abInfo.GetRes(resName, resType);
                }

                return LoadAb(abName).GetRes(resName, resType);
            }

            #endregion
            
            /// <summary>
            /// 卸载单个资源
            /// </summary>
            /// <param name="abName"></param>
            public void Unload(string abName)
            {
                abName = abName.ToLower();
                if (_abDic.TryGetValue(abName, out var abInfo))
                {
                    abInfo.Unload();
                    _abDic.Remove(abName);
                }
            }

            /// <summary>
            /// 卸载所有资源
            /// </summary>
            public void UnloadAll()
            {
                AssetBundle.UnloadAllAssetBundles(false);
                _abDic.Clear();
                _mainAb = null;
                _manifest = null;
            }

            #endregion

            #region 异步

            private bool IsLoading; //判断主包是否正在加载

            public void LoadAsync<T>(string abName, string resName, UnityAction<T> callBack) where T : Object
            {
                // 将类型转换为 UnityAction<Object>
                UnityAction<Object> objectCallBack = obj => callBack(obj as T);
                // 调用另一个重载方法
                LoadAsync(abName, resName, typeof(T), objectCallBack);
            }

            public void LoadAsync(string abName, string resName, Type resType, UnityAction<Object> callBack)
            {
                abName = abName.ToLower();
                //加载主包
                if (_mainAb == null && !IsLoading)
                {
                    MonoMgr.Instance.StartCoroutine(LoadABInfo());
                }

                //没有该AB包就去加载
                if (!_abDic.ContainsKey(abName))
                {
                    _abDic[abName] = new ABInfo(abName);
                    MonoMgr.Instance.StartCoroutine(LoadAB(abName));
                }

                //加载AB包中的资源
                _abDic[abName].Execute(resName, resType, callBack);
            }

            /// <summary>
            /// 异步加载主包和依赖信息
            /// </summary>
            /// <returns></returns>
            IEnumerator LoadABInfo()
            {
                IsLoading = true;
                var ab = AssetBundle.LoadFromFileAsync(Path.Combine(PathUrl, MainABName));
                yield return ab;
                _mainAb = ab.assetBundle;
                var bc = ab.assetBundle.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest");
                yield return bc;
                _manifest = bc.asset as AssetBundleManifest;
                IsLoading = false;
            }

            /// <summary>
            /// 加载AB包
            /// </summary>
            /// <param name="abName"></param>
            /// <returns></returns>
            IEnumerator LoadAB(string abName)
            {
                //等待主包和依赖信息的加载
                while (IsLoading)
                {
                    yield return null;
                }

                //加载依赖包
                LoadDependencies(abName);
                var ab = AssetBundle.LoadFromFileAsync(Path.Combine(PathUrl, abName));
                yield return ab;
                //触发ab加载完成的事件
                EventCenter.Instance.EventTrigger(abName, ab.assetBundle);
            }

            /// <summary>
            /// 加载依赖信息
            /// </summary>
            /// <param name="abName"></param>
            private void LoadDependencies(string abName)
            {
                string[] strs = _manifest.GetAllDependencies(abName);
                foreach (var ab in strs)
                {
                    if (_abDic.ContainsKey(ab))
                    {
                        continue;
                    }

                    _abDic[ab] = new ABInfo(ab);
                    MonoMgr.Instance.StartCoroutine(ABLoad.ReallyLoadDependencies(PathUrl, ab));
                }
            }

            #endregion
        }
    }
}