using UnityEngine;

namespace Lvc.Singleton
{
    [DisallowMultipleComponent]
    public abstract class SingleMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    //创建对象并添加单例
                    GameObject obj = new GameObject($"{typeof(T).Name} Manager");
                    _instance = obj.AddComponent<T>();
                    //过场景不移除
                    GameObject.DontDestroyOnLoad(obj);
                }

                return _instance;
            }
        }
    }
}