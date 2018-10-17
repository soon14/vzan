using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Plat;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{
    public class PlatMsgCommentBLL : BaseMySql<PlatMsgComment>
    {
        #region 单例模式
        private static PlatMsgCommentBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatMsgCommentBLL()
        {

        }

        public static PlatMsgCommentBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatMsgCommentBLL();
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
        /// <param name="DataType"></param>
        /// <param name="keyMsg"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="Id">帖子Id 可空 如果大于0则表示是获取某条帖子的</param>
        /// <param name="userId">用户Id 获取用户帖子所属评论</param>
        /// <returns></returns>
        public List<PlatMsgComment> GetPlatMsgComment(int aid, out int totalCount, int DataType = 0, string keyMsg = "", int pageSize = 10, int pageIndex = 1, int Id = 0, int userId = 0)
        {
            string strWhere = $"aid={aid} and State=0 and DataType={DataType}  ";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (userId > 0)
            {
                strWhere += $"  and UserId={userId} ";
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
            //  log4net.LogHelper.WriteInfo(this.GetType(),strWhere);
            totalCount = base.GetCount(strWhere, parameters.ToArray());
            
            List<PlatMsgComment> listPlatMsgComment = base.GetListByParam(strWhere, parameters.ToArray(), pageSize, pageIndex,"*"," AddTime Desc");
            if (listPlatMsgComment != null && listPlatMsgComment.Count > 0)
            {
                listPlatMsgComment.ForEach(x =>
                {
                    PlatMyCard platMyCard = PlatMyCardBLL.SingleModel.GetModelByUserId(x.UserId, aid);
                    if (platMyCard != null)
                    {
                        x.NickName = platMyCard.Name;
                        x.HeaderImg = platMyCard.ImgUrl;
                    }
                    else
                    {
                        C_UserInfo c_UserInfoFrom = C_UserInfoBLL.SingleModel.GetModel(x.UserId);
                        if (c_UserInfoFrom != null)
                        {
                            x.NickName = c_UserInfoFrom.NickName;
                            x.HeaderImg = c_UserInfoFrom.HeadImgUrl;
                        }
                      
                    }

                    PlatMyCard platMyCardTo = PlatMyCardBLL.SingleModel.GetModel(x.ToUserId);
                    if (platMyCardTo != null)
                    {
                        x.ToNickName = platMyCardTo.Name;
                        x.ToHeaderImg = platMyCardTo.ImgUrl;
                    }
                    else
                    {
                        C_UserInfo c_UserInfo = C_UserInfoBLL.SingleModel.GetModel(x.ToUserId);
                        if (c_UserInfo != null)
                        {
                            x.NickName = c_UserInfo.NickName;
                            x.HeaderImg = c_UserInfo.HeadImgUrl;
                        }
                    }

                    PlatMsg platMsg = PlatMsgBLL.SingleModel.GetModel(x.MsgId);
                    if (platMsg != null)
                    {
                        x.MsgTxt = platMsg.MsgDetail.Length > 20 ? platMsg.MsgDetail.Substring(0, 20) + "..." : platMsg.MsgDetail;
                        x.MsgFirstImg = platMsg.Imgs == null ? "" : platMsg.ImgList.FirstOrDefault();
                    }


                    x.ShowTimeStr = CommondHelper.GetTimeSpan(DateTime.Now - x.AddTime);

                });
            }

            return listPlatMsgComment;


        }

        /// <summary>
        /// 用户评论数量
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public int GetUserContentCount(long userid)
        {
            string sqlwhere = $"userid = {userid}";
            return base.GetCount(sqlwhere);
        }

        public List<PlatMsgComment> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<PlatMsgComment>();
            return base.GetList($"id in ({ids})");
        }

    }
}
