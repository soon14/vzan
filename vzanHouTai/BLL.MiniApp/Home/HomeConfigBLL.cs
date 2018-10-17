using DAL.Base;
using Entity.MiniApp.Home;
using System;

namespace BLL.MiniApp.Home
{
    public class HomeConfigBLL : BaseMySql<HomeConfig>
    {

        #region 单例模式
        private static HomeConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private HomeConfigBLL()
        {

        }

        public static HomeConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new HomeConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion


        string key = "HomeConfig_{0}";
        public HomeConfig GetModelByCache(int id)
        {
            HomeConfig model = RedisUtil.Get<HomeConfig>(string.Format(key,id));
            if (model == null)
            {
                model = this.GetModel(id);
                if (model != null)
                {
                    RedisUtil.Set<HomeConfig>(key, model, TimeSpan.FromHours(1));
                }
            }
            return model;
        }
        public bool UpdateModel(HomeConfig model, string fields)
        {
            if (base.Update(model, fields))
            {
                RemoveCache(model);
                return true;
            }
            return false;
        }

        public bool UpdateModel(HomeConfig model)
        {
            if (base.Update(model))
            {
                RemoveCache(model);
                return true;
            }
            return false;
        }
        public void RemoveCache(HomeConfig model)
        {
            RedisUtil.Remove(string.Format(key, model.Id));
        }
    }
}
