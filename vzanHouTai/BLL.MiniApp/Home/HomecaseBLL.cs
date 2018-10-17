using DAL.Base;
using Entity.MiniApp.Home;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Home
{
    public class HomecaseBLL : BaseMySql<Homecase>
    {
        #region 单例模式
        private static HomecaseBLL _singleModel;
        private static readonly object SynObject = new object();

        private HomecaseBLL()
        {

        }

        public static HomecaseBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new HomecaseBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        private string zbcasekey = "zbcaseListkey";
        private string tccasekey = "tccaseListkey";
        private string sqcasekey = "sqcaseListkey";
        private string yycasekey = "yycaseListkey";

        public List<Homecase> GetListByCache(int type, int count)
        {
            string key = string.Empty;
            string sqlwhere = $" state=1 and type={type}";
            if (type == (int)caseType.zbcase)
            {
                key = zbcasekey;
            }
            else if (type == (int)caseType.tccase)
            {
                key = tccasekey;
            }
            else if (type == (int)caseType.sqcase)
            {
                key = sqcasekey;
            }
            else
            {
                key = yycasekey;
            }
            List<Homecase> List = RedisUtil.Get<List<Homecase>>(key);
            if (List == null || List.Count == 0)
            {
                List = this.GetList(sqlwhere, count, 1, "*", "sort desc,id desc");
                if (List != null && List.Count > 0)
                {
                    RedisUtil.Set<List<Homecase>>(key, List, TimeSpan.FromHours(1));
                }
            }
            return List;
        }
        public bool UpdateModel(Homecase model)
        {
            if (base.Update(model))
            {
                string key = string.Empty;
                if (model.type == (int)caseType.zbcase)
                {
                    key = zbcasekey;
                }
                else if (model.type == (int)caseType.tccase)
                {
                    key = tccasekey;
                }
                else if (model.type == (int)caseType.sqcase)
                {
                    key = sqcasekey;
                }
                else
                {
                    key = yycasekey;
                }
                RedisUtil.Remove(key);
            }
            return false;
        }
    }
}
