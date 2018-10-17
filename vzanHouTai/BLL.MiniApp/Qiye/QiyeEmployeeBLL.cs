using BLL.MiniApp.Im;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Qiye;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Qiye
{

    public class QiyeEmployeeBLL : BaseMySql<QiyeEmployee>
    {

        #region 单例模式
        private static QiyeEmployeeBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyeEmployeeBLL()
        {

        }

        public static QiyeEmployeeBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyeEmployeeBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 客服队列key
        /// </summary>
        public readonly string KeFuDequeueKey = "KeFuDequeue_{0}";
        /// <summary>
        /// 获取员工列表
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="totalCount"></param>
        /// <param name="searchKey"></param>
        /// <param name="wxBindState"></param>
        /// <param name="departMentId"></param>
        /// <param name="workState"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<QiyeEmployee> GetListQiyeEmployee(int aid, string appId, out int totalCount, string searchKey = "", int wxBindState = -1, int departMentId = -1, int workState = -2, int pageSize = 10, int pageIndex = 1)
        {

            string strwhere = $"aid={aid} and state<>-1 and appId='{appId}'";
            totalCount = 0;
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(searchKey))
            {
                strwhere += $" and (Name like @name or Phone like @name) ";
                parameters.Add(new MySqlParameter("@name", $"%{searchKey}%"));
            }

            if (wxBindState > -1) //微信绑定状态 -1表示全部
            {
                //0表示未绑定
                if (wxBindState == 0)
                {
                    strwhere += $" and UserId=0 ";
                }
                else
                {
                    strwhere += $" and UserId>0 ";
                }
            }

            if (departMentId > -1) //所属部门
            {
                strwhere += $" and DepartmentId={departMentId} ";
            }

            if (workState > -2) //在职状态 -2表示全部
            {
                strwhere += $" and WorkState={workState} ";
            }


            List<QiyeEmployee> list = base.GetListByParam(strwhere, parameters.ToArray(), pageSize, pageIndex);

            totalCount = base.GetCount(strwhere, parameters.ToArray());

            return list;
        }

        /// <summary>
        /// 获取当前员工数量
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public int GetQiyeEmployeeCount(int aid, string appId)
        {
            return base.GetCount($"aid={aid} and state<>-1 and appId='{appId}'");
        }

        /// <summary>
        /// 获取当前客服数量
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public int GetQiyeEmployeeKefuCount(int aid, string appId, int curEmployeeId = 0)
        {
            return base.GetCount($"aid={aid} and state<>-1 and Kefu=1 and Id<>{curEmployeeId} and appId='{appId}'");
        }

        public List<QiyeEmployee> GetListByUserIds(string userIds)
        {
            if (string.IsNullOrEmpty(userIds))
                return new List<QiyeEmployee>();

            string sqlWhere = $"userid in ({userIds}) and state>=0";
            return base.GetList(sqlWhere);
        }

        public List<QiyeEmployee> GetListByAid(int aid)
        {
            string sqlWhere = $"aid={aid} and state>=0";
            return base.GetList(sqlWhere);
        }

        /// <summary>
        /// 根据aid以及员工码workId查询该员工码是否绑定了员工
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="workId"></param>
        /// <returns></returns>
        public QiyeEmployee GetQiyeEmployeeByWorkId(int aid, string workId, string appId)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();

            string strwhere = $" WorkID=@workId and Aid={aid} and State<>-1 and WorkState<>-1 and appId=@appId";
            parameters.Add(new MySqlParameter("@workId", $"{workId}"));
            parameters.Add(new MySqlParameter("@appId", $"{appId}"));

            return base.GetModel(strwhere, parameters.ToArray());

        }

        /// <summary>
        /// 根据aid以及userId查询该员工是否绑定了名片
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public QiyeEmployee GetQiyeEmployeeByUserId(int aid, int userId, string appId)
        {
            return base.GetModel($"aid={aid} and userId={userId} and  State<>-1 and appId='{appId}'");
        }

        public QiyeEmployee GetModelByUserId(int userId)
        {
            return base.GetModel($"userid={userId}");
        }

        public string CommondFavoriteMsg(int aid, int msgId, int userId, int actionType, int dataType, ref int curState)
        {
            int count = 1;

            //记录表
            QiyeUserFavoriteMsg qiyeUserFavoriteMsgModel = QiyeUserFavoriteMsgBLL.SingleModel.GetUserFavoriteMsg(aid, msgId, userId, actionType, dataType);
            if (qiyeUserFavoriteMsgModel == null)
            {
                qiyeUserFavoriteMsgModel = new QiyeUserFavoriteMsg();
                qiyeUserFavoriteMsgModel.ActionType = actionType;
                qiyeUserFavoriteMsgModel.AddTime = DateTime.Now;
                qiyeUserFavoriteMsgModel.AId = aid;
                qiyeUserFavoriteMsgModel.Datatype = dataType;
                qiyeUserFavoriteMsgModel.MsgId = msgId;
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
            //统计表
            QiyeMsgviewFavoriteShare qiyemsgviewModel = QiyeMsgviewFavoriteShareBLL.SingleModel.GetModelByMsgId(aid, msgId, dataType);
            if (qiyemsgviewModel == null)
            {
                qiyemsgviewModel = new QiyeMsgviewFavoriteShare();
                qiyemsgviewModel.AddTime = DateTime.Now;
                qiyemsgviewModel.AId = aid;
                qiyemsgviewModel.DataType = dataType;
                qiyemsgviewModel.DzCount = 0;
                qiyemsgviewModel.FavoriteCount = 0;
                qiyemsgviewModel.FollowCount = 0;
                qiyemsgviewModel.MsgId = msgId;
                qiyemsgviewModel.ShareCount = 0;
                qiyemsgviewModel.SiXinCount = 0;
                qiyemsgviewModel.ViewCount = 0;
                qiyemsgviewModel.Id = Convert.ToInt32(QiyeMsgviewFavoriteShareBLL.SingleModel.Add(qiyemsgviewModel));
            }

            switch (actionType)
            {
                case (int)PointsActionType.关注: qiyemsgviewModel.FollowCount += count; break;
                case (int)PointsActionType.收藏: qiyemsgviewModel.FavoriteCount += count; break;
                case (int)PointsActionType.点赞: qiyemsgviewModel.DzCount += count; break;
                case (int)PointsActionType.看过: qiyemsgviewModel.ViewCount += 1; break;
                case (int)PointsActionType.私信: qiyemsgviewModel.SiXinCount += 1; break;
                case (int)PointsActionType.转发: qiyemsgviewModel.ShareCount += 1; break;
            }

            QiyeMsgviewFavoriteShareBLL.SingleModel.Update(qiyemsgviewModel);
            if (qiyeUserFavoriteMsgModel.Id > 0)
            {
                if (actionType != (int)PointsActionType.扫码 && actionType != (int)PointsActionType.转发 && actionType != (int)PointsActionType.看过 && actionType != (int)PointsActionType.私信)
                {
                    qiyeUserFavoriteMsgModel.AddTime = DateTime.Now;
                    QiyeUserFavoriteMsgBLL.SingleModel.Update(qiyeUserFavoriteMsgModel, "State,AddTime");
                }
            }
            else
            {
                qiyeUserFavoriteMsgModel.Id = Convert.ToInt32(QiyeUserFavoriteMsgBLL.SingleModel.Add(qiyeUserFavoriteMsgModel));
                if (qiyeUserFavoriteMsgModel.Id <= 0)
                {
                    return "操作失效";
                }
            }

            return "";
        }

        /// <summary>
        /// 获取我通过转发或者扫码浏览过的员工名片
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="userId"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="actionType"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public List<QiyeEmployee> GetMyListQiyeEmployee(int aid, int userId, out int totalCount, int pageIndex = 1, int pageSize = 10)
        {
            totalCount = 0;
            List<QiyeEmployee> list = new List<QiyeEmployee>();
            List<int> listQiyeEmployeeId = new List<int>();
            List<QiyeUserFavoriteMsg> listQiyeUserFavoriteMsg = QiyeUserFavoriteMsgBLL.SingleModel.GetUserFavoriteMsgList(aid, userId, out totalCount, pageIndex, pageSize, (int)PointsDataType.客户, (int)PointsActionType.扫码, (int)PointsActionType.转发);
            if (listQiyeUserFavoriteMsg != null && listQiyeUserFavoriteMsg.Count > 0)
            {
                foreach (QiyeUserFavoriteMsg item in listQiyeUserFavoriteMsg)
                {
                    listQiyeEmployeeId.Add(item.MsgId);
                }

                list = GetListByIds(string.Join(",", listQiyeEmployeeId));
                list.ForEach(x =>
                {
                    QiyeUserFavoriteMsg qiyeUserFavoriteMsg = listQiyeUserFavoriteMsg.Find(y => y.MsgId == x.Id);
                    if (qiyeUserFavoriteMsg != null)
                    {
                        x.Source = qiyeUserFavoriteMsg.ActionType == 6 ? "来自转发" : "来自扫码";
                    }
                    x.MsgCount = ImMessageBLL.SingleModel.GetCountBySql($"select count(*) from immessage where tuserid={userId} and fuserid={x.UserId} and isread=0");

                });

            }


            return list;


        }

        public List<QiyeEmployee> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<QiyeEmployee>();

            string sqlWhere = $"Id in ({ids}) and state>=0";
            return base.GetList(sqlWhere);
        }

        /// <summary>
        /// 从缓存获取客服，轮流获取客服，每次取第一位，然后再把第一位放最后
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public QiyeEmployee GetKeFuModel(int aid, string appId)
        {
            string key = string.Format(KeFuDequeueKey, aid, appId);
            List<QiyeEmployee> list = RedisUtil.Get<List<QiyeEmployee>>(key);
            if (list == null || list.Count <= 0)
                return null;

            QiyeEmployee model = list[0];
            list.RemoveAt(0);
            list.Add(model);
            RedisUtil.Set<List<QiyeEmployee>>(key, list);
            return model;
        }

        /// <summary>
        /// 将客服集合存入缓存
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public bool SetKeFuListToRedis(int aid, string appId)
        {
            string key = string.Format(KeFuDequeueKey, aid, appId);
            List<QiyeEmployee> list = GetQiyeKeFu(aid, appId);
            RedisUtil.Set<List<QiyeEmployee>>(key, list);
            return true;

        }

        /// <summary>
        /// 获取企业客服列表
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public List<QiyeEmployee> GetQiyeKeFu(int aid, string appId)
        {
            return base.GetList($"Aid={aid} and appId='{appId}' and State<>-1 and WorkState<>-1 and Kefu=1");
        }

   

    }
}
