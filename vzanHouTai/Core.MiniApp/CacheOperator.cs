using System;
using System.Collections.Generic;
using DAL.Base;
using Entity.MiniApp; 

namespace Core.MiniApp
{
    public class CacheOperator
    {
        //public static void RemovUserRightCache(int userId, int typeId)
        //{
        //    if (userId > 0)
        //    {
        //        RedisUtil.Remove(string.Format(MemCacheKey.UserRight_userId_Key, userId, typeId));
        //    }
        //}
        //public static WxAuthorize GetAccessTokenFromCache(string code)
        //{
        //    if (!string.IsNullOrEmpty(code))
        //        return RedisUtil.Get<WxAuthorize>(code);
        //    return null;
        //}
        //public static void SetAccessToken(string code, WxAuthorize token)
        //{
        //    if (!string.IsNullOrEmpty(code)&&token!=null && !string.IsNullOrEmpty(token.openid))
        //        RedisUtil.Set(code, token, TimeSpan.FromSeconds(10));
        //}
         
        //public static void Remolistemoji()
        //{
        //    RedisUtil.Remove(MemCacheKey.emojiKey);
        //}

        //public static void RemoveHotArticle(int fId)
        //{
        //    string key = string.Format(MemCacheKey.HotArticleKey, fId, DateTime.Now.ToString("yyyyMMdd"));
        //    RedisUtil.Remove(key);
        //}
    }
}
