using DAL.Base;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Text;
using Utility;

/// <summary>
/// Member转移过来的
/// </summary>
namespace BLL.MiniApp
{
    public class MemberBLL : BaseMySql<Member>
    {
        #region 单例模式
        private static MemberBLL _singleModel;
        private static readonly object SynObject = new object();

        private MemberBLL()
        {

        }

        public static MemberBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new MemberBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        #region 注册会员信息
        /// <summary>
        /// 注册会员信息
        /// </summary>
        /// <param name="AccountId">帐号ID</param>
        /// <param name="Phone">电话</param>
        /// <param name="CellPhone">手机</param>
        /// <param name="Referrer">推荐人</param>
        /// <returns>返回是否成功</returns>
        public bool InsertRegInfo(Guid AccountId, string Phone, string CellPhone)
        {
            Member member = new Member();
            member.Id = new Guid();
            member.AccountId = AccountId;
            member.ConsigneePhone1 = DESEncryptTools.Encrypt(Phone);
            member.ConsigneePhone2 = DESEncryptTools.Encrypt(CellPhone);
            member.Sex = 0;
            member.Birthday = DateTime.Now;
            member.SyncStatus = "I";
            return (int)Add(member) > 0;
        }
        #endregion

        #region 修改会员资料
        /// <summary>
        /// 修改会员资料
        /// </summary>
        /// <param name="theObject">会员对象</param>
        /// <returns>返回是否成功</returns>
        public bool UpdateMember(Member theObject)
        {
            theObject.SyncStatus = "U";
            return Update(theObject);
        }
        #endregion

        #region 发送修改密码邮件
        /// <summary>
        /// 发送修改密码邮件
        /// </summary>
        /// <param name="AccountID">帐号ID</param>
        /// <param name="LoginId">登录ID(登录名)</param>
        /// <param name="Email">注册Email</param>
        /// <param name="Url">修改密码地址</param>
        /// <param name="SendEmail">发送人邮箱</param>
        /// <param name="SmtpUserNamme">发送人邮箱用户名</param>
        /// <param name="SmtpPassword">发送人邮箱密码</param>
        /// <param name="EmailHost">邮件服务器</param>
        /// <returns>是否发送成功</returns>
        public string GetPass_SendEmail(string AccountID, string LoginId, string Email, string Url, string SendEmail, string SmtpUserNamme, string SmtpPassword, string EmailHost)
        {
            try
            {
                SendMail SendMail = new SendMail();
                SendMail.Host = EmailHost;                                //邮件服务器
                SendMail.SmtpUsername = SmtpUserNamme;                    //发送人邮箱用户名
                SendMail.SendEmail = SendEmail;                           //发送人邮箱
                SendMail.SmtpPassword = SmtpPassword;                     //发送人邮箱密码
                SendMail.Port = 25;                                       //服务器端口
                SendMail.ReplyToEmail = Email;                            //回复人邮箱帐号
                SendMail.ReplyUserName = LoginId;                         //回复人用户名
                SendMail.GetEmail = Email;                                //收件人邮箱帐号


                #region 传输参数

                Random random = new Random();
                int RandomValue1 = random.Next(1, 10000);
                int RandomValue2 = random.Next(1, 10000);

                string X1 = DESEncryptTools.DESEncrypt(RandomValue1.ToString());     //干扰参数
                string X2 = DESEncryptTools.DESEncrypt(AccountID);                   //帐号ID
                string X3 = DESEncryptTools.DESEncrypt(LoginId);                     //登录ID
                string X4 = DESEncryptTools.DESEncrypt(RandomValue2.ToString());     //干扰参数
                string X5 = DESEncryptTools.DESEncrypt(DateTime.Now.ToString());     //过期时间

                //邮件标题
                SendMail.Title = "亲爱的" + LoginId + "：您的密码找回通知";

                #endregion
                // 邮件内容
                StringBuilder sb = new StringBuilder();
                sb.Append("<br/><b>亲爱的" + LoginId + "：</b><br>&nbsp;&nbsp;&nbsp;&nbsp;您好！<br/>&nbsp;&nbsp;&nbsp;&nbsp;点击如下链接地址修改密码：<br>");
                sb.Append("<br>&nbsp;&nbsp;&nbsp;&nbsp;<a href='" + Url + "/Help/GetPassUrl.aspx?X1=" + X1 + "&X2=" + X2 + "&X3=" + X3 + "&X4=" + X4 + "&X5=" + X5 + "' target='_blank' title='点击链接进行修改'>");
                sb.Append(Url + "/Help/GetPassUrl.aspx?X1=" + X1 + "&X2=" + X2 + "&X3=" + X3 + "&X4=" + X4 + "&X5=" + X5 + "</a><br/>");
                sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;<font color=red>如果点击打不开，请将网址复制后输入浏览地址栏进行操作！</font><br><br>&nbsp;&nbsp;&nbsp;&nbsp;<b>本链接地址24小时内有效</b>");
                SendMail.Content = sb.ToString();

                //发送邮件
                if (SendMail.Send())
                {
                    //返回处理结果
                    string result = "";
                    switch (Email.Substring(Email.LastIndexOf("@") + 1, Email.LastIndexOf(".") - Email.LastIndexOf("@") - 1).ToLower())
                    {
                        case "sina":
                            result = "http://mail.sina.com.cn/";
                            break;
                        case "163":
                            result = "http://email.163.com/";
                            break;
                        case "qq":
                            result = "https://mail.qq.com/cgi-bin/loginpage";
                            break;
                        case "sohu":
                            result = "http://mail.sohu.com/";
                            break;
                        default:
                            break;
                    }
                    return result;
                }
                else
                    return "false";
            }
            catch { return "false"; }
        }
        #endregion
        
        public Member GetMemberByAccountId(string AccountId)
        {
            MySqlParameter[] parm = { new MySqlParameter("@AccountId", AccountId) };
            Member model = base.GetModel("AccountId=@AccountId",parm);
            return model;
        }
    } 
}
