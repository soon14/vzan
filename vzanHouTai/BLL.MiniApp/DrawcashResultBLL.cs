//using BLL.MiniSNS.chat;
using BLL.MiniSNS.VZANCity;
using DAL.Base;
using Entity.MiniSNS;
using Entity.MiniSNS.chat;
using Entity.MiniSNS.VZANCity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniSNS
{
    public class DrawcashResultBLL : BaseMySql<DrawcashResult>
    {
        public void NOTENOUGH(string amout, PayCenterSetting setting = null)
        {
            if (WebSiteConfig.IsSendToAdmin != "1")
            {
                return;
            }
            if (setting == null)
            {
                setting = new PayCenterSetting();
                setting.Appid = "wx9dc9ddc7b3eb7f8d";
            }
            if (WebConfigBLL.WzCompanyAppidList.Contains(setting.Appid))
            {
                int[] userids = new int[] { 10475223, 483747, 181993615 };
                string tid = "faxeaBv8TpcHNzgH5g_-23_yFSwXnI_fJqqJ5QYsVFY";
                string openid = "";
                string title = "报警：用户提现余额不足，请尽快充值！用户提现金额：" + amout + "(元),提现APPID：" + setting.Appid;
                string fromName = "微赞";
                string remark = "如10分钟之后发现提现余额不足，会继续提醒";
                string datajson = "{\"first\": {\"value\":\"" + title + "\",\"color\":\"#FF0000\"},\"keyword1\":{\"value\":\"" + fromName + "\",\"color\":\"#333333\"},\"keyword2\": {\"value\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\",\"color\":\"#333333\"},\"keyword3\": {\"value\":\"点击查看详情\",\"color\":\"#333333\"},\"remark\":{\"value\":\"" + remark + "\",\"color\":\"#17b5ee\"}}"; ;

                string postjson = "";
                string _mySerId = WebSiteConfig.WxSerId;
                foreach (int uid in userids)
                {
                    openid = new OAuthUserBll("3").GetModel(uid).Openid;
                    postjson = "{\"touser\":\"" + openid + "\",\"template_id\":\"" + tid + "\",\"url\":\"\",\"data\":" + datajson + "}";
                    string posturl = string.Format(this.DomainName(_mySerId) + "/cgi-bin/message/template/send?access_token={0}", _mySerId);
                    Utility.Net.JKClient.DoPostJson(posturl, postjson);
                    //new PushMsgBLL().SendTips(_minisnsid, model.openid, url, "您好,[" + nickName + "]回复帖子[" + _titletips + "]: " + _contenttips, _forumname, "点击查看详情.祝您生活愉快^.^");
                }
            }
            else
            {
                if (setting.BindingType == (int)PayCenterSettingType.City)
                {
                    C_CityInfo city = new C_CityInfoBLL().GetModelByAreaCode(setting.BindingId);
                    C_UserInfo info = new C_UserInfoBLL().GetModelFromCache(city.OpenId);

                    string tid = "faxeaBv8TpcHNzgH5g_-23_yFSwXnI_fJqqJ5QYsVFY";
                    string openid = city.OpenId;
                    string title = "报警：用户提现余额不足，请尽快充值！用户提现金额：" + amout + "(元),提现APPID：" + setting.Appid + "。充值之后请登陆PC管理后台->同城流水，找到余额不足的提现流水，点击重新提现（重新提现不会造成重复提现的现象）";
                    string fromName = "微赞";
                    string remark = "如10分钟之后发现提现余额不足，会继续提醒";
                    string datajson = "{\"first\": {\"value\":\"" + title + "\",\"color\":\"#FF0000\"},\"keyword1\":{\"value\":\"" + fromName + "\",\"color\":\"#333333\"},\"keyword2\": {\"value\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\",\"color\":\"#333333\"},\"keyword3\": {\"value\":\"点击查看详情\",\"color\":\"#333333\"},\"remark\":{\"value\":\"" + remark + "\",\"color\":\"#17b5ee\"}}"; ;

                    string postjson = "";
                    string _mySerId = WebSiteConfig.WxSerId;

                    postjson = "{\"touser\":\"" + openid + "\",\"template_id\":\"" + tid + "\",\"url\":\"\",\"data\":" + datajson + "}";
                    string posturl = string.Format(this.DomainName(_mySerId) + "/cgi-bin/message/template/send?access_token={0}", _mySerId);
                    Utility.Net.JKClient.DoPostJson(posturl, postjson);
                } 
            } 
        }
        private string DomainName(string accessToken)
        {
            string domainName = "https://api.weixin.qq.com";
            if (!string.IsNullOrEmpty(accessToken) && accessToken.Length < 30)
                return domainName = ConfigurationManager.AppSettings["transferruleUrl"].ToString();
            return domainName;
        }
    }
}
