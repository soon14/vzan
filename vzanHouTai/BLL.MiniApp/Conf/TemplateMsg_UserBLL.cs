using Entity.MiniApp;
using Entity.MiniApp.Conf;
using System.Collections.Generic;

namespace BLL.MiniApp.Conf
{
    public class TemplateMsg_UserBLL : DAL.Base.BaseMySql<TemplateMsg_User>
    {
        #region 单例模式
        private static TemplateMsg_UserBLL _singleModel;
        private static readonly object SynObject = new object();

        private TemplateMsg_UserBLL()
        {

        }

        public static TemplateMsg_UserBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new TemplateMsg_UserBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 返回appid下是开启的指定模板
        /// </summary>
        /// <param name="TmpId">小程序模板主表Id</param>
        /// <param name="Ttypeid">小程序模板类型</param>
        /// <param name="TmId">消息模板表Id</param>
        /// <returns></returns>
        public TemplateMsg_User getModelByAppIdTypeId(string AppId, int Ttypeid)
        {
            //return GetModel($" TmpId = {TmpId} and Ttypeid = {Ttypeid} and TmId = {TmId} and State >= 0");
            return GetModel($" AppId = '{AppId}' and Ttypeid = {Ttypeid} and State = 0 ");
        }

        /// <summary>
        /// 返回appid下是开启的指定模板
        /// </summary>
        /// <param name="TmpId">小程序模板主表Id</param>
        /// <param name="Ttypeid">小程序模板类型</param>
        /// <param name="TmId">消息模板表Id</param>
        /// <returns></returns>
        public TemplateMsg_User getModelByAppIdTypeId(string AppId, int Ttypeid,int TmgType)
        {
            //return GetModel($" TmpId = {TmpId} and Ttypeid = {Ttypeid} and TmId = {TmId} and State >= 0");
            var model = GetModel($" AppId = '{AppId}' and Ttypeid = {Ttypeid} and TmgType = {TmgType} ") ;
            if (model == null)
            {
                model = new TemplateMsg_User();
                model.TmId = 0;
            }
            //model.PageUrl = new TemplateMsgBLL().GetModel(model.TmId)?.PageUrl;
            return model;
        }


        /// <summary>
        /// 返回appid下是开启的订单支付模板
        /// </summary>
        /// <param name="AppId">小程序模板主表Id</param>
        /// <returns></returns>
        public TemplateMsg_User GetModelByAppId_BuySuccessTemp(string AppId,ref string msgErr)
        {
            //return GetModel($" TmpId = {TmpId} and Ttypeid = {Ttypeid} and TmId = {TmId} and State >= 0");
            XcxAppAccountRelation tempRelationModel = XcxAppAccountRelationBLL.SingleModel.GetModel($" AppId = '{AppId}' " );
            if (tempRelationModel == null)
            {
                return null;
            }

            var tempType = XcxTemplateBLL.SingleModel.GetModel(tempRelationModel.TId)?.Type;
            if (tempType == null)
            {
                return null;
            }
            
            int TmgType = 0;
            switch (tempType)
            {
                case (int)TmpType.小程序餐饮模板:
                    TmgType = (int)SendTemplateMessageTypeEnum.餐饮订单支付成功通知;
                    break;
                case (int)TmpType.小程序电商模板:
                    TmgType = (int)SendTemplateMessageTypeEnum.电商订单支付成功通知;
                    break;
                default:
                    log4net.LogHelper.WriteInfo(GetType(), msgErr + ":" + tempType);
                    return null;

            }
            
            TemplateMsg_User model = GetModel($" AppId = '{AppId}' and TmgType = {TmgType} ") ;
            msgErr += $" AppId = '{AppId}' and TmgType = {TmgType} ";
            return model;
        }


