using System;
using System.Collections.Generic;
using DAL.Base;
using Entity.MiniApp.Pin;
using MySql.Data.MySqlClient;
using Utility;
using Entity.MiniApp;
using System.Linq;

namespace BLL.MiniApp.Pin
{
    public class PinStoreBLL : BaseMySql<PinStore>
    {
        #region 单例模式
        private static PinStoreBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinStoreBLL()
        {

        }

        public static PinStoreBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinStoreBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<PinStore> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<PinStore>();

            return base.GetList($"id in ({ids})");
        }
        public List<PinStore> GetListByCondition(string appid,int aid, int pageIndex, int pageSize, out int recordCount, int state = -999, string storeName = "", string phone = "", string nickName = "", int biaogan = -999,string fnickName="",string fphone="")
        {
            List<PinStore> list = new List<PinStore>();
            recordCount = 0;
            string fsql = $"select pinagent.* from pinagent left join c_userInfo on pinagent.userid=c_userInfo.id where c_userinfo.appid='{appid}'";
            List<MySqlParameter> paras = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(fnickName))
            {
                fsql += " and c_userInfo.nickname like @nickname";
                paras.Add(new MySqlParameter("@nickname", $"%{fnickName}%"));
            }
            if (!string.IsNullOrEmpty(fphone))
            {
                fsql += " and c_userInfo.telephone like @telephone";
                paras.Add(new MySqlParameter("@telephone", $"%{fphone}%"));
            }
            string agentIds = string.Empty;
            if (paras.Count > 0)
            {
                List<PinAgent> agentList = PinAgentBLL.SingleModel.GetListBySql(fsql,paras.ToArray());
                if(agentList==null || agentList.Count <= 0)
                {
                    return list;
                }
                agentIds = string.Join(",", agentList.Select(agent => agent.id));
            }

            string sql = $"select a.* from pinstore as a";
            string sqlcount = "select count(1) from pinstore as a";
            
