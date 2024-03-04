using System.Reflection;

namespace Lvc.Singleton
{
    //T 约束 没有 new ,不让类可以new
    public abstract class Single<T> where T : class
    {
        protected static readonly object lockObj = new object();
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    //防止多个线程同时访问
                    lock (lockObj)
                    {
                        //得到类的type
                        System.Type type = typeof(T);
                        //获取私有无参构造函数
                        var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
                            System.Type.EmptyTypes, null);

                        //私有无参构造函数不为空 同时 是 该类的构造函数 赋值
                        if (constructor?.Invoke(null) is T tmpInstance)
                        {
                            _instance = tmpInstance;
                        }
                        else
                        {
                            UnityEngine.Debug.LogError($"{type} -- 该类没有声明私有构造函数");
                        }
                    }
                }

                return _instance;
            }
        }
        
    }
}