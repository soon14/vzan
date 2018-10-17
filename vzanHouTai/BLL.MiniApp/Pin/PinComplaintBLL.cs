using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Pin;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Pin
{
    public class PinComplaintBLL : BaseMySql<PinComplaint>
    {
        #region 单例模式
        private static PinComplaintBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinComplaintBLL()
        {

        }

        public static PinComplaintBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinComplaintBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<PinComplaint> GetListByCondition(int aid, int pageIndex, int pageSize, string outTradeNo, string userName, int storeId, string storeName, int orderState, DateTime? startDate, DateTime? endDate, int state, out int recordCount)
        {
            List<PinComplaint> list = new List<PinComplaint>();
            recordCount = 0;
            string sql = $"select a.* from PinComplaint a left join pingoodsorder b on a.orderid=b.id";
            string countSql = $"select count(1) from PinComplaint a left join pingoodsorder b on a.orderid=b.id";
            string sqlwhere = $" where a.aid={aid} and a.state>-1";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(outTradeNo))
            {
                sqlwhere += " and b.outtradeno like @outtradeno";
                parameters.Add(new MySqlParameter("@outtradeno", $"%{outTradeNo}%"));
            }
            if (!string.IsNullOrEmpty(userName))
            {
                XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
                if (xcx == null)
                {
                    return list;
                }
                List<MySqlParameter> paras = new List<MySqlParameter>();
                paras.Add(new MySqlParameter("@userName", $"%{userName}%"));
                List<C_UserInfo> userList = C_UserInfoBLL.SingleModel.GetListByParam($"appid='{xcx.AppId}' and nickname like @userName", paras.ToArray());
                if (userList == null || userList.Count <= 0)
                {
                    return list;
                }
                string userIds = string.Join(",", userList.Select(user => user.Id));
                sqlwhere += $" and a.userid in ({userIds})";
            }
            if (storeId != -999)
            {
                sqlwhere += $" and a.storeid={storeId}";
            }
            if (!string.IsNullOrEmpty(storeName)&& storeId==-999)//有storeId就不根据店铺名模糊查询
            {
                List<MySqlParameter> paras = new List<MySqlParameter>();
                paras.Add(new MySqlParameter("@storeName", $"%{storeName}%"));
                
                List<PinStore> stores = PinStoreBLL.SingleModel.GetListByParam("state>0 and storename like @storeName", paras.ToArray());
                if (stores == null || stores.Count <= 0)
                {
                    return list;
                }
                string storeIds = string.Join(",", stores.Select(store => store.id));
                sqlwhere += $" and a.storeid in ({storeIds})";
            }
            if (orderState != -999)
            {
                sqlwhere += $" and b.state={orderState}";
            }
            if (startDate != null && endDate != null)
            {
                sqlwhere += $" and b.paytime between @startDate and @endDate";
                parameters.Add(new MySqlParameter("@startDate", $"{((DateTime)startDate).ToString("yyyy-MM-dd")} 00:00:00"));
                parameters.Add(new MySqlParameter("@endDate", $"{((DateTime)endDate).ToString("yyyy-MM-dd")} 23:59:59"));
            }
            if (state != -999)
            {
                sqlwhere += $" and a.state={state}";
            }

            sql += $"{sqlwhere} order by a.id desc limit {(pageIndex - 1) * pageSize},{pageSize}";
            countSql += sqlwhere;
            list = GetListBySql(sql, parameters.ToArray());
            recordCount = GetCountBySql(countSql, parameters.ToArray());
            return list;
        }

        public PinComplaint GetModelByUserId_OrderId(int userId, int orderId)
        {
            string sqlwhere = $" userId={userId} and orderId={orderId}";
            return GetModel(sqlwhere);
        }

        public PinComplaint GetModelByAid_Id(int aid, int id)
        {
            string sqlwhere = $" aid={aid} and id={id} and state>-1";
            return GetModel(sqlwhere);
        }
    }
}
