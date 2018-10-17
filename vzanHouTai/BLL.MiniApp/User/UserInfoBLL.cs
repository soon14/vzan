using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace BLL.MiniApp
{
    /// <summary>
    /// 用户基础表
    /// </summary>
    public class UserInfoBLL : BaseMySql<UserInfo>
    {
        #region 单例模式
        private static UserInfoBLL _singleModel;
        private static readonly object SynObject = new object();

        private UserInfoBLL()
        {

        }

        public static UserInfoBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new UserInfoBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public UserInfo GetModelByOpenId(string openid)
        {
            string strWhere = "openid=@openId";
            MySqlParameter[] param = { new MySqlParameter("@openId", openid) };
            return GetModel(strWhere, param);
        }

        public UserInfo GetModelByUnionId(string unionid)
        {
            string strWhere = "unionid=@unionid";
            MySqlParameter[] param = { new MySqlParameter("@unionid", unionid) };
            return GetModel(strWhere, param);
        }

        /// <summary>
        /// 从主DB查数据，保证获取正确的数据
        /// xiaowei，2016-09-17 16:12:41
        /// </summary>
        /// <returns></returns>
        public UserInfo GetModelFromMaster(string strWhere)
        {
            UserInfo model = new UserInfo();
            string strSql = "select * from UserInfo where " + strWhere;
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, strSql, null))
            {
                if (dr.Read())
                    model = GetModel(dr);
            }
            if (model.Id == 0)
                return null;
            return model;
        }

        public override object Add(UserInfo model)
        {
            object obj = null;
            try
            {
                obj = base.Add(model);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(),ex);
            }
            return obj;
        }

        /// <summary>
        /// 拿微信的信息进行添加到基础用户表
        /// </summary>
        /// <param name="weixinUser"></param>
        /// <returns></returns>
        public UserInfo addWeiXinOAuthUser(WxUser weixinUser)
        {
            UserInfo model = new UserInfo();
            model.openid = weixinUser.openid;
            model.nickname = weixinUser.nickname;
            model.headimgurl = weixinUser.headimgurl;
            model.sex = weixinUser.sex.ToString();
            model.country = weixinUser.country;
            model.city = weixinUser.city;
            model.province = weixinUser.province;
            model.unionid = weixinUser.unionid;
            model.addtime = DateTime.Now;
            model.Id= Convert.ToInt32( Add(model));
          
            return model;
        }
        public void updateWeiXinOAuthUser(UserInfo model,WxUser weixinUser)
        {
            model.nickname = weixinUser.nickname;
            model.headimgurl = weixinUser.headimgurl;
            model.sex = weixinUser.sex.ToString();
            model.country = weixinUser.country;
            model.city = weixinUser.city;
            model.province = weixinUser.province;
            model.unionid = weixinUser.unionid;
            Update(model);
        }

        /// <summary>
        /// 根据微信的用户信息 , 通过 unionid 来处理，并更新信息，没有就新增记录
        /// app 授权的openid 不是微赞 公众号的，记得更新
        /// </summary>
        /// <param name="wxUserInfo"></param>
        /// <returns></returns>
        public UserInfo RegisterBaseUserFromAPP(WxUser wxUserInfo)
        {
            if(wxUserInfo==null  || string.IsNullOrEmpty(wxUserInfo.unionid))
            {
                throw new Exception("用户信息不足,uid为空");
            }
            UserInfo model = GetModelFromMaster(string.Format("unionid='{0}'", wxUserInfo.unionid));
            if (model != null)
            {
                if ((!string.IsNullOrEmpty(wxUserInfo.nickname) && wxUserInfo.nickname != model.nickname) 
                    ||(!string.IsNullOrEmpty(wxUserInfo.headimgurl) && wxUserInfo.headimgurl != model.headimgurl)
                    )
                {//头像和名字有改动，去更新
                    model.nickname = wxUserInfo.nickname;
                    model.headimgurl = wxUserInfo.headimgurl;
                    Update(model, "nickname,headimgurl");
                }
                return model;
            }
            model = addWeiXinOAuthUser(wxUserInfo);
            return model;
        }
    }
}
