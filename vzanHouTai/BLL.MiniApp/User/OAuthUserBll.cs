using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.User;
using System;

namespace BLL.MiniApp
{
    public class OAuthUserBll : BaseMySql<OAuthUser>
    {
        public OAuthUserBll(int MinisnsId)
        {
            this.TableName = GetTableName(MinisnsId.ToString());
        }
        ///// <summary>
        ///// 获取分表的表名
        ///// </summary>
        ///// <param name="MinisnsId"></param>
        ///// <returns></returns>
        public string GetTableName(string MinisnsId)
        {
            return "OAuthUser_" + MinisnsId.Substring(MinisnsId.Length - 1, 1);
        }

        public OAuthUser GetUserByOpenId(string OpenId, int minisnsid, bool fromCache = true)
        {
            this.TableName = GetTableName(minisnsid.ToString());
            OAuthUser model = null;
            if (!fromCache)
            {
                model = base.GetModel(string.Format(" MinisnsId={1} AND Openid='{0}'", OpenId, minisnsid));
                return model;
            }
            string strKey = string.Format(MemCacheKey.User_key_OpenId_MinisnsId, OpenId, minisnsid);
            model = RedisUtil.Get<OAuthUser>(strKey);
            if (model == null)
            {
                model = base.GetModel(string.Format(" MinisnsId={1} AND Openid='{0}'", OpenId, minisnsid));
                RedisUtil.Set<OAuthUser>(strKey, model, TimeSpan.FromHours(3));
            }
            return model;
        }
        

        ///// <summary>
        ///// 从缓存读取用户信息
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <returns></returns>
        public OAuthUser GetUserByCache(int userId)
        {
            string strKey = string.Format(MemCacheKey.User_key, userId);
            OAuthUser model = null;
            try
            {
                model = RedisUtil.Get<OAuthUser>(strKey);
            }
            catch
            {
                model = null;
            }
            if (model == null)
            {
                model = GetModel(userId);
                RedisUtil.Set(strKey, model, TimeSpan.FromHours(3));
            }
            return model;
        }
    }
}