using DAL.Base;
using Entity.MiniApp.Qiye;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Qiye
{
    public class QiyePostMsgBLL : BaseMySql<QiyePostMsg>
    {
        #region 单例模式
        private static QiyePostMsgBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyePostMsgBLL()
        {

        }

        public static QiyePostMsgBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyePostMsgBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<QiyePostMsg> getListByaid(int aid,int userId,out int totalCount , int pageIndex=1,int pageSize=10,string orderWhere= "AddTime desc")
        {
            string strWhere = $"Aid={aid} and UserId={userId} and State<>-1";
            totalCount = base.GetCount(strWhere);
            List<QiyePostMsg> list = base.GetListByParam(strWhere, null, pageSize, pageIndex, "*",orderWhere);
            return list;
        }

    }
}
