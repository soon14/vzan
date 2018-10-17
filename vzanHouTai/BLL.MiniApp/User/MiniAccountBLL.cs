using DAL.Base;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using Utility;

namespace BLL.MiniApp
{
    public class MiniAccountBLL : BaseMySql<MiniAccount>
    {
        private static string dzlogin = "";
        /// <summary>
        /// 开通多门店时初始化店铺数据
        /// </summary>
        /// <param name="scount">开通门店数量</param>
        /// <param name="accountid">用户accountid</param>
        /// <param name="rid">模板权限ID</param>
        /// <returns></returns>
        public List<string> GetAddMiniAccountSQL(int scount,string accountid,int rid,int storeid=0)
        {
            List<string> sqllist = new List<string>();
            MiniAccount miniaccount = new MiniAccount();
            if (scount >0)
            {
                if(storeid==0)
                {
                    miniaccount = new MiniAccount()
                    {
                        AccountId = accountid,
                        ParentId = storeid,
                        TemplateId = rid,
                        LoginId = "",
                        CreationDate = DateTime.Now
                    };
                    storeid = Convert.ToInt32(Add(miniaccount));
                }

                dzlogin = "dz" + DateTime.Now.ToString("yyyyMMddHHmmss");
                miniaccount.LoginId = dzlogin;
                var pwd = DESEncryptTools.GetMd5Base32("123456");
                for (int i = 0; i < scount; i++)
                {
                    miniaccount = new MiniAccount()
                    {
                        AccountId = accountid,
                        ParentId = storeid,
                        TemplateId = rid,
                        LoginId = GetUserLoginId(miniaccount.LoginId, dzlogin),
                        Password = pwd,
                        DayBegin = DateTime.Now,
                        DayEnd = DateTime.Now.AddYears(1),
                        CreationDate = DateTime.Now
                    };

                    sqllist.Add(BuildAddSql(miniaccount));
                }
            }

            return sqllist;
        }

        /// <summary>
        /// 获取分店用户登陆ID
        /// </summary>
        /// <param name="d"></param>
        /// <param name="number">后面数字几位数</param>
        /// <returns></returns>
        public string GetUserLoginId(string d,string dzlogin,int number=4)
        {
            string loginid = d;
            string temp = d.Replace(dzlogin,"");
            if (!string.IsNullOrEmpty(temp))
            {
                int temp2 = 0;
                loginid = int.TryParse(temp, out temp2) ? dzlogin + (temp2+1) : "";
            }
            else
            {
                int sum = Convert.ToInt32(Math.Pow(10,4));
                loginid = dzlogin + sum;
            }
            
            return loginid;
        }
    }
}