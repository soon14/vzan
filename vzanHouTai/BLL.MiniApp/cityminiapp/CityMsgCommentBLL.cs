using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.cityminiapp;
using Entity.MiniApp.Plat;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.cityminiapp
{
    public class CityMsgCommentBLL : BaseMySql<CityMsgComment>
    {
        #region 单例模式
        private static CityMsgCommentBLL _singleModel;
        private static readonly object SynObject = new object();

        private CityMsgCommentBLL()
        {

        }

        public static CityMsgCommentBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CityMsgCommentBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 获取帖子评论列表
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="totalCount"></param>
        /// <param name="keyMsg"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="Id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<CityMsgComment> GetCityMsgComment(int aid, out int totalCount, string keyMsg = "", int pageSize = 10, int pageIndex = 1, int Id = 0, int userId = 0)
        {
            string strWhere = $"aid={aid} and State=0 ";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (userId > 0)
            {
                strWhere += $"  and FromUserId={userId} ";
            }

            if (Id > 0)
            {
                strWhere += $"  and MsgId={Id}";
            }

            if (!string.IsNullOrEmpty(keyMsg))
            {
                strWhere += $" and CommentDetail like @keyMsg ";
                parameters.Add(new MySqlParameter("keyMsg", $"%{keyMsg}%"));
            }

            totalCount = base.GetCount(strWhere, parameters.ToArray());
            
            List<CityMsgComment> listCityMsgComment = base.GetListByParam(strWhere, parameters.ToArray(), pageSize, pageIndex,"*"," AddTime Desc");
            if (listCityMsgComment != null && listCityMsgComment.Count > 0)
            {
                string fuserIds = string.Join(",",listCityMsgComment.Select(s=>s.FromUserId));
                string tuserIds = string.Join(",", listCityMsgComment.Select(s => s.ToUserId));
                if(!string.IsNullOrEmpty(tuserIds))
                {
                    if(!string.IsNullOrEmpty(fuserIds))
                    {
                        fuserIds += "," + tuserIds;
                    }
                    else
                    {
                        fuserIds = tuserIds;
                    }
                }
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(fuserIds);

                string cityMsgIds = string.Join(",",listCityMsgComment.Select(s=>s.MsgId));
                List<CityMsg> cityMsgList = CityMsgBLL.SingleModel.GetListByIds(cityMsgIds);

                listCityMsgComment.ForEach(x =>
                {
                    C_UserInfo c_userInfo = userInfoList?.FirstOrDefault(f=>f.Id == x.FromUserId);
                    if (c_userInfo != null)
                    {
                        x.NickName = c_userInfo.NickName;
                        x.HeaderImg = c_userInfo.HeadImgUrl;
                    }
                    else
                    {
                        x.NickName = "匿名";
                    }
                    
                    C_UserInfo to_userInfo = userInfoList?.FirstOrDefault(f=>f.Id == x.ToUserId);
                    if (to_userInfo != null)
                    {
                        x.ToNickName = to_userInfo.NickName;
                        x.ToHeaderImg = to_userInfo.HeadImgUrl;
                    }

                    CityMsg cityMsg = cityMsgList?.FirstOrDefault(f=>f.Id == x.MsgId);
                    if (cityMsg != null)
                    {
                        x.MsgTxt = cityMsg.msgDetail.Length > 20 ? cityMsg.msgDetail.Substring(0, 20) + "..." : cityMsg.msgDetail;
                        x.MsgFirstImg = cityMsg.imgList.FirstOrDefault();
                    }
                    
                    x.ShowTimeStr = CommondHelper.GetTimeSpan(DateTime.Now - x.AddTime);

                });
            }

            return listCityMsgComment;
        }

        /// <summary>
        /// 用户评论数量
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public int GetUserContentCount(long userid)
        {
            string sqlwhere = $"FromUserId = {userid}";
            return base.GetCount(sqlwhere);
        }

        public List<CityMsgComment> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<CityMsgComment>();
            return base.GetList($"id in ({ids})");
        }

    }
}
