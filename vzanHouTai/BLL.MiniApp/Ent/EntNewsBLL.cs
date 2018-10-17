using DAL.Base;
using Entity.MiniApp.Ent;
using System;

namespace BLL.MiniApp.Ent
{
    public class EntNewsBLL : BaseMySql<EntNews>
    {
        #region 单例模式
        private static EntNewsBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntNewsBLL()
        {

        }

        public static EntNewsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntNewsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public const string key = "entnews_{0}";
        public override EntNews GetModel(int Id)
        {
            EntNews model = RedisUtil.Get<EntNews>(string.Format(key, Id));
            if (model == null)
            {
                model = base.GetModel(Id);
                if (model != null)
                {
                    RedisUtil.Set(string.Format(key, Id), model, TimeSpan.FromHours(12));
                }
            }
            return base.GetModel(Id);
        }

        public override bool Update(EntNews model)
        {
            bool result = base.Update(model);
            if (result)
            {
                RemoveCache(model);
            }
            return result;
        }

        public override bool Update(EntNews model, string columnFields)
        {
            bool result = base.Update(model, columnFields);
            if (result)
            {
                RemoveCache(model);
            }
            return result;
        }

        public void RemoveCache(EntNews model)
        {
            RedisUtil.Remove(string.Format(key, model.id));
        }
    }
}
