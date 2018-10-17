using DAL.Base;
using Entity.MiniApp.Home;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Home
{
    public class HomekeywordBLL : BaseMySql<Homekeyword>
    {

        #region 单例模式
        private static HomekeywordBLL _singleModel;
        private static readonly object SynObject = new object();

        private HomekeywordBLL()
        {

        }

        public static HomekeywordBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new HomekeywordBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private string key = "Homekeyword";
        public string GetKeyWord()
        {
            string result = string.Empty;
            List<Homekeyword> List = GetListBycache();
            if (List != null && List.Count > 0)
            {
                foreach(var keyword in List)
                {
                    result += keyword.keyword + ",";
                }
                result = result.TrimEnd(',');
            }
            return result;
        }
        public List<Homekeyword> GetListBycache()
        {
            List<Homekeyword> List = RedisUtil.Get<List<Homekeyword>>(key);
            if (List == null || List.Count == 0)
            {
                List = this.GetList("1=1", 100, 1, "*", "sort desc,id asc");
                if (List != null && List.Count > 0)
                {
                    RedisUtil.Set<List<Homekeyword>>(key, List, TimeSpan.FromHours(12));
                }
            }
            return List;
        }
    }
}
