using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Sms.Model.V20160927;

namespace Core.MiniApp
{
    public class SendMsgHelper
    {
        private string user = System.Configuration.ConfigurationManager.AppSettings["ShengYaMsgUser"];
        private string password = System.Configuration.ConfigurationManager.AppSettings["ShengYaMsgPassword"];

        private string alikey = System.Configuration.ConfigurationManager.AppSettings["AliyunAccessKeyId_MTS"];
        private string alisecret = System.Configuration.ConfigurationManager.AppSettings["AccessKeySecret_MTS"];
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phonenums">手机号码，多个可用,隔开</param>
        /// <param name="sign">短信签名，如微赞论坛，微赞交友...</param>
        /// <param name="msgtpl">短信模板1为论坛绑定管理员的,201为微赞交友手机验证码</param>
        /// <param name="param">短信模板里的参数格式"{\"code\":\"4565\"}"</param>
        /// <returns></returns>
        public bool AliSend(string phonenums, string param, string sign = "", int msgtpl = 1)
        {
            IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", alikey, alisecret);
            IAcsClient client = new DefaultAcsClient(profile);
            SingleSendSmsRequest request = new SingleSendSmsRequest();
            try
            {
                if (!string.IsNullOrEmpty(sign))
                    request.SignName = sign;//"管理控制台中配置的短信签名（状态必须是验证通过）
                if (1 == msgtpl)//尊敬的用户：您的微赞账号正在发生绑定操作，校验码：${code}，如有疑问，立即联系微赞公司客服喔！
                    request.TemplateCode = "SMS_52460138";//管理控制台中配置的审核通过的短信模板的模板CODE（状态必须是验证通过）
                if (201 == msgtpl)//尊敬的${user}，你的手机验证码为${code}，请于5分钟内填写。如非本人操作，请忽略本短信- ${forum}
                    request.TemplateCode = "SMS_53770114";//管理控制台中配置的审核通过的短信模板的模板CODE（状态必须是验证通过）
                if (301 == msgtpl)//管理控制台中配置，论坛记过的短信模板的模板
                    request.TemplateCode = "SMS_62765215";
                if (401 == msgtpl)//验证码${code}，您正在进行${product}身份验证，打死不要告诉别人哦！
                    request.TemplateCode = "SMS_34450177";
                request.RecNum = phonenums;//"接收号码，多个号码可以逗号分隔"
                request.ParamString = param;//短信模板中的变量；数字需要转换为字符串；个人用户每个变量长度必须小于15个字符。
                SingleSendSmsResponse httpResponse = client.GetAcsResponse(request);
                return true;
            }
            catch (ServerException ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return false;
            }
            catch (ClientException ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return false;
            }
        }
        
    }
}
