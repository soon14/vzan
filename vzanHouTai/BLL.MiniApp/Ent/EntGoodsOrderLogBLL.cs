using DAL.Base;
using Entity.MiniApp.Ent;
using System;

namespace BLL.MiniApp.Ent
{
    public class EntGoodsOrderLogBLL : BaseMySql<EntGoodsOrderLog>
    {
        #region 单例模式
        private static EntGoodsOrderLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntGoodsOrderLogBLL()
        {

        }

        public static EntGoodsOrderLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntGoodsOrderLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        //插入日志
        public int AddLog(int oid,int userid, string LogInfo)
        {
            var model = new EntGoodsOrderLog();
            model.UserId = userid;
            model.GoodsOrderId = oid;
            model.LogInfo = LogInfo;
            model.CreateDate = DateTime.Now;
            model.Id = Convert.ToInt32(Add(model));
            return model.Id;
        }
    }
}
