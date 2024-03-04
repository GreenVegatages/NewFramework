using Lvc.ResModel.Help;
using Lvc.Singleton;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Lvc.ResModel
{
    public class ResMgr : Single<ResMgr>, IResHelp, IABHelp
    {
        private ResMgr()
        {
        }

        private IResHelp.ResHelp _resHelp = new IResHelp.ResHelp();
        private IABHelp.ABHelp _abHelp = new IABHelp.ABHelp();

        public T Load_Res<T>(string resPath) where T : Object
        {
            return _resHelp.Load<T>(resPath);
        }

        public void LoadAsync_Res<T>(string resPath, UnityAction<T> callback) where T : Object
        {
            _resHelp.LoadAsync(resPath, callback);
        }

        public void Unload_Res<T>(string resPath) where T : Object
        {
            _resHelp.Unload<T>(resPath);
        }

        public void UnloadAll_Res()
        {
            _resHelp.UnloadAll();
        }

        public T Load_AB<T>(string abName, string resName) where T : Object
        {
            return _abHelp.Load<T>(abName, resName);
        }

        public Object Load_AB(string abName, string resName, System.Type resType)
        {
            return _abHelp.Load(abName, resName, resType);
        }

        public void LoadAsync_AB<T>(string abName, string resName, UnityAction<T> callback) where T : Object
        {
            _abHelp.LoadAsync<T>(abName, resName, callback);
        }

        public void LoadAsync_AB(string abName, string resName, System.Type resType, UnityAction<Object> callback)
        {
            _abHelp.LoadAsync(abName, resName, resType, callback);
        }

        public void Unload_AB(string abName)
        {
            _abHelp.Unload(abName);
        }

        public void UnloadAll_AB()
        {
            _abHelp.UnloadAll();
        }
    }
}