using DAL.Base;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace BLL.MiniApp
{
    public class CommandExceptionLogBLL : BaseMySql<CommandExceptionLog>
    {
        #region 单例模式
        private static CommandExceptionLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private CommandExceptionLogBLL()
        {

        }

        public static CommandExceptionLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CommandExceptionLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 小程序异常邮件推送通知
        /// </summary>
        /// <param name="exLog">异常对象</param>
        /// <param name="toUsers">邮件收件邮箱集合</param>
        /// <param name="body">邮件模板网站根目录下的XcxExceptionLogEmail.html</param>
        /// <returns></returns>
        public bool Send(CommandExceptionLog exLog, List<string> toUsers,string body)
        {
            // 构造邮件体（按照常规构造即可）

            body = body.Replace("{time}", DateTime.Now.ToString()).Replace("{Subject}", exLog.Version).Replace("{XcxAppSmtpUser}", WebSiteConfig.XcxAppSmtpUser)
                 .Replace("{AppId}", exLog.AppId).Replace("{Version}", exLog.Version).Replace("{SourcePath}", exLog.SourcePath).Replace("{ExceptionMsg}",exLog.ExceptionMsg);
             var mail = new MailMessage();
            mail.From = new MailAddress(WebSiteConfig.XcxAppSmtpUser, WebSiteConfig.XcxAppSmtpNickName, Encoding.UTF8);
            if (toUsers != null && toUsers.Count > 0)
            {
                foreach (var item in toUsers)
                {
                    mail.To.Add(new MailAddress(item));
                }
            }
            int SubjectLength = 100;
            if (exLog.ExceptionMsg.Length < 100)
                SubjectLength = exLog.ExceptionMsg.Length;

            string Subject = exLog.ExceptionMsg.Substring(0, SubjectLength).Replace("</br>","");


            mail.Subject = $"【{exLog.Version}】小程序异常通知:{Subject}";
            mail.SubjectEncoding = Encoding.UTF8;
            mail.Body = body;
            mail.BodyEncoding = Encoding.UTF8;
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.Normal;

            // 构造SMTP服务器（很重要！！！）
            var client = new SmtpClient();
            client.UseDefaultCredentials = true;   // 在最终发送成功的代码中，本属性必须在 Credentials 之前赋值
            client.Credentials = new NetworkCredential(WebSiteConfig.XcxAppSmtpUser, WebSiteConfig.XcxAppSmtpPwd);   // 本属性必须在 UseDefaultCredentials 之后赋值
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Host = WebSiteConfig.XcxAppSmtpHost;
            client.Port = Convert.ToInt32(WebSiteConfig.XcxAppSmtpPort);   // 注意打开系统防火墙相应的端口
                                                                         //  client.EnableSsl = true;   // 要看 SMTP 服务器是否支持
            client.SendCompleted += SMTPSendCompleted;

            int exLogId = Convert.ToInt32(base.Add(exLog));//记录到数据库
            try
            { 
                client.SendAsync(mail, exLogId);
                return true;
            }
            catch (Exception e)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "发送异常:" + e.Message);
            }
            return false;
        }

        private void SMTPSendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            SendEmailResult result = new SendEmailResult();

            if (e.Cancelled)
            {
                result.id = -1;
                result.msg = "已取消发送邮件";
            }
            else if (e.Error != null)
            {
                result.id = 0;
                result.msg = "发送失败:" + e.Error.Message;

            }
            else
            {
                result.id = (int)e.UserState;
                result.msg = "发送成功";
                CommandExceptionLog m = base.GetModel(result.id);
                if (m!=null)
                {
                    //更新数据库 发送字段
                    m.IsSend = 1;
                  if(base.Update(m, "IsSend"))
                    {
                        log4net.LogHelper.WriteInfo(this.GetType(), "发送成功并且更新数据库成功" + result.id);//记录发送结果
                    }
                    else
                    {
                        log4net.LogHelper.WriteInfo(this.GetType(), "发送成功但是更新数据库失败"+result.id);//记录发送结果
                    }
                }
            }


            log4net.LogHelper.WriteInfo(this.GetType(), result.id + "==" + result.msg);//记录发送结果


        }
    }
}
