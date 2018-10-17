using DAL.Base;
using Entity.MiniApp.Conf;
using System.Collections.Generic;

namespace BLL.MiniApp.Conf
{
    public class DevelopmentBLL : BaseMySql<Development>
    {
        #region 单例模式
        private static DevelopmentBLL _singleModel;
        private static readonly object SynObject = new object();

        private DevelopmentBLL()
        {

        }

        public static DevelopmentBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DevelopmentBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<Development> GetListByAppid(int appid,int pageIndexInt,int pageSize)
        {
            string wheresql = "appId = " + appid + " and State = 1";

            return GetList(wheresql, pageSize, pageIndexInt, "", "Year desc,Month desc,id desc");
        }
        public int GetListByAppidCount(int appid)
        {
            string wheresql = "appId = " + appid + " and State = 1";

            return GetCount(wheresql);
        }
    }
}
