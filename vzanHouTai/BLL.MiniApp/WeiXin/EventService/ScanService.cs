using DAL.Base;
using Entity.MiniApp;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp
{
    public class ScanService
    {
        private string appId = Config.SenparcWeixinSetting.WeixinAppId;
        public ResponseMessageText SetNotice(string openId,string key)
        {
            ResponseMessageText response = null;
            try
            {
                ScanModel lcode = RedisUtil.Get<ScanModel>("wxbindSessionID:" + key);
                if (lcode == null)
                {
                    return response;
                }
                lcode.userInfo = UserApi.Info(appId, openId, Language.zh_CN);
                lcode.isUse = 0;
                RedisUtil.Set<ScanModel>("wxbindSessionID:" + key, lcode, TimeSpan.FromMinutes(3));
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
            return response;
        }
    }
}
