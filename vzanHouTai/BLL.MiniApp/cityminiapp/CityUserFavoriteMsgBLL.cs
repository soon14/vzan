using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.cityminiapp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BLL.MiniApp.cityminiapp
{
    public class CityUserFavoriteMsgBLL : BaseMySql<CityUserFavoriteMsg>
    {
        #region 单例模式
        private static CityUserFavoriteMsgBLL _singleModel;
        private static readonly object SynObject = new object();

        private CityUserFavoriteMsgBLL()
        {

        }

        public static CityUserFavoriteMsgBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CityUserFavoriteMsgBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<CityUserFavoriteMsg> GetListByaidAndMIds(int aid,int userid,string msgids,int actionType=0,int datatype = (int)PointsDataType.帖子)
        {
            string sqlwhere = $"aid={aid} and userid={userid} and msgid in ({msgids}) and state=0 and actiontype={actionType} and datatype={datatype}";
            return base.GetList(sqlwhere);
        }
        /// <summary>
        /// 获取一条收藏或者点赞记录
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="msgId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public CityUserFavoriteMsg getCity_UserFavoriteMsg(int aid, int msgId, int userId,int actionType=0,int datatype=(int)PointsDataType.帖子)
        {
            CityUserFavoriteMsg model = base.GetModel($"aid={aid} and userId={userId} and msgId={msgId} and actionType={actionType} and datatype={datatype}");
            return model;
        }

        /// <summary>
        /// 获取用户收藏帖子列表
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="userId"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<CityMsg> getListMyFavoriteMsg(int aid,int userId, out int totalCount, int pageSize = 10, int pageIndex = 1)
        {
            
            
            string strWhere = $"f.aid={aid} and f.userId={userId} and f.state=0 and f.actionType=0 and msg.state=1";

            string sql = $"select msg.*,f.Id as FavoriteId  from CityUserFavoriteMsg f left join CityMsg msg on f.msgId=msg.Id where {strWhere} order by addTime desc LIMIT {(pageIndex - 1) * pageSize},{pageSize}";
            totalCount = GetMyFavoriteMsgCount(strWhere);
            List<CityMsg> listCity_Msg = new List<CityMsg>();
          
            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql))
            {
               
                while (dr.Read())
                {
                    CityMsg x = CityMsgBLL.SingleModel.GetModel(dr);
                    if (x != null && x.Id > 0)
                    {
                        //获取用户头像
                        C_UserInfo c_UserInfo = C_UserInfoBLL.SingleModel.GetModel(x.userId);
                        if (c_UserInfo != null)
                        {
                            x.userName = c_UserInfo.NickName;
                            x.userHeaderImg = c_UserInfo.HeadImgUrl;
                        }

                        //获取帖子类别
                        CityStoreMsgType city_StoreMsgType = CityStoreMsgTypeBLL.SingleModel.GetModel(x.msgTypeId);
                        if (city_StoreMsgType != null)
                        {
                            x.msgTypeName = city_StoreMsgType.name;
                        }

                        //根据帖子ID获取其浏览量-收藏量-分享量数据
                        CityMsgViewFavoriteShare city_MsgViewFavoriteShare = CityMsgViewFavoriteShareBLL.SingleModel.getModelByMsgId(x.Id);
                        if (city_MsgViewFavoriteShare != null)
                        {
                            x.ViewCount = city_MsgViewFavoriteShare.ViewCount;
                            x.FavoriteCount = city_MsgViewFavoriteShare.FavoriteCount;
                            x.ShareCount = city_MsgViewFavoriteShare.ShareCount;
                            x.DzCount = city_MsgViewFavoriteShare.DzCount;
                        }
                        x.msgDetail = HttpUtility.HtmlDecode(x.msgDetail);
                        x.showTimeStr = CommondHelper.GetTimeSpan(DateTime.Now - x.addTime);
                        x.FavoriteId = (dr["FavoriteId"]!=DBNull.Value?Convert.ToInt32(dr["FavoriteId"]):0);//收藏记录ID
                        listCity_Msg.Add(x);
                    }

                }
                
            }


            return listCity_Msg;
        }

        public int GetMyFavoriteMsgCount(string strWhere)
        {
            string sql = $"select count(f.Id) from CityUserFavoriteMsg f left join CityMsg msg on f.msgId=msg.Id  where {strWhere} ";
            object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (obj != DBNull.Value)
            {
                return Convert.ToInt32(obj);
            }
            return 0;
        }


        public bool delMolde(int id,int userId,int appId)
        {
            CityUserFavoriteMsg model = base.GetModel($"Id={id} and userId={userId} and aid={appId} and actionType=0");
            if (model == null)
                return false;

            model.state = -1;
            return base.Update(model, "state");
        }
    }
}
