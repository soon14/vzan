using DAL.Base;
using Entity.MiniApp.Home;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Home
{
    public class HomepicBLL : BaseMySql<Homepic>
    {
        #region 单例模式
        private static HomepicBLL _singleModel;
        private static readonly object SynObject = new object();

        private HomepicBLL()
        {

        }

        public static HomepicBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new HomepicBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        string keylist = "HomeImgListkey";
        public List<Homepic> GetListByCache(string v1, int v2, int v3, string v4, string v5)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 通过缓存获取首页图片
        /// </summary>
        /// <returns></returns>
        public List<Homepic> GetListByCache()
        {
            string key = string.Format(keylist);
            List<Homepic> List = RedisUtil.Get<List<Homepic>>(key);
            if (List == null || List.Count == 0)
            {
                List = this.GetList($" state=1", 100, 1, "*", "sort desc,id asc");
                if (List != null && List.Count > 0)
                {
                    RedisUtil.Set<List<Homepic>>(key, List, TimeSpan.FromHours(1));
                }
            }
            return List;
        }
    }
}
