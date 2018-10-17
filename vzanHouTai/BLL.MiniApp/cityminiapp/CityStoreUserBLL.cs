using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.cityminiapp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.cityminiapp
{
   public class CityStoreUserBLL : BaseMySql<CityStoreUser>
    {
        #region 单例模式
        private static CityStoreUserBLL _singleModel;
        private static readonly object SynObject = new object();

        private CityStoreUserBLL()
        {

        }

        public static CityStoreUserBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CityStoreUserBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<CityStoreUser> getListByaid(int aid,out int totalCount, int pageSize=10,int pageIndex=1,string userName="",string userPhone="",string startDate = "",string endDate = "", string orderWhere="addTime desc",string appId="")
        {
            string strWhere = $"aid={aid} and state<>-1";

            #region 根据用户昵称模糊匹配用户列表
            List<C_UserInfo> listUserInfo = new List<C_UserInfo>();
            if (!string.IsNullOrEmpty(userName))
            {
                //用户
                string userIds = string.Empty;
                List<int> listIds = new List<int>();
                listUserInfo = C_UserInfoBLL.SingleModel.GetUserListByNickName(userName,appId);
                if (listUserInfo != null && listUserInfo.Count > 0)
                {
                    listIds.AddRange(listUserInfo.Select(x => x.Id));
                    userIds = string.Join(",", listIds);
                }


                if (!string.IsNullOrEmpty(userIds))
                {
                    strWhere += $" and userId in ({userIds})";
                }
            }
            #endregion

            List<MySqlParameter> parameters = new List<MySqlParameter>();

            //电话号码
            if (!string.IsNullOrEmpty(userPhone))
            {
                parameters.Add(new MySqlParameter("@userPhone", $"%{userPhone}%"));
                strWhere += " and phone like @userPhone";
            }

            //时间
            if (!string.IsNullOrEmpty(Utility.EncodeHelper.ReplaceSqlKey(startDate)) && !string.IsNullOrEmpty(Utility.EncodeHelper.ReplaceSqlKey(endDate)))
            {
                startDate = Convert.ToDateTime(startDate + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss");
                endDate = Convert.ToDateTime(endDate + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss");
                strWhere += $" and addTime between '{startDate}' and '{endDate}'";
            }

            totalCount = base.GetCount(strWhere, parameters.ToArray());

            List<CityStoreUser> listCity_UserMsg = base.GetListByParam(strWhere, parameters.ToArray(), pageSize, pageIndex, "*", orderWhere);
            if (listCity_UserMsg != null && listCity_UserMsg.Count > 0)
            {
                listCity_UserMsg.ForEach(x => {

                    if (listUserInfo != null && listUserInfo.Count > 0)
                    {
                        x.userName = listUserInfo.FirstOrDefault(u => u.Id == x.userId).NickName;
                    }
                    else
                    {
                        C_UserInfo c_UserInfo = C_UserInfoBLL.SingleModel.GetModel(x.userId);
                        if (c_UserInfo != null)
                        {
                            x.userName = c_UserInfo.NickName;
                        }
                    }
                    Ccity_userMsgCountViewMolde model = GetCountViewModel(x.userId);
                    if (model != null)
                    {
                        x.msgTotalCount = model.msgTotalCount;
                        x.topMsgCostTotalPrice = model.topMsgCostTotalPrice;
                        x.topMsgTotalCount = model.topMsgTotalCount;
                    }




                });
            }

            return listCity_UserMsg;
        }

        /// <summary>
        /// 获取用户发布信息的统计 总发帖数量-总置顶天数-置顶天数所花费的总金额
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Ccity_userMsgCountViewMolde GetCountViewModel(int userId)
        {
            Ccity_userMsgCountViewMolde model = new Ccity_userMsgCountViewMolde();
            if (userId > 0)
            {
                string sql = $"SELECT COUNT(Id) as msgTotalCount,SUM(topCostPrice) as topMsgCostTotalPrice,(SELECT COUNT(Id) from citymsg where userId={userId} and topday>0 and state<>0) as topMsgTotalCount from citymsg where userId={userId} and state<>0";

                using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql))
                {

                    while (dr.Read())
                    {
                        model.msgTotalCount = Convert.ToInt32(dr["msgTotalCount"]);
                        model.topMsgCostTotalPrice = (dr["topMsgCostTotalPrice"] == DBNull.Value ? 0 : Convert.ToDouble(dr["topMsgCostTotalPrice"]) * 0.01).ToString("0.00");
                        model.topMsgTotalCount = Convert.ToInt32(dr["topMsgTotalCount"]);
                    }

                }

            }
            return model;
        }


        public CityStoreUser getCity_StoreUser(int aid,int userId)
        {
           return base.GetModel($"aid={aid} and userId={userId}");
        }



    }
}
