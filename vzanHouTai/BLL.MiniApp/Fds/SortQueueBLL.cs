using BLL.MiniApp.Footbath;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Stores;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.Stores;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Fds
{
    public class SortQueueBLL : BaseMySql<SortQueue>
    {
        #region 单例模式
        private static SortQueueBLL _singleModel;
        private static readonly object SynObject = new object();

        private SortQueueBLL()
        {

        }

        public static SortQueueBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SortQueueBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 单个model缓存
        /// </summary>
        private readonly string Redis_SortQueue = "Miniapp_SortQueue_{0}";
        /// <summary>
        /// 单个model缓存
        /// </summary>
        private readonly string Redis_SortQueue_version = "Miniapp_SortQueues_version_{0}";

        /// <summary>
        /// 当前 店铺 的队列集合
        /// </summary>
        private readonly string Redis_SortQueues = "Miniapp_SortQueues_{0}_{1}_{2}_{3}_{4}";
        /// <summary>
        /// 当前 店铺 的队列集合(队列中)
        /// </summary>
        private readonly string Redis_SortQueues_queueing = "Miniapp_SortQueues_{0}_{1}";
        /// <summary>
        /// 当前 店铺 的队列集合
        /// </summary>
        private readonly string Redis_SortQueues_version = "Miniapp_SortQueues_version_{0}_{1}";

        /// <summary>
        /// 获取当前队列信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public override SortQueue GetModel(int Id)
        {
            string model_key = string.Format(Redis_SortQueue, Id);
            string version_key = string.Format(Redis_SortQueue_version, Id);

            RedisModel<SortQueue> redisModel_Store = RedisUtil.Get<RedisModel<SortQueue>>(model_key);
            int version = RedisUtil.GetVersion(version_key);
            if (redisModel_Store == null || redisModel_Store.DataList == null
                    || redisModel_Store.DataList.Count > 0 || redisModel_Store.DataVersion != version)
            {
                redisModel_Store = new RedisModel<SortQueue>();
                List<SortQueue> store = new List<SortQueue>() { base.GetModel(Id) };

                redisModel_Store.DataList = store;
                redisModel_Store.DataVersion = version;
                redisModel_Store.Count = store.Count;

                RedisUtil.Set<RedisModel<SortQueue>>(model_key, redisModel_Store, TimeSpan.FromHours(12));
            }
            return redisModel_Store.DataList[0];
        }

        /// <summary>
        /// 获取 当前 店铺 的队列集合
        /// </summary>
        /// <param name="aId"></param>
        /// <returns></returns>
        public List<SortQueue> GetListByWhere(ref int recordCount, int aId,int storeId = 0,int state = 0,int pageIndex = 1,int pageSize = 10,DateTime? startTime = null,DateTime? endTime = null,string telephone = "",int sortNo = 0)
        {
            bool disableRedis = false;//有否查询条件时,若有则不用缓存
            string sqlWhere = "";
            List<MySqlParameter> sqlParams = new List<MySqlParameter>();

            if (startTime != null && startTime > Convert.ToDateTime("0001-01-01 00:00:00"))
            {
                disableRedis = true;
                sqlWhere += " and createDate >= @startTime ";
                sqlParams.Add(new MySqlParameter("@startTime", startTime?.ToString("yyyy-MM-dd 00:00:00")));
            }
            if (endTime != null && endTime > Convert.ToDateTime("0001-01-01 00:00:00"))
            {
                disableRedis = true;
                sqlWhere += " and createDate <= @endTime ";
                sqlParams.Add(new MySqlParameter("@endTime", endTime?.ToString("yyyy-MM-dd 23:59:59")));
            }
            if (!string.IsNullOrWhiteSpace(telephone))
            {
                disableRedis = true;
                sqlWhere += " and telephone = @telephone ";
                sqlParams.Add(new MySqlParameter("@telephone", telephone));
            }
            if (sortNo > 0)
            {
                disableRedis = true;
                sqlWhere += " and sortNo = @sortNo ";
                sqlParams.Add(new MySqlParameter("@sortNo", sortNo));
            }
            string execSql = $" aId={aId} "+ (storeId <= 0 ? string.Empty : $" and storeId = {storeId} ") + (state == 0 ? " and State = 0 " : "and State in (-1,1)") + sqlWhere ;
            if (disableRedis) 
            {
                recordCount = base.GetCount(execSql, sqlParams.ToArray());
                return base.GetListByParam(execSql, sqlParams.ToArray(), pageSize, pageIndex, "*", (state == 0 ? " id asc " : " id desc "));
            }
            else
            {
                string model_key = string.Format(Redis_SortQueues, aId, storeId, state, pageIndex, pageSize);
                string version_key = string.Format(Redis_SortQueues_version, aId, storeId);

                RedisModel<SortQueue> redisModel_SortQueues = RedisUtil.Get<RedisModel<SortQueue>>(model_key);
                int version = RedisUtil.GetVersion(version_key);
                if (redisModel_SortQueues == null || redisModel_SortQueues.DataList == null
                        || redisModel_SortQueues.DataList.Count > 0 || redisModel_SortQueues.DataVersion != version)
                {
                    redisModel_SortQueues = new RedisModel<SortQueue>();
                    List<SortQueue> SortQueues = base.GetListByParam(execSql, sqlParams.ToArray(),pageSize,pageIndex,"*",(state == 0 ? " id asc " : " id desc "));

                    string userIds = string.Join(",", SortQueues?.Select(s=>s.userId).Distinct());
                    List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

                    SortQueues?.ForEach(s =>
                    {
                        s.nickName = userInfoList?.FirstOrDefault(f=>f.Id == s.userId)?.NickName;
                    });
                    recordCount = base.GetCount(execSql, sqlParams.ToArray());

                    redisModel_SortQueues.DataList = SortQueues;
                    redisModel_SortQueues.DataVersion = version;
                    redisModel_SortQueues.Count = recordCount;

                    RedisUtil.Set<RedisModel<SortQueue>>(model_key, redisModel_SortQueues, TimeSpan.FromHours(12));
                }
                return redisModel_SortQueues.DataList;
            }
        }

        /// <summary>
        /// 获取当前 正在排队的 队列数据集合
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public List<SortQueue> GetListByQueueing(int aId, int storeId = 0)
        {
            string model_key = string.Format(Redis_SortQueues_queueing, aId, storeId);
            string version_key = string.Format(Redis_SortQueues_version, aId, storeId);

            RedisModel<SortQueue> redisModel_SortQueues = RedisUtil.Get<RedisModel<SortQueue>>(model_key);
            int version = RedisUtil.GetVersion(version_key);
            if (redisModel_SortQueues == null || redisModel_SortQueues.DataList == null
                    || redisModel_SortQueues.DataList.Count > 0 || redisModel_SortQueues.DataVersion != version)
            {
                redisModel_SortQueues = new RedisModel<SortQueue>();
                List<SortQueue> sortQueues = base.GetList($" aId={aId} {(storeId <= 0 ? "" : $" and storeId = {storeId} ")} and State = 0 ");
                
                string userIds = string.Join(",", sortQueues?.Select(s => s.userId).Distinct());
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

                sortQueues?.ForEach(s =>
                {
                    s.nickName = userInfoList?.FirstOrDefault(f=>f.Id == s.userId)?.NickName;

                });
                redisModel_SortQueues.DataList = sortQueues;
                redisModel_SortQueues.DataVersion = version;
                redisModel_SortQueues.Count = sortQueues.Count;

                RedisUtil.Set<RedisModel<SortQueue>>(model_key, redisModel_SortQueues, TimeSpan.FromHours(12));
            }
            return redisModel_SortQueues.DataList;
        }



        /// <summary>
        /// 通知到号用户及即将到号用户排队情况(只应用于操作了队列之后去发送)
        /// </summary>
        /// <param name="errMsg"></param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="oldSortQueue">在操作排队记录前的队列数据集合</param>
        public void SendTemplateMsgToNextUser(ref string errMsg,int aId, int storeId, List<SortQueue> oldSortQueue)
        {
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aId);
            if (xcxrelation == null)
            {
                errMsg = "未找到小程序的授权资料";
                return;
            }
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if (xcxTemplate == null)
            {
                errMsg = "未找到小程序的模板";
                return;
            }

            string storeName = string.Empty;//店铺名称
                                            //店铺坐标经纬度
            double lat = 0.00;
            double lng = 0.00;
            switch (xcxTemplate.Type)
            {
                case (int)TmpType.小程序餐饮模板:
                    Food store_Food = FoodBLL.SingleModel.GetModel($" appId = {xcxrelation.Id} {(storeId > 0 ? $" and Id = {storeId}" : "")} ");
                    if (store_Food == null)
                    {
                        errMsg = "未找到店铺信息";
                        return;
                    }
                    storeName = store_Food.FoodsName;
                    lat = store_Food.Lat;
                    lng = store_Food.Lng;
                    break;
                case (int)TmpType.小程序专业模板:
                    Store store = StoreBLL.SingleModel.GetModel($" appId = {xcxrelation.Id} {(storeId > 0 ? $" and Id = {storeId}" : "")} ");
                    if (store == null)
                    {
                        errMsg = "未找到店铺信息";
                        return;
                    }
                    storeName = store.name;
                    lat = store.Lat;
                    lng = store.Lng;
                    break;

                default:
                    errMsg = "未找到店铺信息";
                    return;
            }

            //没有店铺名，默认取小程序名
            if (string.IsNullOrWhiteSpace(storeName))
            {
                XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(aId);
                if (xcx == null)
                {
                    errMsg = "未找到小程序信息";
                    return;
                }

                OpenAuthorizerConfig config = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcx.AppId, xcx.Id);
                if (config == null)
                {
                    errMsg = "未找到小程序配置信息";
                    return;
                }

                storeName = config.nick_name;
            }



            oldSortQueue = oldSortQueue?.OrderBy(s => s.id).ToList() ?? new List<SortQueue>();
            //最新队列情况
            List<SortQueue> newSortQueue = GetListByQueueing(aId, storeId)?.OrderBy(s => s.id).ToList();
            if (newSortQueue == null || !newSortQueue.Any())
            {
                errMsg = "队列为空,不需要发送模板消息";
                return;
            }
            //当前叫号,通过匹配新旧队列判断是否当前叫号人有变动,若有变动去通知
            SortQueue curSortQueue = newSortQueue.FirstOrDefault();
            int? oldCurSortQueueId = oldSortQueue?.FirstOrDefault()?.id;
            if (curSortQueue != null && curSortQueue.id > 0
                                && (oldCurSortQueueId == null || curSortQueue.id != oldCurSortQueueId))
            {
                switch (xcxTemplate.Type)//不同业务,不同的模板选用字段
                {
                    case (int)TmpType.小程序餐饮模板:
                        object curSortQueue_TemplateMsgObj = TemplateMsg_Miniapp.SortQueueGetTemplateMessageData(storeName, curSortQueue, SendTemplateMessageTypeEnum.排队拿号排队到号通知);
                        TemplateMsg_Miniapp.SendTemplateMessage(curSortQueue.userId, SendTemplateMessageTypeEnum.排队拿号排队到号通知, xcxTemplate.Type, curSortQueue_TemplateMsgObj);
                        break;
                    default:
                        curSortQueue_TemplateMsgObj = TemplateMsg_Miniapp.SortQueueGetTemplateMessageData(storeName, curSortQueue, SendTemplateMessageTypeEnum.排队拿号排队到号通知_通用);
                        TemplateMsg_Miniapp.SendTemplateMessage(curSortQueue.userId, SendTemplateMessageTypeEnum.排队拿号排队到号通知_通用, xcxTemplate.Type, curSortQueue_TemplateMsgObj);
                        break;
                }
            }

            //即将到号,通过匹配新旧队列判断是否即将到号人有变动,若有变动去通知
            SortQueue nextSortQueue = newSortQueue.FirstOrDefault(s => s.id > curSortQueue.id);
            int? oldNextSortQueueId = oldSortQueue.FirstOrDefault(s => s.id > oldCurSortQueueId)?.id;
            if (nextSortQueue != null && nextSortQueue.id > 0
                                && (oldNextSortQueueId == null || nextSortQueue.id != oldNextSortQueueId))//操作了排队记录的到号用户或即将到号用户后,才去通知下一位即将到号用户
            {
                switch (xcxTemplate.Type)//不同业务,不同的模板选用字段
                {
                    case (int)TmpType.小程序餐饮模板:
                        object nextSortQueue_TemplateMsgObj = TemplateMsg_Miniapp.SortQueueGetTemplateMessageData(storeName, nextSortQueue, SendTemplateMessageTypeEnum.排队拿号排队即将排到通知);
                        TemplateMsg_Miniapp.SendTemplateMessage(nextSortQueue.userId, SendTemplateMessageTypeEnum.排队拿号排队即将排到通知, xcxTemplate.Type, nextSortQueue_TemplateMsgObj);
                        break;
                    default:
                        nextSortQueue_TemplateMsgObj = TemplateMsg_Miniapp.SortQueueGetTemplateMessageData(storeName, nextSortQueue, SendTemplateMessageTypeEnum.排队拿号排队即将排到通知_通用);
                        TemplateMsg_Miniapp.SendTemplateMessage(nextSortQueue.userId, SendTemplateMessageTypeEnum.排队拿号排队即将排到通知_通用, xcxTemplate.Type, nextSortQueue_TemplateMsgObj);
                        break;
                }
            }
        }


        /// <summary>
        /// 拿号成功后通知用户拿号成功
        /// </summary>
        /// <param name="errMsg"></param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="oldSortQueue">在操作排队记录前的队列数据集合</param>
        public void SendTemplateMsgToGetNoSuccessUser(ref string errMsg, SortQueue curSort)
        {
            if (curSort == null)
            {
                errMsg = "无效的排队记录";
                return;
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(curSort.aId);
            if (xcxrelation == null)
            {
                errMsg = "未找到小程序的授权资料";
                return;
            }
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if (xcxTemplate == null)
            {
                errMsg = "未找到小程序的模板";
                return;
            }

            string storeName = string.Empty;//店铺名称
            //店铺坐标经纬度
            double lat = 0.00;
            double lng = 0.00;
            switch (xcxTemplate.Type)
            {
                case (int)TmpType.小程序餐饮模板:
                    Food store_Food = FoodBLL.SingleModel.GetModel($" appId = {xcxrelation.Id} {(curSort.storeId > 0 ? $" and Id = {curSort.storeId}" : "")} ");
                    if (store_Food == null)
                    {
                        errMsg = "未找到店铺信息";
                        return;
                    }
                    storeName = store_Food.FoodsName;
                    lat = store_Food.Lat;
                    lng = store_Food.Lng;
                    break;
                default:
                    errMsg = "未找到店铺信息";
                    return;
            }
            string msg = string.Empty;
            object curSortQueueGetNoSuccess_Obj = TemplateMsg_Miniapp.SortQueueGetTemplateMessageData(storeName, curSort, SendTemplateMessageTypeEnum.排队拿号排队成功通知);
            TemplateMsg_Miniapp.SendTemplateMessage(curSort.userId, SendTemplateMessageTypeEnum.排队拿号排队成功通知, xcxTemplate.Type, curSortQueueGetNoSuccess_Obj);
        }
        
        #region 重写基层清理缓存
        /// <summary>
        /// 更新此列表缓存最新版本号,达到加载覆盖旧数据
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="yellowpagesType"></param>
        public void RemoveSortQueuesCache(int id, int rid = 0, int storeId = 0)
        {
            if (id > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_SortQueue_version, id));
            }

            if (rid > 0 || storeId > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_SortQueues_version, rid, storeId));
            }
        }

        public override object Add(SortQueue model)
        {
            object newId = base.Add(model);

            RemoveSortQueuesCache(model.id,model.aId, model.storeId);
            return newId;
        }

        public override bool Update(SortQueue model)
        {
            bool isSuccess = base.Update(model);

            RemoveSortQueuesCache(model.id, model.aId, model.storeId);
            return isSuccess;
        }

        public override bool Update(SortQueue model, string columnFields)
        {
            bool isSuccess = base.Update(model, columnFields);

            RemoveSortQueuesCache(model.id, model.aId, model.storeId);
            return isSuccess;
        }
        #endregion
    }
}
