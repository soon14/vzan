using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp.Conf;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Conf
{
    /// <summary>
    /// 用户基础表
    /// </summary>
    public class UserBaseInfoBLL : BaseMySql<UserBaseInfo>
    {
        #region 单例模式
        private static UserBaseInfoBLL _singleModel;
        private static readonly object SynObject = new object();

        private UserBaseInfoBLL()
        {

        }

        public static UserBaseInfoBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new UserBaseInfoBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public UserBaseInfo GetModelByOpenId(string openid,string serverid)
        {
            string strWhere = $"openid='{openid}' and serverid='{serverid}'";
            return GetModel(strWhere);
        }


        public UserBaseInfo GetModelByUnionidServerid(string unionid, string serverid)
        {
            string strWhere = $" unionid='{unionid}' and serverid='{serverid}' ";
            return GetModel(strWhere);
        }

        public override object Add(UserBaseInfo model)
        {
            object obj = null;
            try
            {
                obj = base.Add(model);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(),ex);
            }
            return obj;
        }
        
        public string GetWxname(string unionId)
        {
            string result = "";
            if (!string.IsNullOrEmpty(unionId))
            {
                List<UserBaseInfo> baseInfoList = GetList($" unionId='{unionId}'");
                if (baseInfoList != null && baseInfoList.Count > 0)
                {
                    result = baseInfoList[0].nickname;
                }
            }
            return result;
        }

        /// <summary>
        /// 添加新用户到基础表
        /// </summary>
        /// <param name="xml"></param>
        public void RegisterOAuthUser(RequestXML xml)
        {
            try
            {
                if (xml == null)
                {
                    return;
                }
                if (string.IsNullOrEmpty(xml.FromUserName))
                {
                    return;
                }
                UserBaseInfoBLL ubll = new UserBaseInfoBLL();
                UserBaseInfo umodel = ubll.GetModelByOpenId(xml.FromUserName, xml.ToUserName);
                if (umodel == null)
                {
                    WeiXinUser wx = WxHelper.GetWxUserInfo(WxHelper.GetToken(), xml.FromUserName);
                    if (wx != null && !string.IsNullOrEmpty(wx.openid))
                    {
                        umodel = new UserBaseInfo();
                        umodel.headimgurl = wx.headimgurl;
                        umodel.nickname = wx.nickname;
                        umodel.openid = wx.openid;
                        umodel.unionid = wx.unionid;
                        umodel.country = wx.country;
                        umodel.sex = wx.sex;
                        umodel.city = wx.city;
                        umodel.province = wx.province;
                        umodel.serverid = xml.ToUserName;
                        ubll.Add(umodel);
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
        }

    }
}
