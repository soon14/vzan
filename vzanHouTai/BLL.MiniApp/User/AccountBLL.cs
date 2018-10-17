using BLL.MiniApp.Conf;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Utility;
/// <summary>
/// Member转移过来的
/// </summary>
namespace BLL.MiniApp
{
    public class AccountBLL : BaseMySql<Account>
    {
        #region 单例模式
        private static AccountBLL _singleModel;
        private static readonly object SynObject = new object();

        private AccountBLL()
        {

        }

        public static AccountBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AccountBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 获取管理员账户信息  *** SQL注入
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Account GetAccountByCache(string Id)
        {
            Guid tempGuid = Guid.NewGuid();
            if (!Guid.TryParse(Id, out tempGuid))
            {//参数验证，防止SQL注入
                return null;
            }
            Account model = RedisUtil.Get<Account>(Id);
            if (model == null)
            {
                model = base.GetModel(string.Format(" Id='{0}'", Id));
                RedisUtil.Set<Account>(Id, model, TimeSpan.FromHours(6));
            }
            return model;
        }

        public override Account GetModel(Guid Id)
        {
            Account model = GetAccountByCache(Id.ToString());
            if (model == null)
            {
                model = base.GetModel(string.Format(" Id='{0}'", Id));
                RedisUtil.Set<Account>(Id.ToString(), model, TimeSpan.FromHours(6));
            }
            return base.GetModel(Id);
        }

        public override bool Update(Account model)
        {
            RedisUtil.Remove(model.Id.ToString());
            return base.Update(model);
        }
        public override bool Update(Account model, string columnFields)
        {
            RedisUtil.Remove(model.Id.ToString());
            return base.Update(model, columnFields);
        }

        public List<Account> GetListByAccoundId(string accoundids)
        {
            if (string.IsNullOrEmpty(accoundids))
                return new List<Account>();

            return GetList($"Id in ({accoundids})");
        }

        #region 鹏讯官网的用户中心登录注册

        /// <summary>
        /// 用户注册用户是否存在，手机，邮箱，不加密
        /// </summary>
        /// <param name="regName">用户的注册名</param>
        /// <returns>返回注册名是否存在</returns>
        public bool IsRegNameWhole(string regName)
        {
            MySqlParameter[] paras = { new MySqlParameter("@loginName", regName) };

            if (StringHelper.IsMobile(regName))//--手机
                return GetModel(" ConsigneePhone=@loginName ", paras) != null;
            else if (StringHelper.IsEmail(regName))//--邮箱
                return GetModel(" EMail=@loginName ", paras) != null;
            else//--鹏讯号
                return GetModel(" LoginId=@loginName", paras) != null;
        }

        /// <summary>
        /// 鹏讯官网用户登录 传输过来的数据不要经过任何加密
        /// </summary>
        /// <param name="LoginName">用户名</param>
        /// <param name="PassWord">用户密码</param>
        /// <param name="memberType">用户类型</param>
        /// <returns>返回用户实体</returns>
        public Account LoginUserWhole(string LoginName, string PassWord)
        {


            try
            {
                MySqlParameter[] paras = new MySqlParameter[]{
                                 new MySqlParameter("@loginName",LoginName),
                                 new MySqlParameter("@ConsigneePhone",LoginName),
                                 new MySqlParameter("@Password",DESEncryptTools.GetMd5Base32(PassWord))
                                //  new MySqlParameter("@Password",PassWord)
                };
                //---检测 手机/邮箱 
                  Account account = base.GetModel(" (LoginId=@loginName || ConsigneePhone=@ConsigneePhone) and Password=@Password and Status=1 ", paras);
             //   Account account = base.GetModel(" (LoginId=@loginName || ConsigneePhone=@ConsigneePhone) and Password='c64fee0ff27c562e4d90d54b3a2c45ce' and Status=1 ", paras);//测试优惠券同步到微信卡包账号
                return account;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 生成随机登录ID
        /// </summary>
        /// <param name="preFix"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GenerateRandomLoginId(string preFix, int length)
        {
            string loginId = string.Empty;
            loginId += preFix;
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                loginId += random.Next(0, 10);
            }
            if (!IsRegNameWhole(loginId))
            {
                return loginId;
            }
            else
            {
                return GenerateRandomLoginId(preFix, length);
            }
        }

        #endregion
        
        public void updateUnionId(Guid accountId, string unionId)
        {
            Account model = GetModel(accountId);
            model.UnionId = unionId;
            Update(model, "UnionId");
        }

        public Account GetModelByLoginid(string loginid)
        {
            MySqlParameter[] paras = { new MySqlParameter("@LoginId", loginid) };
            return GetModel(" LoginId=@LoginId", paras);
        }
        public List<Account> GetModelByLoginidL(string loginid)
        {
            MySqlParameter[] paras = { new MySqlParameter("@LoginId", "%" + loginid + "%") };
            return GetListByParam(" LoginId like @LoginId", paras);
        }

        public List<Account> GetListByPhones(string phones)
        {
            if (string.IsNullOrEmpty(phones))
                return new List<Account>();
            return GetList($" ConsigneePhone in ({phones})");
        }

        public Account GetModelByPhone(string phone)
        {
            MySqlParameter[] paras = { new MySqlParameter("@ConsigneePhone", phone) };
            return GetModel(" ConsigneePhone=@ConsigneePhone", paras);
        }
        public List<Account> GetListByAccountids(string accountids,string loginid="")
        {
            if (accountids==null || accountids=="")
                return new List<Account>();

            string sqlwhere = $"id in ({accountids})";
            List<MySqlParameter> paras = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(loginid))
            {
                sqlwhere += $" and LoginId like @LoginId";
                paras.Add( new MySqlParameter("@LoginId", "%" + loginid + "%"));
            }

            return base.GetListByParam(sqlwhere,paras.ToArray());
        }
        
