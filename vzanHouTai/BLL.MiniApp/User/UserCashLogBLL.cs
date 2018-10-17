using DAL.Base;
using Entity.MiniApp;
using System.Collections.Generic;

namespace BLL.MiniApp
{
    public partial class UserCashLogBLL : BaseMySql<UserCashLog>
    {  
        //public string GetCashLogString(int userId, int minisnsId, int PageSize, int PageIndex, string OrderType = "")
        //{
        //    StringBuilder namestr = new StringBuilder();
        //    List<UserCashLog> list = GetUserCashLogList(userId, minisnsId, PageSize, PageIndex, OrderType);
        //    foreach (UserCashLog log in list)
        //    {
        //        string cashType = string.Empty;
        //        if (log.cashtype == 1 && log.percent == 100)
        //        {
        //            cashType = "爱心众筹";
        //        }
        //        else
        //        {
        //            cashType = GetCashType(log.cashtype);
        //        }
        //        if (log.cashtype == -1)
        //        {
        //            namestr.Append(string.Format("用户{0}{1}元，费率{2}%，提现金额{3}，余额{4}({5})<br/>", cashType, log.vircash * 0.01, log.percent, log.cash * 0.01, log.usercash * 0.01, log.addtime.ToString("yyyy-MM-dd:HH:mm:ss")));
        //        }
        //        else
        //        {
        //            namestr.Append(string.Format("收到{0}{1}元，提成比率{2}%，所得金额{3}，余额{4}({5})<br/>", cashType, log.vircash * 0.01, log.percent, log.cash * 0.01, log.usercash * 0.01, log.addtime.ToString("yyyy-MM-dd:HH:mm:ss")));
        //        }
                
        //    }
        //    string result = namestr.ToString();
        //    if (result.Length > 5)
        //    {
        //        return result.Substring(0, result.Length-5);
        //    }
        //    return result;
        //}
        //public string GetCashType(int cashtype)
        //{
        //    try
        //    {
        //        CashType type = (CashType)cashtype;
        //        return type.ToString();
        //    }
        //    catch (System.Exception)
        //    {
        //        return "未知类型收入";
        //    }
        //}

        public List<UserCashLog> GetUserCashLogList(int userId, int minisnsId, int PageSize, int PageIndex, string OrderType = "")
        {
            if (string.IsNullOrEmpty(OrderType))
            {
                OrderType = "id desc";
            }
            return base.GetList(string.Format("userid={0} and minisnsid={1}", userId, minisnsId), PageSize, PageIndex, "", OrderType);
        }

    }
}