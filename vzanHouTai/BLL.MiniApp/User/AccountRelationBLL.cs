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
    public class AccountRelationBLL : BaseMySql<AccountRelation>
    {
        #region 单例模式
        private static AccountRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private AccountRelationBLL()
        {

        }

        public static AccountRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AccountRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public AccountRelation GetModelByAccountId(string accountid)
        {
            AccountRelation model = base.GetModel($"AccountId='{accountid}'"); 
            if(model==null)
            {
                model = new AccountRelation();
                model.AccountId = accountid;
                Member member = MemberBLL.SingleModel.GetMemberByAccountId(accountid);
                if(member==null)
                {
                    return model;
                }
                model.AddTime = member.CreationDate;
                base.Add(model);
            }
            return model;
        }
    }
}