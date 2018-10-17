using BLL.MiniApp.Ent;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Tools
{
    public class PaidContentRecordBLL : BaseMySql<PaidContentRecord>
    {
        #region 单例模式
        private static PaidContentRecordBLL _singleModel;
        private static readonly object SynObject = new object();

        private PaidContentRecordBLL()
        {

        }

        public static PaidContentRecordBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PaidContentRecordBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public int AddPayRecordEnt(EntGoodsOrder order, EntNews attachContent, PayContent payContent, PayContentPayment payInfo)
        {
            PaidSnapShot snapshot = new PaidSnapShot
            {
                UserName = order.AccepterName,
                Contact = order.AccepterTelePhone,
                ContentType = payContent.ContentType,
                ArticleId = attachContent.id,
                VideoCover = payContent.VideoCover,
                VideoURL = payContent.VideoURL,
                PaidAmout = order.BuyPrice,
                OriginalAmout = payInfo.OrgAmount,
                DiscountInfo = payInfo.Info,
            };
            return AddPayRecord(attachContent.aid, attachContent.title, attachContent.paycontent, order.UserId, order.Id, snapshot: snapshot);
        }

        private int AddPayRecord(int aid, string title, int contentId, int userId, int payId, string attr = null, PaidSnapShot snapshot = null)
        {
            PaidContentRecord newRecord = new PaidContentRecord
            {
                Aid = aid,
                Addtime = DateTime.Now,
                Title = title,
                ContentId = contentId,
                UserId = userId,
                PayId = payId,
                Attr = attr,
                Snapshot = JsonConvert.SerializeObject(snapshot),
            };
            int newId = 0;
            if (int.TryParse(Add(newRecord).ToString(), out newId))
            {
                return newId;
            }
            return 0;
        }

        public List<string> UpdateToPaySql(PaidContentRecord record)
        {
            List<string> updateSql = new List<string>();
            switch(record.ContentType)
            {
                case (int)PaidContentType.专业版图文:
                case (int)PaidContentType.专业版视频:
                    updateSql = updateSql.Concat(EntGoodsOrderBLL.SingleModel.UpdatePaidContentOrder(record)).ToList();
                    break;
            }
            record = SetPayState(record);
            //底层框架抽风，改为纯手写SQL语句更新
            //updateSql.Add(BuildUpdateSql(record, "state"));
            updateSql.Add($"update PaidContentRecord set `state` = 1 where Id= {record.Id}");
            return updateSql;
        }

        public void UpdateSuccessCallBack(PaidContentRecord record)
        {
            switch (record.ContentType)
            {
                case (int)PaidContentType.专业版图文:
                case (int)PaidContentType.专业版视频:
                    EntGoodsOrderBLL.SingleModel.UpdatePaidContenSuccess(record);
                    break;
            }
        }

        public bool UpdateToPay(PaidContentRecord record)
        {
            TransactionModel tranModel = new TransactionModel();
            List<string> updateSql = UpdateToPaySql(record);
            updateSql.ForEach(sql =>
            {
                tranModel.Add(sql);
            });
            //log4net.LogHelper.WriteInfo(this.GetType(), $"90:{string.Join(",", updateSql)}");
            return ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
        }

        public bool isPaid(PayContent content, int userId)
        {
            string whereSql = BuildWhereSql(userId: userId, contentId: content.Id);
            return GetCount(whereSql) > 0;
        }

        public PaidContentRecord SetPayState(PaidContentRecord record)
        {
            record.State = 1;
            return record;
        }

        public List<PaidContentRecord> GetListByPara(int? appId = null, string contact = null, string title = null, DateTime? queryStart = null, DateTime? queryEnd = null, int pageIndex = 1, int pageSize = 10)
        {
            string whereSql = BuildWhereSql(appId:appId, contact: contact, title: title, queryStart: queryStart, queryEnd: queryEnd);
            return GetList(strWhere: whereSql, PageIndex: pageIndex, PageSize: pageSize);
        }

        public int GetCountByPara(int? appId = null, string contact = null, string title = null, DateTime? queryStart = null, DateTime? queryEnd = null)
        {
            string whereSql = BuildWhereSql(appId: appId, contact: contact, title: title, queryStart: queryStart, queryEnd: queryEnd);
            return GetCount(strWhere: whereSql);
        }

        public string BuildWhereSql(int? appId = null, int? userId = null, int? contentId = null,string title = null, string contact = null, DateTime? queryStart = null, DateTime? queryEnd = null, bool isDel = false)
        {
            List<string> whereSql = new List<string>();
            if (!isDel)
            {
                whereSql.Add("State = 1");
            }
            if(appId.HasValue)
            {
                whereSql.Add($"Aid = {appId.Value}");
            }
            if (userId.HasValue)
            {
                whereSql.Add($"UserId = {userId.Value}");
            }
            if (contentId.HasValue)
            {
                whereSql.Add($"ContentId = {contentId.Value}");
            }
            if (!string.IsNullOrWhiteSpace(contact))
            {
                whereSql.Add($"instr(Snapshot,'\"Contact\":\"{contact}\"') > 0");
            }
            if(!string.IsNullOrWhiteSpace(title))
            {
                whereSql.Add($"Title Like '%{title}%'");
            }
            if(queryStart.HasValue && queryEnd.HasValue)
            {
                whereSql.Add($"Addtime between '{queryStart.Value.ToString()}' AND '{queryEnd.Value.ToString()}'");
            }
            else if (queryStart.HasValue)
            {
                whereSql.Add($"Addtime >= '{queryStart.Value.ToString()}'");
            }
            else if (queryEnd.HasValue)
            {
                whereSql.Add($"Addtime <= '{queryEnd.Value.ToString()}'");
            }
            return string.Join(" AND ", whereSql);
        }
    }
}
