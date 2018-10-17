using Entity.MiniApp.Conf;
using System;
using System.Linq;

namespace BLL.MiniApp.Conf
{
    public class TemplateMsg_UserParamBLL : DAL.Base.BaseMySql<TemplateMsg_UserParam>
    {
        #region 单例模式
        private static TemplateMsg_UserParamBLL _singleModel;
        private static readonly object SynObject = new object();

        private TemplateMsg_UserParamBLL()
        {

        }

        public static TemplateMsg_UserParamBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new TemplateMsg_UserParamBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 计入营销功能后不适用
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="openid">对应用户的openId</param>
        /// <param name="tmpType">模板类型</param>
        /// <returns></returns>
        public TemplateMsg_UserParam getParamByOrderIdOpenId(int orderid,int orderidType,string openid)
        {

            //如果没有当前订单生成的标识,则拿备用标识去使用
            return GetModel($" OrderId = {orderid} and  Open_Id = '{openid}' and OrderidType = {orderidType}  and State = 1 and LoseDateTime > now() ") ?? 
                            GetModel($" OrderId = 0 and  Open_Id = '{openid}' and OrderidType = {orderidType}  and State = 1 and LoseDateTime > now() ") ?? new TemplateMsg_UserParam();
        }


        /// <summary>
        /// AppId
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="openid">对应用户的openId</param>
        /// <param name="tmpType">模板类型</param>
        /// <returns></returns>
        public TemplateMsg_UserParam getParamByAppIdOpenId(string appId,string openid)
        {
            return GetModel($" Form_Id is not null and appId = '{appId}'  and  Open_Id = '{openid}' and State = 1 and LoseDateTime > now() ") ?? new TemplateMsg_UserParam();
        }

        /// <summary>
        /// AppId
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="openid">对应用户的openId</param>
        /// <param name="tmpType">模板类型</param>
        /// <returns></returns>
        public TemplateMsg_UserParam getLastParamByAppIdOpenId(string appId, string openid)
        {
            return GetList($" Form_Id is not null and appId = '{appId}'  and  Open_Id = '{openid}' and State = 1 and LoseDateTime > now() ", 1, 1, "ID", "ID DESC").First();
        }

        /// <summary>
        /// 增加一次参数使用次数
        /// </summary>
        /// <param name="userParam"> 被使用的参数</param>
        /// <param name="addUsingNum">使用次数要加多少次</param>
        /// <returns></returns>
        public bool addUsingCount(TemplateMsg_UserParam userParam,int addUsingNum = 1)
        {
            if (userParam.Id <= 0)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception() { Source = $"{userParam} 参数使用失败" });
                return false;
            }
            //使用次数 + 1
            userParam.SendCount += addUsingNum;
            if (userParam.Form_IdType == 0)//假设标识是form_id
            {
                if (userParam.SendCount >= 1)//formId只能被使用一次
                {
                    userParam.State = 0;
                }
            }
            else if(userParam.Form_IdType == 1)//假设标识是prepay_id 
            {
                if (userParam.SendCount >= 3)//formId只能被使用一次
                {
                    userParam.State = 0;
                }
            }

            return Update(userParam, "SendCount,State");
        }

        ///// <summary>
        ///// 完成订单关闭相应模板消息
        ///// </summary>
        ///// <param name="TmpId">小程序模板主表Id</param>
        ///// <param name="Ttypeid">小程序模板类型</param>
        ///// <param name="TmId">消息模板表Id</param>
        ///// <returns></returns>
        //public bool confTemplateMsg(int orderid,int Ttypeid)
        //{
        //    try
        //    {
        //        //更改发送记录表有效性
        //        var strSql = $" update miniapptemplatemsg_userlog set state = -2 where orderid = {orderid} and Ttypeid = {Ttypeid} ";
        //        SqlMySql.ExecuteNonQuery(dbEnum.QLWL.ToString(), CommandType.Text, strSql.ToString(), null);

        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }

        //    return true;
        //}


        ///// <summary>
        ///// 完成订单关闭相应模板消息   --不需
        ///// </summary>
        ///// <param name="TmpId">小程序模板主表Id</param>
        ///// <param name="Ttypeid">小程序模板类型</param>
        ///// <param name="TmId">消息模板表Id</param>
        ///// <returns></returns>
        //public bool updateTemplateState(int orderid, int Ttypeid)
        //{
        //    //try
        //    //{
        //    //    //更改发送记录表有效性
        //    //    var strSql = $" update miniapptemplatemsg_userlog set state = 0 where orderid = {orderid} and Ttypeid = {Ttypeid} ";
        //    //    SqlMySql.ExecuteNonQuery(dbEnum.QLWL.ToString(), CommandType.Text, strSql.ToString(), null);

        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    return false;
        //    //}

        //    return true;
        //}

    }
}
