using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;

using Entity.MiniApp;
using Utility;
using Utility.MemberLogin;  
using DAL.Base;
using System.Transactions;
//using BLL.MiniApp; 
using Entity.MiniApp.User;

namespace Core.MiniApp
{
    public class AccountCore
    {
        //private AccountBLL bllAccount = new AccountBLL();

        //public bool MakeAgent(Guid AccountId)
        //{
        //    Account account = bllAccount.GetModel(AccountId);
        //    if(account!=null)
        //    {
        //        account.MemberStatus = 1;//--将用户变成代理
        //        bllAccount.Update(account);
        //        return true;
        //    }
        //    return false;
        //}

        /// <summary>
        /// 鹏讯官网注册，手机，邮箱，注册成功默认绑定手机和邮箱，传入的account 不加密
        /// 密码加密，帐号不加密
        /// </summary>
        /// <param name="account">account实体</param>
        /// <param name="nickName">第三方登陆注册 用户昵称 可为空</param>
        /// <returns>返回参数 1=注册成功，2=已有相同名称 注册失败</returns>
        public bool RegisterWhole(Account account)
        {
            try
            {
                return true;
            }
            catch (Exception )
            {
                return false;//异常记录
            }
        }


        //public Account CustomerRegister(string password)
        //{
        //    Account account = new Account();
        //    //--生成随机唯一登录ID
        //    account.LoginId = bllAccount.GenerateRandomLoginId("vzan", 8);
        //    account.Password = LixSecurity.MD5(password);
        //    new AccountBLL().Add(account);//添加用户账户表
        //    return account;
        //}
    }
}