using DAL.Base;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{
   public class PlatMsgReportBLL : BaseMySql<PlatMsgReport>
    {
        #region 单例模式
        private static PlatMsgReportBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatMsgReportBLL()
        {

        }

        public static PlatMsgReportBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatMsgReportBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<PlatMsgReport> GetListByaid(int aid, out int totalCount, int pageSize = 10, int pageIndex = 1, string orderWhere = "addTime desc")
        {
            string strWhere = $"aid={aid} and state=0";
            totalCount = base.GetCount(strWhere);

            List<PlatMsgReport> listCity_MsgReport = base.GetList(strWhere, pageSize, pageIndex, "*", orderWhere);
            if (listCity_MsgReport != null && listCity_MsgReport.Count > 0)
            {
                string msgIds = string.Join(",",listCity_MsgReport.Select(s=>s.MsgId).Distinct());
                List<PlatMsg> platMsgList = PlatMsgBLL.SingleModel.GetListByIds(msgIds);

                string msgCardIds = string.Join(",",platMsgList?.Select(s=>s.MyCardId).Distinct());
                string cardIds = string.Join(",", listCity_MsgReport.Select(s => s.ReportcardId).Distinct());
                if(!string.IsNullOrEmpty(msgCardIds))
                {
                    if(!string.IsNullOrEmpty(cardIds))
                    {
                        cardIds = cardIds + "," + msgCardIds;
                    }
                    else
                    {
                        cardIds = msgCardIds;
                    }
                }
                List<PlatMyCard> platMyCardList = PlatMyCardBLL.SingleModel.GetListByIds(cardIds);
                
                listCity_MsgReport.ForEach(x => {
                    //获取举者用户昵称
                    PlatMyCard platMyCard = platMyCardList?.FirstOrDefault(f=>f.Id == x.ReportcardId);
                    if (platMyCard != null)
                    {
                        x.ReportUserName = platMyCard.Name;
                    }

                    //获取被举报帖子的信息
                    PlatMsg platMsg = platMsgList?.FirstOrDefault(f=>f.Id == x.MsgId);
                    if (platMsg != null)
                    {
                        x.BeReportMsgPhone = platMsg.Phone;
                        PlatMyCard model = platMyCardList?.FirstOrDefault(f=>f.Id == platMsg.MyCardId);
                      
                        if (model != null)
                        {
                            x.BeReportUserName = model.Name;
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
        public List<PlatMsgReport> GetListByIds(int aid, string ids)
        {
            string strWhere = $"aid={aid} and state=0 and Id in({ids})";
            return base.GetList(strWhere);
        }

        public PlatMsgReport GetMsgReport(int reportUserId, int msgId)
        {
            return base.GetModel($"ReportcardId={reportUserId} and msgId={msgId}");
        }

    }
}