            if (aid <= 0 || pageIndex <= 0 || pageSize <= 0)
            {
                return list;
            }
            string sqlwhere = $"where a.aid={aid} and a.state>=0";
            if (!string.IsNullOrEmpty(agentIds))
            {
                sqlwhere += $" and a.agentid in ({agentIds})";
            }
            if (biaogan != -999)
            {
                sqlwhere += $" and a.biaogan={biaogan}";
            }
            if (state != -999)
            {
                sqlwhere += $" and a.state={state}";
            }
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(storeName))
            {
                sqlwhere += " and a.storeName like @storeName";
                parameters.Add(new MySqlParameter("@storeName", $"%{storeName}%"));
            }
            if (!string.IsNullOrEmpty(phone))
            {
                sqlwhere += " and a.phone like @phone";
                parameters.Add(new MySqlParameter("@phone", $"%{phone}%"));
            }
            if (!string.IsNullOrEmpty(nickName))
            {
                sql += " left join c_userInfo as b on a.userid=b.id";
                sqlcount += " left join  c_userInfo as b on a.userid=b.id";
                sqlwhere += $" and b.nickName like @nickname and b.appid='{appid}'";
                parameters.Add(new MySqlParameter("@nickname", $"%{nickName}%"));
            }
            sql = $"{sql} {sqlwhere} order by a.id desc limit {(pageIndex - 1) * pageSize},{pageSize} ";
            list = GetListBySql(sql, parameters.ToArray());
            sqlcount = $"{sqlcount} {sqlwhere}";
            recordCount = GetCountBySql(sqlcount, parameters.ToArray());
            return list;
        }

        public PinStore GetModelByAid_Id(int aid, int id)
        {
            PinStore store = null;
            if (aid <= 0 || id <= 0)
            {
                return store;
            }
            string sqlwhere = $" aid={aid} and id={id} and state>=0";
            store = GetModel(sqlwhere);
            return store;
        }

        public PinStore GetAdminByLoginParams(string loginName, string password)
        {
            MySqlParameter[] paras = new MySqlParameter[]{
                                 new MySqlParameter("@loginName",loginName),
                                 new MySqlParameter("@password",DESEncryptTools.GetMd5Base32(password)),
                };
            return base.GetModel(" loginName = @loginName and password = @password and state=1 ", paras);
        }

        public PinStore GetStoreByPhone(string loginName)
        {
            MySqlParameter[] paras = new MySqlParameter[]{
                                 new MySqlParameter("@loginName",loginName)
             };
            return base.GetModel(" loginName = @loginName and state=1 ", paras);
        }

        /// <summary>
        /// 开通店铺，新增店铺后将店铺ID更新到用户表的storeId 建立联系
        /// </summary>
        /// <param name="user"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        public int OpenStore(C_UserInfo user, PinStore store)
        {
            object result = Add(store);
            int storeId = 0;
            if (!Convert.IsDBNull(result))
            {
                storeId = Convert.ToInt32(result);
            }
            user.StoreId = storeId;
            bool userResult = C_UserInfoBLL.SingleModel.Update(user, "StoreId");
            if (storeId > 0 && userResult)
                return storeId;
            else
                return 0;
        }

        public PinStore GetModelByAid_UserId(int aid, long userId)
        {
            PinStore store = null;
            if (aid <= 0 || userId <= 0)
            {
                return store;
            }
            string sqlwhere = $"aid={aid} and userId={userId} and state>0";
            store = GetModel(sqlwhere);
            return store;
        }

        public List<PinStore> GetListByAidUserId(int aid, string userIds)
        {
            if (aid <= 0 || string.IsNullOrEmpty(userIds))
            {
                return new List<PinStore>();
            }
            string sqlwhere = $"aid={aid} and userId in ({userIds}) and state>0";
            return base.GetList(sqlwhere);
        }

        /// <summary>
        /// 店铺是否可用
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool IsAvailable(PinStore model)
        {
            if (model == null || model.id <= 0)
                return false;

            DateTime now = DateTime.Now;
            if (model.state == 1 && (now >= model.startDate && now <= model.endDate))
                return true;
            else
                return false;
        }

        public int GetCountByAgentId(int agentId)
        {
            string sqlwhere = $" agentid={agentId} and state>=0";
            return GetCount(sqlwhere);
        }
        /// <summary>
        /// 增加商家收入
        /// </summary>
        /// <param name="order"></param>
        /// <param name="store"></param>
        /// <param name="tran"></param>
        /// <param name="type">0：增加订单实付金额 1：增加订单返利金额</param>
        public void AddIncome(PinGoodsOrder order, PinStore store, TransactionModel tran, int type = 0)
        {
            string fileds = "";
            int money = 0;
            if (type == 0)
            {
                money = order.money - order.returnMoney;
            }
            else
            {
                money = order.returnMoney;
            }
            if (order.sourceType == 0)
            {
                store.money += money;
                store.cash += money;
                fileds = "money,cash";
            }
            else
            {
                store.qrcodeMoney += money;
                store.qrcodeCash += money;
                fileds = "qrcodemoney,qrcodecash";
            }
            MySqlParameter[] pone = null;
            tran.Add(BuildUpdateSql(store, fileds, out pone), pone);
        }
        /// <summary>
        /// 更新商家收入
        /// </summary>
        /// <param name="group"></param>
        /// <param name="tran"></param>
        public void UpdateIncome(PinGroup group, TransactionModel tran)
        {
            PinStore store = GetModel(group.storeId);
            if (store == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"门店信息不存在storeId:{group.storeId}"));
                return;
            }
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            List<PinGoodsOrder> orderList = pinGoodsOrderBLL.GetListByGroupId(group.id);
            if (orderList != null && orderList.Count > 0)
            {
                orderList = orderList.Where(order => order.state == (int)PinEnums.PinOrderState.交易成功 || order.state == (int)PinEnums.PinOrderState.已评价 ).ToList();
                if (orderList != null && orderList.Count > 0)
                {
                    foreach (var order in orderList)
                    {
                        AddIncome(order, store, tran, 1);
                    }
                }
            }
        }

        public List<object> GetStoreListByAgentId_type(int agentId, int pageIndex, int pageSize, out int count, int extractType = 0)
        {
            string sqlwhere = $" agentid={agentId} and state=1";
            if (extractType == 1)
            {
                //查找下下级
                sqlwhere = $" agentid in({GetGrandfatherStoreAgentIds(agentId)}) and state=1 ";
            }
            count = GetCount(sqlwhere);
            List<PinStore> list = GetList(sqlwhere, pageSize, pageIndex, "*", "id desc");
            List<object> objList = new List<object>();
            if (list != null && list.Count > 0)
            {
              
                foreach (var store in list)
                {
                    string sql = $"select sum(income) as income from pinagentincomelog where source=1 and agentid={agentId} and  sourceuid={store.userId} and ExtractType={extractType}";
                    var result = SqlMySql.ExecuteScalar(connName, System.Data.CommandType.Text, sql);
                    string income = result == DBNull.Value ? "0.00" : (Convert.ToInt32(result) * 0.001 * 0.01).ToString("0.00");
                   

                    objList.Add(new { headImg = store.logo, name = store.storeName, income, addtime = store.startDateStr, phone = store.phone ,storeId=store.id });
                }
            }
            return objList;
        }
        /// <summary>
        /// 成为代理自动入驻店铺
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public bool AddStore(PinAgent agent)
        {
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(agent.userId);
            if (userInfo == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"代理缴费自动入驻门店失败：找不到用户信息 userid:{agent.userId}"));
                return false;
            }
            if (userInfo.StoreId > 0) return true;
            PinStore store = GetModel($"loginName=@loginName and state>-1 and aid={agent.aId}", new MySqlParameter[] {
                new MySqlParameter("@loginName",userInfo.TelePhone)
            });
            if (store != null) return true;
            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid((agent.aId));
            if (platform == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"代理缴费自动入驻门店失败：平台信息错误 aId:{agent.aId}"));
                return false;
            }
            PinStore storeInfo = new PinStore
            {
                aId = agent.aId,
                rz = 1,
                state = 1,
                endDate = DateTime.Now.AddDays(platform.freeDays),
                loginName = userInfo.TelePhone,
                password = Utility.DESEncryptTools.GetMd5Base32("123456"),
                startDate = DateTime.Now,
                userId = userInfo.Id,
                phone = userInfo.TelePhone,
                logo = userInfo.HeadImgUrl,
                storeName = userInfo.NickName,
            };
            int result = OpenStore(userInfo, storeInfo);
            return result > 0;
        }

        /// <summary>
        /// 获取下级店铺agentId集合
        /// </summary>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public string GetGrandfatherStoreAgentIds(int agentId)
        {
            List<PinStore> list = base.GetList($" agentid={agentId} and state=1 ", 1000000, 1, "userId");
            if (list == null || list.Count <= 0)
            {
                return "-1";
            }
           
            List<int> listStoreUserIds = new List<int>();
            foreach (PinStore item in list)
            {
                if (!listStoreUserIds.Contains(item.userId))
                {
                    listStoreUserIds.Add(item.userId);
                }
            }

            string strWhere = $" userId in({string.Join(",", listStoreUserIds)}) and state=1 ";

            List<PinAgent> listPinAgent =PinAgentBLL.SingleModel.GetList(strWhere, 1000000, 1, "id");
            List<int> listPinAgentIds = listPinAgent.Select(x=>x.id).ToList();
            if(listPinAgentIds==null|| listPinAgentIds.Count <= 0)
            {
                return "-1";
            }

            return string.Join(",", listPinAgentIds);
        }

        /// <summary>
        /// 获取发展的商户数
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="fuserId"></param>
        /// <param name="extractType">0 查找下级 1查找下下级</param>
        /// <returns></returns>
        public int GetStoreCount(int agentId, int extractType)
        {

            string sqlwhere = $" agentid={agentId} and state=1";
            if (extractType == 1)
            {
                //查找下下级
                sqlwhere = $" agentid in({GetGrandfatherStoreAgentIds(agentId)}) and state=1 ";
            }
            return GetCount(sqlwhere);
        }

    }
}