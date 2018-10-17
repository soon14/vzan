using BLL.MiniApp.Im;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Im;
using Entity.MiniApp.Qiye;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Qiye
{
    public class QiyeUserFavoriteMsgBLL : BaseMySql<QiyeUserFavoriteMsg>
    {
        #region 单例模式
        private static QiyeUserFavoriteMsgBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyeUserFavoriteMsgBLL()
        {

        }

        public static QiyeUserFavoriteMsgBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyeUserFavoriteMsgBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 我点赞，关注，收藏总数量
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public int GetMyMsgCount(long userId, int actionType, int dataType)
        {
            string sqlwhere = $"userid={userId} and state>=0 and actiontype={actionType} and datatype={dataType}";
            return base.GetCount(sqlwhere);
        }

        /// <summary>
        /// 店铺，帖子，名片点赞，关注，收藏总量
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="actionType"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public int GetUserMsgCount(int aid, int msgId, int actionType, int dataType, int userId = 0)
        {
            string sqlWhere = $"aid={aid} and state>=0 and actiontype={actionType} and datatype={dataType}";
            if (msgId > 0)
            {
                sqlWhere += $" and msgid={msgId}";
            }
            if (userId > 0)
            {
                sqlWhere += $" and userid={userId}";
            }
            return base.GetCount(sqlWhere);
        }

        /// <summary>
        /// 获取一条收藏或者点赞记录
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="msgId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public QiyeUserFavoriteMsg GetUserFavoriteMsg(int aid, int msgId, int userId, int actionType = 0, int dataType = (int)PointsDataType.帖子)
        {
            QiyeUserFavoriteMsg model = base.GetModel($"aid={aid} and userId={userId} and msgId={msgId} and actionType={actionType} and datatype={dataType}");
            return model;
        }

        public List<QiyeUserFavoriteMsg> GetUserFavoriteMsgList(int aid, string msgIds, int userId, int dataType = (int)PointsDataType.帖子)
        {
            return base.GetList($"aid={aid} and userId={userId} and msgId in ({msgIds})  and datatype={dataType}");
        }
        
        public string CommondFavoriteMsg(int aid, int otherCardId, int userId, int actionType, int dataType, ref int curState)
        {
            int count = 1;
            
            QiyeUserFavoriteMsg qiyeUserFavoriteMsgModel = GetUserFavoriteMsg(aid, otherCardId, userId, actionType, dataType);
            if (qiyeUserFavoriteMsgModel == null)
            {
                qiyeUserFavoriteMsgModel = new QiyeUserFavoriteMsg();
                qiyeUserFavoriteMsgModel.ActionType = actionType;
                qiyeUserFavoriteMsgModel.AddTime = DateTime.Now;
                qiyeUserFavoriteMsgModel.AId = aid;
                qiyeUserFavoriteMsgModel.Datatype = dataType;
                qiyeUserFavoriteMsgModel.MsgId = otherCardId;
                qiyeUserFavoriteMsgModel.State = 0;
                qiyeUserFavoriteMsgModel.UserId = userId;
            }
            else if (qiyeUserFavoriteMsgModel.State == -1)
            {
                qiyeUserFavoriteMsgModel.State = 0;
            }
            else if (qiyeUserFavoriteMsgModel.State == 0)//取消
            {
                qiyeUserFavoriteMsgModel.State = -1;
                count = -1;
            }
            curState = qiyeUserFavoriteMsgModel.State;
            QiyeMsgviewFavoriteShare platMsgViewModel = QiyeMsgviewFavoriteShareBLL.SingleModel.GetModelByMsgId(aid, otherCardId, dataType);
            if (platMsgViewModel == null)
            {
                platMsgViewModel = new QiyeMsgviewFavoriteShare();
                platMsgViewModel.AddTime = DateTime.Now;
                platMsgViewModel.AId = aid;
                platMsgViewModel.DataType = dataType;
                platMsgViewModel.DzCount = 0;
                platMsgViewModel.FavoriteCount = 0;
                platMsgViewModel.FollowCount = 0;
                platMsgViewModel.MsgId = otherCardId;
                platMsgViewModel.ShareCount = 0;
                platMsgViewModel.SiXinCount = 0;
                platMsgViewModel.ViewCount = 0;
                platMsgViewModel.Id = Convert.ToInt32(QiyeMsgviewFavoriteShareBLL.SingleModel.Add(platMsgViewModel));
            }

            switch (actionType)
            {
                case (int)PointsActionType.关注: platMsgViewModel.FollowCount += count; break;
                case (int)PointsActionType.收藏: platMsgViewModel.FavoriteCount += count; break;
                case (int)PointsActionType.点赞: platMsgViewModel.DzCount += count; break;
                case (int)PointsActionType.看过: platMsgViewModel.ViewCount += 1; break;
                case (int)PointsActionType.私信: platMsgViewModel.SiXinCount += 1; break;
                case (int)PointsActionType.转发: platMsgViewModel.ShareCount += 1; break;
            }

            QiyeMsgviewFavoriteShareBLL.SingleModel.Update(platMsgViewModel);
            if (qiyeUserFavoriteMsgModel.Id > 0)
            {
                if (actionType != (int)PointsActionType.转发 && actionType != (int)PointsActionType.看过 && actionType != (int)PointsActionType.私信)
                {
                    qiyeUserFavoriteMsgModel.AddTime = DateTime.Now;
                    base.Update(qiyeUserFavoriteMsgModel, "State,AddTime");
                }
            }
            else
            {
                qiyeUserFavoriteMsgModel.Id = Convert.ToInt32(base.Add(qiyeUserFavoriteMsgModel));
                if (qiyeUserFavoriteMsgModel.Id <= 0)
                {
                    return "操作失效";
                }
            }

            return "";
        }

        /// <summary>
        /// 删除用户收藏的记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public bool DelMolde(int id, int userId, int appId)
        {
            QiyeUserFavoriteMsg model = base.GetModel($"Id={id} and userId={userId} and aid={appId} and actionType={(int)PointsActionType.收藏}");
            if (model == null)
                return false;
            model.State = -1;
            return base.Update(model, "State");
        }


        /// <summary>
        /// 获取用户相关动作记录列表
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="userId"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="dataType"></param>
        /// <param name="actionType"></param>
        /// <param name="actionType2"></param>
        /// <returns></returns>
        public List<QiyeUserFavoriteMsg> GetUserFavoriteMsgList(int aid, int userId, out int totalCount, int pageIndex = 1, int pageSize = 10,  int dataType = (int)PointsDataType.帖子, int actionType = 0, int actionType2 = 0)
        {
            totalCount = 0;
            string sqlWhere = $"aid={aid} and userId={userId}  and datatype={dataType} and State <>-1 and actionType in ({actionType},{actionType2})";
            List<QiyeUserFavoriteMsg> list = base.GetList(sqlWhere, pageSize, pageIndex);
           
            if (list != null || list.Count > 0)
            {
                totalCount = base.GetCount(sqlWhere);
            }
           // log4net.LogHelper.WriteInfo(this.GetType(), $"sql={sqlWhere};cout={list.Count}");
            return list;

        }


    }
}
