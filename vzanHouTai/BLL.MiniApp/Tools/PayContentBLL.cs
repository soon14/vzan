using BLL.MiniApp.Conf;
using Core.MiniApp.Common;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Tools
{
    public class PayContentBLL : BaseMySql<PayContent>
    {
        #region 单例模式
        private static PayContentBLL _singleModel;
        private static readonly object SynObject = new object();

        private PayContentBLL()
        {

        }

        public static PayContentBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PayContentBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public bool UpdateContent(PayContent content)
        {
            return Update(content, "Amount,Exclusive,ContentType,VideoURL,VideoCover,AudioURL,ArticleId");
        }

        public PayContentPayment GetVipDiscount(PayContent content, VipLevel userLevel)
        {
            List<int> vipFreeList = content.Exclusive.ConvertToIntList(',');
            PayContentPayment payInfo = new PayContentPayment { OrgAmount = content.Amount };
            if (userLevel != null && vipFreeList != null && vipFreeList.Contains(userLevel.Id))
            {
                int free = 0;
                payInfo.PayAmount = free;
                payInfo.DiscountAmount = content.Amount;
                payInfo.Info = $"用户为[{userLevel.name}]免费购买付费内容";
            }
            else
            {
                payInfo.PayAmount = content.Amount;
                payInfo.Info = "无优惠";
            }
            return payInfo;
        }

        public PayContentPayment GetVipDiscountByUserId(PayContent content, int userId)
        {
            //会员等级信息
            VipRelation vipInfo = VipRelationBLL.SingleModel.GetModel($"uid={userId} and state>=0");
            VipLevel levelInfo = levelInfo = vipInfo != null ? VipLevelBLL.SingleModel.GetModel($"id={vipInfo.levelid} and state>=0") : null;
            return GetVipDiscount(content, levelInfo);
        }

        public List<EntNews> GetContentFormatEnt(List<EntNews> news, int? userId = null)
        {
            string payContentIds = string.Join(",", news.Where(item => item.paycontent > 0).Select(item => item.paycontent));
            List<PayContent> paycontents = null;
            if (!string.IsNullOrWhiteSpace(payContentIds))
            {
                paycontents = GetByIds(payContentIds);
            }

            paycontents?.ForEach(content =>
            {
                int index = news.FindIndex(item => item.paycontent == content.Id);
                if (index > -1)
                {
                    news[index].video = content.VideoURL;
                    news[index].videocover = content.VideoCover;
                    news[index].contenttype = content.ContentType;
                    news[index].amount = content.Amount * 0.01;
                    news[index].ispaid = userId.HasValue ? PaidContentRecordBLL.SingleModel.isPaid(content, userId.Value) : false;
                    news[index].payinfo = userId.HasValue ? GetVipDiscountByUserId(content, userId.Value) : null;
                    news[index].audio = content.AudioURL;
                }
            });
            return news;
        }

        public EntNews GetContentFormatEnt(EntNews newsModel, int? userId = null)
        {
            if (newsModel.paycontent == 0)
            {
                return newsModel;
            }
            return GetContentFormatEnt(new List<EntNews>() { newsModel }, userId).FirstOrDefault();
        }


        public List<PayContent> GetByIds(string ids)
        {
            return GetList($"Id in({ids})");
        }
    }
}
