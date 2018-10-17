
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Conf
{
    public class TemplateMsgBLL : DAL.Base.BaseMySql<TemplateMsg>
    {
        #region 单例模式
        private static TemplateMsgBLL _singleModel;
        private static readonly object SynObject = new object();

        private TemplateMsgBLL()
        {

        }

        public static TemplateMsgBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new TemplateMsgBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 查找当前模板下的供用户启用的模板
        /// </summary>
        /// <param name="TypeId">小程序模板类型Id</param>
        /// <returns></returns>
        public List<TemplateMsg> GetListByType(int TypeId)
        {
            string where = $" Ttypeid = {TypeId} and State = 1 ";
            return GetList(where);
        }


        /// <summary>
        /// 查找未开启模板消息的用户用户,帮其开通模板消息
        /// </summary>
        public void openNewTemplateMsg()
        {
            //查找所有用户未开启的模板消息    --专业版的基础版不开放模板消息,因此增加过滤条件 (xcxtemplate.Type != 22 or xcxappaccountrelation.versionId != 3) 
            string findNotOpenTemplateMsgSql = $@"select templatemsg.Id as templateMsgId,xcxtemplate.type, xcxappaccountrelation.appId,templatemsg.titileId,templatemsg.colNums,templatemsg.tmgType from xcxappaccountrelation
                                                    inner join xcxtemplate on xcxappaccountrelation.TId = xcxtemplate.Id  and (xcxtemplate.Type != 22 or xcxappaccountrelation.versionId != 3) 
                                                    inner join templatemsg on xcxtemplate.Type =  templatemsg.Ttypeid and templatemsg.state = 1
                                                    left join templatemsg_user on xcxappaccountrelation.AppId = templatemsg_user.AppId and templatemsg_user.TmId = templatemsg.Id
                                                    where IFNULL(xcxappaccountrelation.appid,'') != '' AND templatemsg_user.Id is NULL 
                                                    limit 0,1000 "; //效率太慢了,1次1000条吧

            
            TemplateMsg_User newUserMsg;
            string errorMsg = string.Empty;
            using (MySql.Data.MySqlClient.MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, System.Data.CommandType.Text, findNotOpenTemplateMsgSql, null))
            {
                if (dr != null)
                {
                    while (dr.Read())
                    {
                        if (dr["templateMsgId"] == DBNull.Value || dr["appId"] == DBNull.Value || dr["titileId"] == DBNull.Value
                                    || dr["colNums"] == DBNull.Value || dr["type"] == DBNull.Value || dr["tmgType"] == DBNull.Value)
                        {
                            continue;
                        }

                        //帮未开通模板消息的客户开通模板消息
                        addResultModel _addResult = MsnModelHelper.addMsnToMy(Convert.ToString(dr["appId"]), Convert.ToString(dr["titileId"]), Convert.ToString(dr["colNums"])?.Split(','), ref errorMsg);
                        if (_addResult != null && !string.IsNullOrWhiteSpace(_addResult.template_id))
                        {
                            newUserMsg = new TemplateMsg_User();
                            newUserMsg.AppId = dr["appId"].ToString();
                            newUserMsg.TmId = Convert.ToInt32(dr["templateMsgId"]);
                            newUserMsg.Ttypeid = Convert.ToInt32(dr["type"]);
                            newUserMsg.ColNums =dr["colNums"].ToString();
                            newUserMsg.TitleId = dr["titileId"].ToString();
                            newUserMsg.State = 1;//启用
                            newUserMsg.CreateDate = DateTime.Now;
                            newUserMsg.TemplateId = _addResult.template_id;//微信公众号内的模板Id
                            newUserMsg.TmgType = Convert.ToInt32(dr["tmgType"]);
                            TemplateMsg_UserBLL.SingleModel.Add(newUserMsg);
                        }
                    }
                }
            }
        }

        public TemplateMsg GetModelByTmgType(SendTemplateMessageTypeEnum templateMsgType)
        {
            return base.GetModel($"tmgType = {(int)templateMsgType}");
        }
    }
}
