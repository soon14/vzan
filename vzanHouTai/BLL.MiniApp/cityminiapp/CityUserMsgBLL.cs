using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.cityminiapp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.cityminiapp
{
    public class CityUserMsgBLL : BaseMySql<CityUserMsg>
    {
        #region 单例模式
        private static CityUserMsgBLL _singleModel;
        private static readonly object SynObject = new object();

        private CityUserMsgBLL()
        {

        }

        public static CityUserMsgBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CityUserMsgBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 获取用户 对应状态的消息
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="userId"></param>
        /// <param name="state">0表示未读 1表示已读</param>
        /// <returns></returns>
        public int GetCountByUserId(int aid, int userId, int state = -1)
        {
            string strWhere = $"aid={aid} and toUserId={userId}";
            if (state != -1)
            {
                strWhere += $" and state={state}";
            }
            return base.GetCount(strWhere);
        }

        public List<CityUserMsg> getListByUserId(int aid, int userId, out int totalCount, int pageSize = 10, int pageIndex = 1, string appId = "")
        {
            
            string strWhere = $"aid={aid} and toUserId={userId}";
            base.ExecuteNonQuery($"update CityUserMsg set state=1 where aid={aid} and toUserId={userId}");//将该用户未读消息全部标记为已读
            
            totalCount = base.GetCount(strWhere);
            OpenAuthorizerConfig openAuthorizerConfig = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(appId);

            List<CityUserMsg> list = base.GetList(strWhere, pageSize, pageIndex, "*", "addTime desc");

            string userIds = string.Join(",",list?.Select(s=>s.fromUserId).Distinct());
            List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

            list.ForEach(x =>
            {
                //获取用户头像 昵称
                C_UserInfo c_UserInfo = userInfoList?.FirstOrDefault(f=>f.Id == x.fromUserId);
                if (c_UserInfo != null)
                {
                    x.fromUserName = c_UserInfo.NickName;
                    x.fromUseImg = c_UserInfo.HeadImgUrl;
                }
                else
                {
                    x.fromUserName = "系统消息";
                    x.fromUseImg = openAuthorizerConfig.head_img;
                }
                x.addTimeStr = CommondHelper.GetTimeSpan(DateTime.Now - x.addTime);

            });

            return list;
        }
    }
}