        /// <summary>
        /// 手机操作
        /// </summary>
        /// <param name="dzuserId"> 用户guid</param>
        /// <param name="type">1=修改绑定手机，2=添加绑定手机，3=解除绑定手机 </param>
        /// <param name="phone"></param>
        /// <returns></returns>
        public Return_Msg ChangePhone(string dzuserId, int type, string phone)
        {
            var msg = new Return_Msg();
            msg.Msg = "成功";

            Account account = GetModel($"id='{dzuserId}'");
            if (account == null)
            {
                msg.isok = false;
                msg.Msg = "账号出错，请刷新重试！";
                return msg;
            }
            //修改绑定手机
            if (type == 2)
            {
                var tempaccount = GetModel($"ConsigneePhone='{phone}'");
                if (tempaccount != null)
                {
                    if (tempaccount.Id.ToString() != dzuserId)
                    {
                        msg.isok = false;
                        msg.Msg = "该手机号已被别人绑定！";
                        return msg;
                    }
                }

                //修改登录用户帐号的电话
                account.ConsigneePhone = phone;
                account.Status = true;
                msg.isok = Update(account, "ConsigneePhone,Status");

                msg.Msg = msg.isok ? "修改成功！" : "修改失败";
                return msg;
            }
            //添加绑定手机
            else if (type == 3)
            {
                var tempaccount = GetModel($"ConsigneePhone='{phone}'");
                if (tempaccount != null)
                {
                    msg.isok = false;
                    msg.Msg = "该手机号已被别人绑定！";
                    return msg;
                }

                //绑定操作
                //修改登录用户帐号的电话
                account.ConsigneePhone = phone;
                account.Status = true;
                msg.isok = Update(account, "ConsigneePhone,Status");
                msg.Msg = "绑定成功！";
            }
            else if (type == 4)
            {
                //修改登录用户帐号的电话
                account.ConsigneePhone = "";
                account.Status = true;
                Update(account, "ConsigneePhone,Status");
                msg.Msg = "解除绑定成功！";
                msg.isok = true;
            }
            return msg;
        }
        
        public Account WeiXinRegister(string OpenId, int usertype, string Unionid = null, bool ismobilereg = false, string address = "", string phone = "", string sourcefrom = "", string password = "123456")
        {
            Account account = null;

            if ((Unionid != null && !string.IsNullOrEmpty(Unionid)) || ismobilereg)
            {
                account = GetModel(string.Format(" Unionid='{0}' ", Unionid));
                //Account account = bllAccount.GetModel(string.Format(" OpenId='{0}' ", OpenId));
                if (account != null && !ismobilereg)
                    return account;
                account = new Account();
                account.OpenId = OpenId;
                account.UnionId = Unionid;
                account.ConsigneePhone = phone;
                ////判断是否是普通用户注册，如果是0普通用户注册，则必须要验证手机号码，才能启用账号
                //account.Status = usertype == 1 ? false : true;
                //--生成随机唯一登录ID
                account.LoginId = GenerateRandomLoginId("vzan", 8);
                //---密码加密MD5
                account.Password = DESEncryptTools.GetMd5Base32(password);
                Member member = new Member()
                {
                    AccountId = account.Id
                };
                member.ConsigneePhone1 = phone;
                member.CompanyRemark = address;
                member.Avatar = sourcefrom;
                //--随机唯一登录ID 座位默认昵称
                member.MemberName = account.LoginId;
                base.Add(account);//添加用户账户表
                MemberBLL.SingleModel.Add(member);//添加用户信息表 
                //建立关联
                AccountRelation accountrelaton = new AccountRelation();
                accountrelaton.AccountId = account.Id.ToString();
                accountrelaton.AddTime = DateTime.Now;
                AccountRelationBLL.SingleModel.Add(accountrelaton);
            }
            return account;
        }

