using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Entity.MiniApp;
using BLL.MiniApp;
using Core.MiniApp;
using User.MiniApp.Model;
using DAL.Base;
using User.MiniApp.Areas.Pin.Models;
using BLL.MiniApp.Pin;
using Entity.MiniApp.Pin;
using MySql.Data.MySqlClient;

namespace User.MiniApp.Areas.Pin.Controllers
{
    public class OrderController : BaseController
    {
        public static PinGoodsOrderBLL goodsOrderBLL = new PinGoodsOrderBLL();
        public ActionResult PayRecords(string act = "", int aId = 0, int pageIndex = 0, int pageSize = 10, int recordType = 0, string storePhone = "",
            string payUserPhone = "",
            string transaction_id = "",
            string out_trade_no = "",
            int parentAgentId = 0)
        {
            pageIndex -= 1;
            if (pageIndex < 0)
                pageIndex = 0;

            if (string.IsNullOrEmpty(act))
            {

                string fieldSql = $@" 

p.transaction_id,
p.out_trade_no,
p.time_end,
p.total_fee,

c.TuserId,
c.orderno,
c.ShowNote,
c.id as CityMordersId,

s.storename,
s.phone as StoreUserPhone,

go.ordertype,
go.goodsid as GoodsOrderGoodsId,

fu.id as payUserId,
fu.NickName as payusername,
fu.telephone as PayUserPhone,
su.NickName as storeUserName,
su.Id as storeUserId,
g.name as goodsname,
g.id as goodsid,
g.img as goodsimg
";
                if (parentAgentId > 0)
                {
                    fieldSql += ",aa.fuserid";
                }

                string fromSql = $@"
from payresult p LEFT  JOIN citymorders c on p.out_trade_no = c.orderno
LEFT JOIN c_userinfo fu on c.FuserId = fu.Id
LEFT JOIN pinstore s on c.TuserId = s.id
LEFT JOIN c_userinfo su on s.userid = su.id
LEFT JOIN pingoodsorder go on c.id=go.payno
LEFT JOIN pingoods g on go.goodsid=g.id

";
                if (parentAgentId > 0)
                {
                    fromSql += " LEFT JOIN pinagent aa on fu.id=aa.userid ";
                }
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                string whereSql = $" c.ActionType ='{(int)ArticleTypeEnum.PinOrderPay}' and go.aid={aId} ";
                if (recordType == 1)
                    whereSql += " and go.ordertype=1 ";
                else if (recordType == 2)
                    whereSql += " and go.ordertype=0 ";
                if (!string.IsNullOrEmpty(payUserPhone))
                {
                    whereSql += $" and fu.telephone=@PayUserPhone";
                    parameters.Add(new MySqlParameter("@PayUserPhone", payUserPhone));
                }
                if (!string.IsNullOrEmpty(storePhone))
                {
                    whereSql += " and s.phone=@StoreUserPhone";
                    parameters.Add(new MySqlParameter("@StoreUserPhone", storePhone));
                }
                if (!string.IsNullOrEmpty(transaction_id))
                {
                    whereSql += " and p.transaction_id=@transaction_id";
                    parameters.Add(new MySqlParameter("@transaction_id", transaction_id));
                }
                if (!string.IsNullOrEmpty(out_trade_no))
                {
                    whereSql += " and p.out_trade_no=@out_trade_no";
                    parameters.Add(new MySqlParameter("@out_trade_no", out_trade_no));
                }
                if (parentAgentId > 0)
                {
                    whereSql += " and go.ordertype=1  and  aa.fuserid=" + parentAgentId;//and aa.state=1
                }

                string orderSql = " order by p.id desc ";
                string pagerSql = $" limit {pageSize * pageIndex},{pageSize} ";

                string querySql = $" select distinct {fieldSql} {fromSql} where {whereSql} {orderSql} {pagerSql}";
                string countSql = $" select distinct count(0) {fromSql} where {whereSql}";



                List<PayRecordModel> list = DataHelper.ConvertDataTableToList<PayRecordModel>(SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), System.Data.CommandType.Text, querySql, parameters.ToArray()).Tables[0]);

                string goodsIds = string.Join(",",list?.Select(s=>s.GoodsId).Distinct());
                List<PinGoods> pinGoodsList = PinGoodsBLL.SingleModel.GetListByIds(goodsIds);
                list?.ForEach(p =>
                {
                    p.OrderGoods = pinGoodsList?.FirstOrDefault(f=>f.id ==p.GoodsId);
                    if (p.OrderType == 1)
                    {
                        PinAgent agentInfo = PinAgentBLL.SingleModel.GetModelByUserId(p.PayUserId);
                        if (agentInfo != null && agentInfo.fuserId > 0)
                        {
                            p.ParentAgentUser = C_UserInfoBLL.SingleModel.GetModel(agentInfo.fuserId);
                            p.ParentAgentStore = PinStoreBLL.SingleModel.GetModelByAid_Id(agentInfo.aId, agentInfo.fuserId);
                        }

                    }

                });
                ViewModel<PayRecordModel> vm = new ViewModel<PayRecordModel>();
                vm.DataList = list;
                vm.aId = aId;
                vm.PageSize = pageSize;
                vm.PageIndex = pageIndex;
                vm.TotalCount = PayResultBLL.SingleModel.GetCountBySql(countSql, parameters.ToArray());
                return View(vm);
            }
            return View();
        }
    }
}