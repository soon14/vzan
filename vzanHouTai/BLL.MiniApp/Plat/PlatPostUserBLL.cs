using DAL.Base;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{
    public class PlatPostUserBLL
    {
        #region 单例模式
        private static PlatPostUserBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatPostUserBLL()
        {

        }

        public static PlatPostUserBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatPostUserBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 获取发帖用户信息列表 后台
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="userName"></param>
        /// <param name="userPhone"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="orderWhere"></param>
        /// <returns></returns>
        public List<PlatPostUser> getListByaid(int aid, ref int totalCount, int pageSize = 10, int pageIndex = 1, string userName = "", string userPhone = "", string startDate = "", string endDate = "", string orderWhere = "addTime desc")
        {
            List<PlatPostUser> listPlatPostUser = new List<PlatPostUser>();

            List<PlatMyCard> listPlatMyCard = PlatMyCardBLL.SingleModel.GetDataList(aid, ref totalCount, userName, string.Empty, userPhone, startDate, endDate, pageSize, pageIndex);
            if (listPlatMyCard != null && listPlatMyCard.Count > 0)
            {
                //查询到用户

                foreach (PlatMyCard item in listPlatMyCard)
                {
                    PlatPostUser platPostUser = new PlatPostUser();
                    platPostUser.PlatMyCardId = item.Id;
                    platPostUser.NickName = item.Name;
                    platPostUser.Phone = item.Phone;
                    platPostUser.AddTimeStr = item.AddTimeStr;

                    PlatPostUserMsgCountViewMolde model = GetCountViewModel(item.Id);
                    if (model != null)
                    {
                        platPostUser.PostMsgCount = model.PostMsgCount;
                        platPostUser.TopMsgCount = model.TopMsgCount;
                        platPostUser.PostMsgPrice = model.PostMsgPrice;
                    }

                    listPlatPostUser.Add(platPostUser);

                }

            }

            return listPlatPostUser;

        }



        /// <summary>
        /// 获取用户发布信息的统计 总发帖数量-总置顶天数-总置顶天数所花费的总金额
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public PlatPostUserMsgCountViewMolde GetCountViewModel(int PlatMyCardId)
        {
            PlatPostUserMsgCountViewMolde model = new PlatPostUserMsgCountViewMolde();
            if (PlatMyCardId > 0)
            {
                string sql = $"SELECT COUNT(Id) as PostMsgCount,SUM(topcostprice) as PostMsgPrice,(SELECT COUNT(Id) from PlatMsg where MyCardId={PlatMyCardId} and topday>0 and state<>0 and payState=1 and isTop=1) as TopMsgCount from PlatMsg where MyCardId={PlatMyCardId} and state<>0 and payState=1";

                using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql))
                {

                    while (dr.Read())
                    {
                        model.PostMsgCount = Convert.ToInt32(dr["PostMsgCount"]);
                        model.PostMsgPrice = (dr["PostMsgPrice"] == DBNull.Value ? 0 : Convert.ToDouble(dr["PostMsgPrice"]) * 0.01).ToString("0.00");
                        model.TopMsgCount = Convert.ToInt32(dr["TopMsgCount"]);
                    }

                }

            }
            return model;
        }


    }
}
