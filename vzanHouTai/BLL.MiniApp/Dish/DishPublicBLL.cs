using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Weixin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Dish
{
    public class DishPublicBLL
    {
        #region 单例模式
        private static DishPublicBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishPublicBLL()
        {

        }

        public static DishPublicBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishPublicBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public string GetOpenId(string utoken)
        {
            string openId = string.Empty;
            if (string.IsNullOrEmpty(utoken))
            {
                return openId;
            }
            string rediskey = $"LoginSessionOpenIdKey_{utoken}";
            UserSession userSession = RedisUtil.Get<UserSession>(rediskey);
            if (userSession != null)
            {
                openId = userSession.openid;
            }
            return openId;
        }

        public C_UserInfo GetUserInfo(string utoken)
        {
            C_UserInfo userInfo = null;
            if (string.IsNullOrEmpty(utoken))
            {
                return userInfo;
            }
            string rediskey = $"LoginSessionOpenIdKey_{utoken}";
            UserSession userSession = RedisUtil.Get<UserSession>(rediskey);
            if (userSession == null)
            {
                return userInfo;
            }
            userInfo = C_UserInfoBLL.SingleModel.GetModelFromCache(userSession.openid);
            return userInfo;
        }
    }
    //public class UserSession
    //{
    //    public string openid { get; set; }
    //    public string unionid { get; set; }
    //    public string session_key { get; set; }
    //    public string code { get; set; }
    //    public string vector { get; set; }
    //    public bool verify()
    //    {
    //        return !(string.IsNullOrWhiteSpace(openid) || string.IsNullOrWhiteSpace(session_key) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(vector));
    //    }
    //    public string enData { get; set; }
    //    public string deData { get; set; }
    //    public string signature { get; set; }
    //}
}
