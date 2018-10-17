using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.cityminiapp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.cityminiapp
{
   public class CityMsgReportBLL : BaseMySql<CityMsgReport>
    {
        #region 单例模式
        private static CityMsgReportBLL _singleModel;
        private static readonly object SynObject = new object();

        private CityMsgReportBLL()
        {

        }

        public static CityMsgReportBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CityMsgReportBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<CityMsgReport> getListByaid(int aid,out int totalCount, int pageSize=10,int pageIndex=1, string orderWhere="addTime desc")
        {
            string strWhere = $"aid={aid} and state=0";
            totalCount = base.GetCount(strWhere);

            List<CityMsgReport> listCity_MsgReport = base.GetList(strWhere,pageSize, pageIndex, "*", orderWhere);
            if (listCity_MsgReport != null && listCity_MsgReport.Count > 0)
            {
                string userIds = string.Join(",",listCity_MsgReport.Select(s=>s.reportUserId).Distinct());
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

                string cityMsgIds = string.Join(",",listCity_MsgReport.Select(s=>s.msgId));
                List<CityMsg> cityMsgList = CityMsgBLL.SingleModel.GetListByIds(cityMsgIds);

                listCity_MsgReport.ForEach(x => {
                    //获取举者用户昵称
                    C_UserInfo c_UserInfo = userInfoList?.FirstOrDefault(f=>f.Id == x.reportUserId);
                    if (c_UserInfo != null)
                    {
                        x.reportUserName = c_UserInfo.NickName;
                    }

                    //获取被举报帖子的信息
                    CityMsg city_Msg = cityMsgList?.FirstOrDefault(f=>f.Id == x.msgId);
                    if (city_Msg != null)
                    {
                        x.beReportMsgState = city_Msg.state;
                        x.beReportMsgPhone = city_Msg.phone;
                        c_UserInfo = C_UserInfoBLL.SingleModel.GetModel(city_Msg.userId);
                        if (c_UserInfo != null)
                        {
                            x.beReportUserName = c_UserInfo.NickName;
                           
                        }
                    }
                });
            }

            return listCity_MsgReport;
        }


        /// <summary>
        /// 根据id集合获取City_MsgReport列表数据
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<CityMsgReport> GetListByIds(int aid, string ids)
        {
            string strWhere = $"aid={aid} and state=0 and Id in({ids})";
            return base.GetList(strWhere);
        }

        public CityMsgReport GetCityMsgReport(int reportUserId, int msgId)
        {
            return base.GetModel($"reportUserId={reportUserId} and msgId={msgId}");
        }


    }
}
