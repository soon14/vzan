using DAL.Base;
using Entity.MiniApp.Stores;
using System;

namespace BLL.MiniApp.Stores
{
    public class StoreGoodsOrderLogBLL : BaseMySql<StoreGoodsOrderLog>
    {
        #region 单例模式
        private static StoreGoodsOrderLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private StoreGoodsOrderLogBLL()
        {

        }

        public static StoreGoodsOrderLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new StoreGoodsOrderLogBLL();
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
            var model = new StoreGoodsOrderLog();
            model.UserId = userid;
            model.GoodsOrderId = oid;
            model.LogInfo = LogInfo;
            model.CreateDate = DateTime.Now;
            model.Id = Convert.ToInt32(Add(model));
            return model.Id;
        }
    }
}
