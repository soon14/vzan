using DAL.Base;
using Entity.MiniApp;
using System;

namespace BLL.MiniApp
{
    public class Base_OAuthUserBLL : BaseMySql<Base_OAuthUser>
    {
        public Base_OAuthUserBLL(int fid)
        {
            base.TableName = GetTableName(fid.ToString());
        }
        public string GetTableName(string MinisnsId)
        {
            return "OAuthUser_" + MinisnsId.Substring(MinisnsId.Length - 1, 1);
        }
        /// <summary>
        /// 从缓存读取用户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Base_OAuthUser GetUserByCache(int userId)
        {
            string strKey = string.Format(MemCacheKey.BaseUser_key, userId);
            Base_OAuthUser model = RedisUtil.Get<Base_OAuthUser>(strKey);
            if (model == null)
            {
                model = base.GetModel(userId);
                RedisUtil.Set<Base_OAuthUser>(strKey, model, TimeSpan.FromHours(3));
            }
            return model;
        }
       
    }
}