        /// <summary>
        /// 返回appid下是开启的发货配送模板模板
        /// </summary>
        /// <param name="AppId">小程序模板主表Id</param>
        /// <returns></returns>
        public TemplateMsg_User GetModelByAppId_SippingTemp(string AppId,ref string strErr)
        {
            XcxAppAccountRelation tempRelationModel = XcxAppAccountRelationBLL.SingleModel.GetModel($" AppId = '{AppId}' ");
            if (tempRelationModel == null)
            {
                return null;
            }
            var tempType = XcxTemplateBLL.SingleModel.GetModel(tempRelationModel.TId)?.Type;
            if (tempType == null)
            {
                return null;
            }
            int TmgType = 0;
            //strErr += "Ttypeid:" + TmId;
            switch (tempType)
            {
                case (int)TmpType.小程序餐饮模板:
                    TmgType = (int)SendTemplateMessageTypeEnum.餐饮订单配送通知;
                    break;
                case (int)TmpType.小程序电商模板:
                    TmgType = (int)SendTemplateMessageTypeEnum.电商订单配送通知;
                    break;
                default:
                    return null;
            }

            TemplateMsg_User model = GetModel($" AppId = '{AppId}' and TmgType = {TmgType} ");
            //if (model == null)
            //{
            //    model = new TemplateMsg_User();
            //    model.TmId = 0;
            //}
            return model;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="TmpId">小程序模板主表Id</param>
        /// <param name="Ttypeid">小程序模板类型</param>
        /// <param name="TmId">消息模板表Id</param>
        /// <returns></returns>
        public TemplateMsg_User getModelByAppId(string AppId, int Ttypeid, int TmId)
        {
            //return GetModel($" TmpId = {TmpId} and Ttypeid = {Ttypeid} and TmId = {TmId} and State >= 0");
            return GetModel($" AppId = '{AppId}' and Ttypeid = {Ttypeid} and TmId = {TmId} ");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TmpId">小程序模板主表Id</param>
        /// <param name="Ttypeid">小程序模板类型</param>
        /// <returns></returns>
        public List<TemplateMsg_User> getListByAppId(string appId, int Ttypeid )
        {
            return GetList($" AppId = '{appId}' and Ttypeid = {Ttypeid} ");
        }


        ///// <summary>
        ///// 停止模板消息使用
        ///// </summary>
        ///// <param name="TmpId">小程序模板主表Id</param>
        ///// <param name="Ttypeid">小程序模板类型</param>
        ///// <param name="TmId">消息模板表Id</param>
        ///// <returns></returns>
        //public bool stopTemplate(MiniAppTemplateMsg_User user)
        //{
        //    try
        //    {
        //        user.State = 0;
        //        Update(user, "State");

        //        //更改发送记录表有效性
        //        var strSql = $" update miniapptemplatemsg_userlog set tmuid = '',state = -1 where tmuid = {user.Id} and state = 0 ";
        //        SqlMySql.ExecuteNonQuery(dbEnum.QLWL.ToString(), CommandType.Text, strSql.ToString(), null);

        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }

        //    return true;
        //}


        ///// <summary>
        ///// 开启模板消息使用
        ///// </summary>
        ///// <param name="TmpId">小程序模板主表Id</param>
        ///// <param name="Ttypeid">小程序模板类型</param>
        ///// <param name="TmId">消息模板表Id</param>
        ///// <returns></returns>
        //public int startTemplate(MiniAppTemplateMsg_User user)
        //{
        //    try
        //    {
        //        user.State = 1;
        //        user.Id = Convert.ToInt32(Add(user));

        //        //更改发送记录表有效性
        //        var strSql = $" update miniapptemplatemsg_userlog set tmuid = {user.Id},state = 0 where tmuid = '' and state = -1 and TmgType = {user.TmgType} ";
        //        SqlMySql.ExecuteNonQuery(dbEnum.QLWL.ToString(), CommandType.Text, strSql.ToString(), null);

        //    }
        //    catch (Exception ex)
        //    {
        //        return user.Id;
        //    }

        //    return user.Id;
        //}


    }
}
