using DAL.Base;
using Entity.MiniApp.Conf;
using System;
using System.Data;
using Utility;

namespace BLL.MiniApp.Conf
{
    /// <summary>
    /// 不用
    /// </summary>
    public class TemplateMsg_UserLogBLL : DAL.Base.BaseMySql<TemplateMsg_UserLog>
    {
        #region 单例模式
        private static TemplateMsg_UserLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private TemplateMsg_UserLogBLL()
        {

        }

        public static TemplateMsg_UserLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new TemplateMsg_UserLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="openid">对应用户的openId</param>
        /// <param name="tmpType">模板类型</param>
        /// <returns></returns>
        public TemplateMsg_UserLog getModelByOrderIdOpenId(int orderid, string openid,int tmpType)
        {
            return GetModel($" OrderId = {orderid} and  Open_Id = '{openid}' and TmgType = {tmpType}  and State = 0 ") ?? new TemplateMsg_UserLog();
        }
        
        /// <summary>
        /// 完成订单关闭相应模板消息
        /// </summary>
        /// <param name="TmpId">小程序模板主表Id</param>
        /// <param name="Ttypeid">小程序模板类型</param>
        /// <param name="TmId">消息模板表Id</param>
        /// <returns></returns>
        public bool confTemplateMsg(int orderid,int Ttypeid)
        {
            try
            {
                //更改发送记录表有效性
                var strSql = $" update templatemsg_userlog set state = -2 where orderid = {orderid} and Ttypeid = {Ttypeid} ";
                SqlMySql.ExecuteNonQuery(dbEnum.QLWL.ToString(), CommandType.Text, strSql.ToString(), null);

            }
            catch (Exception )
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// 完成订单关闭相应模板消息   --不需
        /// </summary>
        /// <param name="TmpId">小程序模板主表Id</param>
        /// <param name="Ttypeid">小程序模板类型</param>
        /// <param name="TmId">消息模板表Id</param>
        /// <returns></returns>
        public bool updateTemplateState(int orderid, int Ttypeid)
        {
            //try
            //{
            //    //更改发送记录表有效性
            //    var strSql = $" update miniapptemplatemsg_userlog set state = 0 where orderid = {orderid} and Ttypeid = {Ttypeid} ";
            //    SqlMySql.ExecuteNonQuery(dbEnum.QLWL.ToString(), CommandType.Text, strSql.ToString(), null);

            //}
            //catch (Exception ex)
            //{
            //    return false;
            //}

            return true;
        }

    }
}