        /// <summary>
        /// 用户扫描代理分销二维码进行注册时，有可能已存在账号，该方法是修改用户一些信息
        /// </summary>
        /// <param name="accountid"></param>
        /// <param name="phone"></param>
        /// <param name="password"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool UpdateUserInfo(string accountid, string phone, string password, string address)
        {
            TransactionModel tran = new TransactionModel();
            string passwordMd = DESEncryptTools.GetMd5Base32(password);
            Member membermodel = MemberBLL.SingleModel.GetMemberByAccountId(accountid);

            MySqlParameter[] param = new MySqlParameter[] {
                new MySqlParameter("@ConsigneePhone",phone),
                new MySqlParameter("@Password",passwordMd),
                new MySqlParameter("@id",accountid),
            };
            tran.Add($"update Account set ConsigneePhone=@ConsigneePhone,Password=@Password where id=@id", param);

            if (membermodel != null)
            {
                MySqlParameter[] param2 = new MySqlParameter[] {
                new MySqlParameter("@ConsigneePhone1",phone),
                new MySqlParameter("@CompanyRemark",address),
                new MySqlParameter("@AccountId",accountid),
                };
                tran.Add($"update Member set ConsigneePhone1=@ConsigneePhone1,CompanyRemark=@CompanyRemark where AccountId=@AccountId", param2);
            }

            return base.ExecuteTransaction(tran.sqlArray, tran.ParameterArray);
        }

        /// <summary>
        /// 通过unionid获取account
        /// </summary>
        /// <param name="wx"></param>
        /// <returns></returns>
        public Account GetAccountByWeixinUser(WeiXinUser wx, int usertype = 0)
        {
            Account accountmodel = null;
            if (!string.IsNullOrEmpty(wx.unionid))
            {
                accountmodel = base.GetModel(string.Format("UnionId='{0}'", wx.unionid));
                if (accountmodel == null)
                {
                    //初始化新用户的默认信息,创建默认社区等
                    accountmodel = WeiXinRegister(wx.openid, usertype, wx.unionid);
                    ////注册赠送单页版
                    //if (accountmodel != null)
                    //{
                    //    AddFreeSinglePage(accountmodel);
                    //}
                }
                if (accountmodel != null && string.IsNullOrEmpty(accountmodel.UnionId))
                    updateUnionId(accountmodel.Id, wx.unionid);
            }
            return accountmodel;
        }

        public Account GetAccountByUnionId(string unionId)
        {
            return base.GetModel($"UnionId = '{unionId}'");
        }


        //public void GetUserUnionID(string openId)
        //{
        //    try
        //    {
        //        string access_token = string.Empty;
        //        if (!string.IsNullOrEmpty(openId))
        //        {
        //            string strwhere = string.Format("OpenId ='{0}' and UnionId =''", openId);
        //            List<Account> accountList = base.GetList(strwhere);
        //            foreach (Account item in accountList)
        //            {
        //                string strUrl = Core.MiniApp.WxSysConfig.User_infoURL(WxHelper.GetToken());
        //                strUrl = string.Format(strUrl, WxHelper.GetToken(), item.OpenId);
        //                HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(strUrl);
        //                hwr.Method = "get";
        //                HttpWebResponse myResponse = (HttpWebResponse)hwr.GetResponse();
        //                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
        //                string content = reader.ReadToEnd();
        //                if (!content.Contains("40013"))
        //                {
        //                    WeiXinUser model = new WeiXinUser();
        //                    model = JsonConvert.DeserializeObject<WeiXinUser>(content);
        //                    if (model != null)
        //                    {
        //                        Account account = base.GetModel(item.Id);
        //                        if (account != null)
        //                        {
        //                            if (model.subscribe == "1")
        //                            {
        //                                account.UnionId = model.unionid;
        //                                updateUnionId(account.Id, model.unionid);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log4net.LogHelper.WriteError(this.GetType(), ex);
        //    }
        //}
       
    }
}