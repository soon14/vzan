using DAL.Base;
using Entity.OpenWx;
using System;
using System.Configuration;
using Newtonsoft;
using Core.OpenWx;

namespace BLL.OpenWx
{
    public class OpenPlatConfigBLL : BaseMySql<OpenPlatConfig>
    {
        #region 单例模式
        private static OpenPlatConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private OpenPlatConfigBLL()
        {

        }

        public static OpenPlatConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new OpenPlatConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private static object _objlock = new object();
        private static int minutes = 60;
        private static string key = "chx_OpenPlatConfig_{0}";
        //0：chx_OpenPlatConfig_wx9a6ab00a752e10e8
        //1：chx_OpenPlatConfig_wx4fd9b6270bd213b5
        private static string tickkey = "chx_xcx_tick_{0}";//chx_xcx_tick_wx4fd9b6270bd213b5
        public bool ComponentVerifyTicket(string appid, string ticket, DateTime createtime)
        {
            OpenPlatConfig model = getCurrentModel();
            if (model == null)
            {
                model = new OpenPlatConfig();
            }

            model.component_Appid = appid;
            model.component_verify_ticket = ticket;
            model.ticket_time = createtime;
            Update(model, "component_verify_ticket,ticket_time");

            RedisUtil.Set<OpenPlatConfig>(string.Format(tickkey, model.component_Appid), model, TimeSpan.FromMinutes(10));
            //log4net.LogHelper.WriteInfo(this.GetType(),"获取ticket="+ticket);
            return true;
        }

        /// <summary>
        /// 获取VerifyTicket
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public string GetComponentVerifyTicket(string appid = "")
        {
            OpenPlatConfig ticketModel = RedisUtil.Get<OpenPlatConfig>(string.Format(tickkey, appid));
            if (ticketModel == null)
            {
                ticketModel = GetModel($"component_Appid='{appid}'");
            }
            if(ticketModel==null)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "没有找到平台tiket");
                return "";
            }
            return ticketModel.component_verify_ticket;
        }
        
        public OpenPlatConfig getCurrentModel(string appid = "", bool isreflsh = false, bool isqiangzhi = false)
        {
            if (string.IsNullOrEmpty(appid))
            {
                appid = ConfigurationManager.AppSettings["Component_Appid"];
            }

            OpenPlatConfig model = RedisUtil.Get<OpenPlatConfig>(string.Format(key, appid));
            if (model != null)
            {
                if (!((model.token_time.AddMinutes(minutes) < DateTime.Now)))
                {
                    return model;
                }
            }
                

            lock (_objlock)
            {
                model = RedisUtil.Get<OpenPlatConfig>(string.Format(key, appid));
                if (model != null)
                {
                    if (!((model.token_time.AddMinutes(minutes) < DateTime.Now)))
                    {
                        return model;
                    }
                }

                model = GetModel(string.Format("component_Appid='{0}'", appid));
                if (model == null)
                {
                    throw new Exception("没有设置第三方平台信息");
                }

                if ((model.token_time.AddMinutes(minutes) < DateTime.Now) || string.IsNullOrEmpty(model.component_access_token))//刷新token
                {
                    try
                    {
                        string tiket = GetComponentVerifyTicket(model.component_Appid);
                        ComponentAccessTokenResult token = WxRequest.GetComonentToken(model.component_Appid, model.component_AppSecret, tiket);

                        if (token != null && token.component_access_token!=null && token.component_access_token.Length > 0)
                        {
                            model.component_access_token = token.component_access_token;
                            model.token_time = DateTime.Now;

                            bool result = Update(model, "component_access_token,token_time");
                            if (!result)
                            {
                                log4net.LogHelper.WriteInfo(this.GetType(), "保存刷新平台后Token失败");
                            }
                        }
                        else
                        {
                            log4net.LogHelper.WriteInfo(this.GetType(), "第三方平台刷新token失败1：" + Utility.SerializeHelper.SerToJson(token));
                        }
                    }
                    catch (Exception ex)
                    {
                        log4net.LogHelper.WriteError(this.GetType(), ex);
                    }
                }

                RedisUtil.Set<OpenPlatConfig>(string.Format(key, appid), model, TimeSpan.FromMinutes(minutes));
            }

            return model;
        }
    }
}
