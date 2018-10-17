using DAL.Base;
using Entity.MiniApp.Home;
using System.Collections.Generic;

namespace BLL.MiniApp.Home
{
    public class NewsGwBLL : BaseMySql<NewsGw>
    {
        #region 单例模式
        private static NewsGwBLL _singleModel;
        private static readonly object SynObject = new object();

        private NewsGwBLL()
        {

        }

        public static NewsGwBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new NewsGwBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<NewsGw> GetListByType(int type,int pageSize,int pageIndex,ref int count)
        {
            string sqlWhere = $"State>0 and Type={type}";
            string sqlOrderBy = "addtime desc";
            count = base.GetCount(sqlWhere);
            List<NewsGw> list = base.GetList(sqlWhere, pageSize, pageIndex, "*", sqlOrderBy);
            return list;
        }
    }
}
