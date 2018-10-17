using BLL.MiniApp.Im;
using BLL.MiniApp.Qiye;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Im;
using Entity.MiniApp.Qiye;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Qiye
{
    public class QiyeCustomerBLL : BaseMySql<QiyeCustomer>
    {
        #region 单例模式
        private static QiyeCustomerBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyeCustomerBLL()
        {

        }

        public static QiyeCustomerBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyeCustomerBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public QiyeCustomer GetModelByUserId(int userId)
        {
            string sqlWhere = $"UserId = {userId}";
            return base.GetModel(sqlWhere);
        }

        public List<QiyeCustomer> GetListByUserIds(string userIds)
        {
            if(string.IsNullOrEmpty(userIds))
            {
                return new List<QiyeCustomer>();
            }

            return base.GetList($"userid in ({userIds})");
        }

        public List<QiyeCustomer> GetDataList(int aid, string name, int pageSize, int pageIndex, ref int count)
        {
            List<QiyeCustomer> list = new List<QiyeCustomer>();
            

            string sql = $"select {"{0}"} from QiyeCustomer c left join QiyeEmployee e on c.staffid=e.id";
            string sqlList = string.Format(sql, "c.*,e.name employeename");
            string sqlCount = string.Format(sql, "count(*)");
            string sqlWhere = $" where c.aid={aid} and c.state>=0";
            string sqlPage = $" limit {(pageIndex - 1) * pageSize},{pageSize}";
            if (!string.IsNullOrEmpty(name))
            {
                XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
                if (xcxrelation == null)
                    return list;
                List<C_UserInfo> userList = C_UserInfoBLL.SingleModel.GetListByName(xcxrelation.AppId,name);
                if (userList == null || userList.Count <= 0)
                    return list;

                string userids = string.Join(",", userList.Select(s=>s.Id));
                sqlWhere += $" and c.userid in ({userids})";
            }

            count = base.GetCountBySql(sqlCount+sqlWhere);
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sqlList+sqlWhere+ sqlPage))
            {
                while (dr.Read())
                {
                    QiyeCustomer model = base.GetModel(dr);
                    model.EmployeeName = dr["employeename"].ToString();
                    list.Add(model);
                }
            }

            if (count>0)
            {
                string userIds = string.Join(",", list.Select(s => s.UserId));
                List<C_UserInfo> userList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);
                //用户咨询发送私信数量
                List<ImMessage> imMessageList = ImMessageBLL.SingleModel.GetListByQiyeCustomerUserIds(userIds);
                //用户买单集合
                List<QiyeGoodsOrder> orderList = QiyeGoodsOrderBLL.SingleModel.GetOrderResult(aid,userIds);
                foreach (QiyeCustomer item in list)
                {
                    //访问次数和咨询次数
                    ImMessage imMessageModel = imMessageList?.FirstOrDefault(f => f.fuserId == item.UserId);
                    if(imMessageModel != null)
                    {
                        item.AskCount = imMessageModel.storeId;
                    }
                    C_UserInfo userInfo = userList.FirstOrDefault(f=>f.Id == item.UserId);
                    if(userInfo!=null)
                    {
                        item.Name = userInfo.NickName;
                        item.Phone = userInfo.TelePhone;
                        item.HeadImgUrl = userInfo.HeadImgUrl;
                    }
                    //总订单数和总订单金额
                    QiyeGoodsOrder order = orderList?.FirstOrDefault(f=>f.UserId == item.UserId);
                    if(order!=null)
                    {
                        item.OrderCount = order.QtyCount;
                        item.OrderTotalPrice = order.BuyPrice;
                    }
                }
            }

            return list;
        }
        
        public List<QiyeCustomer> GetDataListApi(int userId,int staffId,int pageSize,int pageIndex,ref int count,string name,string appid)
        {
            List<QiyeCustomer> list = new List<QiyeCustomer>();
            
            
            string sql = $@"select {"{0}"} from QiyeCustomer q left join c_userinfo c on q.userid = c.id ";
            string sqlCount = string.Format(sql,"Count(*)");
            //(select count(*) from immessage where tuserid = q.userid and fuserid = { userId } and isread = 0) messagecount,(select msg from immessage where tuserid = q.userid and fuserid = { userId } order by senddate DESC LIMIT 1) msg
            string sqlList = string.Format(sql, $@"q.*,c.NickName,c.HeadImgUrl,c.TelePhone");
            string sqlWhere = $" where q.StaffId={staffId} and q.state>=0 and c.appid='{appid}'";
            string sqlPage = $" limit {(pageIndex - 1) * pageSize},{pageSize}";

            List<MySqlParameter> parms = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(name))
            {
                sqlWhere += $" and (c.NickName like @name or c.TelePhone like @name or q.Desc like @name)";
                parms.Add(new MySqlParameter("@name", $"%{name}%"));
            }
          //  log4net.LogHelper.WriteInfo(this.GetType(), sqlList + sqlWhere );

            count = base.GetCountBySql(sqlCount+sqlWhere, parms.ToArray());
            if (count <= 0)
                return list;
         //   log4net.LogHelper.WriteInfo(this.GetType(), sqlList + sqlWhere + sqlPage);
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sqlList + sqlWhere + sqlPage, parms.ToArray()))
            {
                while (dr.Read())
                {
                    QiyeCustomer model = base.GetModel(dr);
                    model.NoReadMessageCount = ImMessageBLL.SingleModel.GetNoReadCount(model.UserId, userId);
                    ImMessage imModel = ImMessageBLL.SingleModel.GetNewMessage(model.UserId,userId,0,1);
                    if(imModel!=null)
                    {
                        model.LastMsg = imModel.msg;
                        model.MsgType = imModel.msgType;
                    }
                    
                    if (dr["NickName"] != DBNull.Value)
                    {
                        model.Name = Convert.ToString(dr["NickName"]);
                    }
                    if (dr["HeadImgUrl"] != DBNull.Value)
                    {
                        model.HeadImgUrl = dr["HeadImgUrl"].ToString();
                    }
                    if (dr["TelePhone"] != DBNull.Value)
                    {
                        model.Phone = dr["TelePhone"].ToString();
                    }

                    list.Add(model);
                }
            }
            
            return list;
        }

        /// <summary>
        /// 根据名片ID也就是员工ID 获取绑定的客户
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public int GetEmployeeCustomerCount(int employeeId,int month=0)
        {
            if (month > 0)
            {
                return base.GetCountBySql($"SELECT count(Id) from qiyecustomer  where  staffid ={employeeId} and UserId>0 and DATE_FORMAT(addtime,'%Y%m') = DATE_FORMAT(CURDATE(),'%Y%m')");
            }
            else
            {
                return base.GetCountBySql($"SELECT count(Id) from qiyecustomer  where  staffid ={employeeId} and UserId>0");

            }
        }

        /// <summary>
        /// 批量修改绑定员工
        /// </summary>
        /// <param name="id"></param>
        /// <param name="customerIds"></param>
        /// <returns></returns>
        public bool UpdateBindEmployee(int id,string customerIds)
        {
            if (id <= 0 || string.IsNullOrEmpty(customerIds))
                return false;

            string sql = $"update QiyeCustomer set StaffId={id},UpdateTime='{DateTime.Now}' where id in ({customerIds})";

            return SqlMySql.ExecuteNonQuery(connName,CommandType.Text,sql)>0;
        }

    }
}