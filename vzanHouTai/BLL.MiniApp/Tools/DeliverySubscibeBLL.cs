using DAL.Base;
using Entity.MiniApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Entity.MiniApp;

namespace BLL.MiniApp.Tools
{
    /// <summary>
    /// 物流跟踪-订阅队列
    /// </summary>
    public class DeliverySubscribeBLL:BaseMySql<DeliverySubscribe>
    {
        #region 单例模式
        private static DeliverySubscribeBLL _singleModel;
        private static readonly object SynObject = new object();

        private DeliverySubscribeBLL()
        {

        }

        public static DeliverySubscribeBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DeliverySubscribeBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 导入接口订阅数据
        /// </summary>
        /// <param name="subscribePush"></param>
        /// <returns></returns>
        public bool AddFromAPI(DeliverySubscribePush subscribePush)
        {
            //事务
            TransactionModel syncFeed = new TransactionModel();

            //订阅推送时间
            DateTime push = DateTime.MinValue;
            DateTime.TryParse(subscribePush.PushTime, out push);
            //订阅推送ID
            Guid subscribeId = Guid.NewGuid();

            foreach (DeliveryData data in subscribePush.Data)
            {
                //预计到达时间
                DateTime estimatedDelivery = DateTime.MinValue;
                DateTime.TryParse(data.EstimatedDeliveryTime, out push);
                //配送状态
                int state = -1;
                int.TryParse(data.State, out state);
                //配送轨迹
                string Traces = data.Traces?.Count > 0 ? JsonConvert.SerializeObject(data.Traces) : null;
                //保存
                DeliverySubscribe newFeed = new DeliverySubscribe
                {
                    Success = data.Success,
                    LogisticCode = data.LogisticCode,
                    ShipperCode = data.ShipperCode,
                    Traces = Traces,
                    State = state,
                    EstimatedDeliveryTime = estimatedDelivery,
                    PushTime = push,
                    SubscribeId = subscribeId,
                    CallBack = data.CallBack,
                    Reason = data.Reason,
                    EBusinessID = data.EBusinessID,
                };
                syncFeed.Add(BuildAddSql(newFeed));
            }

            //执行
            return ExecuteTransactionDataCorect(syncFeed.sqlArray, syncFeed.ParameterArray);
        }

        public List<DeliverySubscribe> GetWaitForSync(int getCount)
        {
            string whereSql = BuilWhereSql(DeliverySubscribeSyncState.未同步);
            return GetList(whereSql, getCount, 1);
        }

        public string BuilWhereSql(DeliverySubscribeSyncState? syncState = null)
        {
            string whereSql = string.Empty;
            if(syncState.HasValue)
            {
                whereSql = $"Sync = {(int)syncState.Value}";
            }
            return whereSql;
        }
    }
}
