using DAL.Base;
using Entity.MiniApp.Fds;
using System;

namespace BLL.MiniApp.Fds
{
    public class FoodGoodsOrderLogBLL : BaseMySql<FoodGoodsOrderLog>
    {
        #region 单例模式
        private static FoodGoodsOrderLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodGoodsOrderLogBLL()
        {

        }

        public static FoodGoodsOrderLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodGoodsOrderLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        //插入日志
        public int AddLog(int oid,string userid, string LogInfo)
        {
            var model = new FoodGoodsOrderLog();
            model.UserId = userid;
            model.GoodsOrderId = oid;
            model.LogInfo = LogInfo;
            model.CreateDate = DateTime.Now;
            model.Id = Convert.ToInt32(Add(model));
            return model.Id;
        }
    }
}
