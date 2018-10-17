using BLL.MiniApp.Helper;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.MiniApp.Tools
{
    public class SubscribeMessageBLL : BaseMySql<SubscribeMessage>
    {
        #region 单例模式
        private static SubscribeMessageBLL _singleModel;
        private static readonly object SynObject = new object();

        private SubscribeMessageBLL()
        {

        }

        public static SubscribeMessageBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SubscribeMessageBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public bool AddByFlashItem(FlashDealItem flashItem,C_UserInfo user, TmpType pageType = TmpType.小程序电商模板)
        {
            FlashDeal flashDeal = FlashDealBLL.SingleModel.GetModel(flashItem.DealId);

            object sendContent = new
            {
                keyword1 = new { value = "限时秒杀开始啦！", color = "#000000" },//交易类型
                keyword2 = new { value = flashDeal.Title, color = "#000000" },//活动信息
                keyword3 = new { value = flashItem.Title, color = "#000000" },//商品信息
                keyword4 = new { value = flashDeal.End.ToString(), color = "#000000" },//到期日
            };

            SubscribeMessage newMsg = new SubscribeMessage
            {
                State = (int)SubscribeMsgState.等待发送,
                Template = (int)SendTemplateMessageTypeEnum.秒杀开始通知,
                ContentType = (int)SubscribeMsgType.小程序模板消息,
                UserId = user.Id,
                SourceId = flashDeal.Id,
                OpenId = user.OpenId,
                PageType = (int)pageType,
                SendTime = flashDeal.Begin,
                SendContent = JsonConvert.SerializeObject(sendContent)
            };

            int newId = 0;
            return int.TryParse(Add(newMsg).ToString(),out newId) && newId > 0;
        }

        public bool SendMessage(SubscribeMessage message, out string errorMsg)
        {
            SendTemplateMessageTypeEnum messageTemplate;
            TmpType pageType;
            bool sendResult = false;
            if (!Enum.TryParse(message.Template.ToString(), out messageTemplate))
            {
                errorMsg = "消息模板枚举异常【SubscribeMessage.Template】";
                return sendResult;
            }
            if(!Enum.TryParse(message.PageType.ToString(),out pageType))
            {
                errorMsg = "小程序模板枚举异常【SubscribeMessage.ContentType】";
            }

            object sendContent = JsonConvert.DeserializeObject(message.SendContent);
            return TemplateMsg_Miniapp.SendSubscribeMessage(message.UserId, messageTemplate, pageType, sendContent, out errorMsg);
        }

        public bool UpdateSendTime(FlashDeal flashDeal)
        {
            List<FlashDealItem> updateItems = FlashDealItemBLL.SingleModel.GetByDealId(flashDeal.Id);
            List<string> updateSql = new List<string>();
            updateItems.ForEach(flashItem =>
            {
                List<string> sql = BuildUpdateSendTimeSql(flashItem.Id, SendTemplateMessageTypeEnum.秒杀开始通知, flashDeal.Begin);
                if(sql?.Count>0)
                {
                    updateSql.AddRange(sql);
                }
            });
            TransactionModel tran = new TransactionModel();
            tran.Add(updateSql.ToArray());
            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }

        //public bool UpdateSendTime(FlashDealItem flashItem, DateTime newSendTime)
        //{
        //    List<string> updateSql = BuildUpdateSendTimeSql(flashItem.Id, SendTemplateMessageTypeEnum.秒杀开始通知, newSendTime);
        //    if (updateSql == null || updateSql.Count == 0)
        //    {
        //        return true;
        //    }

        //    TransactionModel tran = new TransactionModel();
        //    tran.Add(updateSql.ToArray());
        //    return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        //}

        public bool DeleteSubscribe(FlashDeal flashDeal)
        {
            return DeleteSubscribe(flashDeal.Id, SendTemplateMessageTypeEnum.秒杀开始通知);
        }

        public bool DeleteSubscribe(FlashDealItem flashItem)
        {
            return DeleteSubscribe(flashItem.DealId, SendTemplateMessageTypeEnum.秒杀开始通知);
        }

        public bool DeleteSubscribe(List<FlashDealItem> flashItem)
        {
            return  DeleteSubscribe(flashItem.First().DealId, SendTemplateMessageTypeEnum.秒杀开始通知);
        }

        public bool DeleteSubscribe(int sourceId, SendTemplateMessageTypeEnum templateType)
        {
            List<SubscribeMessage> messages = GetListByPara(sourceId: sourceId, templateType: templateType, sendState: SubscribeMsgState.等待发送);
            if(messages.Count == 0)
            {
                return true;
            }

            TransactionModel tran = new TransactionModel();
            messages.ForEach(msg =>
            {
                msg.State = (int)SubscribeMsgState.删除;
                tran.Add(BuildUpdateSql(msg, "state"));
            });

            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }

        public List<string> BuildUpdateSendTimeSql(int sourceId, SendTemplateMessageTypeEnum templateType, DateTime newSendTime)
        {
            List<SubscribeMessage> messages = GetListByPara(sourceId:sourceId,templateType:templateType,sendState:SubscribeMsgState.等待发送);
            if (messages == null || messages.Count == 0)
            {
                return null;
            }

            List<string> updateSqls = new List<string>();
            messages.ForEach(msg =>
            {
                msg.SendTime = newSendTime;
                updateSqls.Add(BuildUpdateSql(msg, "SendTime"));
            });

            return updateSqls;
        }

        public List<SubscribeMessage> GetListByPara(int sourceId, SendTemplateMessageTypeEnum templateType, SubscribeMsgState sendState)
        {
            string whereSql = BuildWhereSql(sourceId: sourceId, templateType: templateType, sendState: sendState);
            return GetList(whereSql);
        }

        public List<SubscribeMessage> GetWaitForSend(int amount)
        {
            string whereSql = BuildWhereSql(sendTime: DateTime.Now, sendState: SubscribeMsgState.等待发送);
            return GetList(whereSql, amount, 1);
        }

        public string BuildWhereSql(int? sourceId = null, DateTime? sendTime = null, SendTemplateMessageTypeEnum? templateType = null, SubscribeMsgState? sendState = null)
        {
            List<string> whereSql = new List<string>();
            if (sendTime.HasValue)
            {
                whereSql.Add($"SendTime <= '{sendTime.ToString()}'");
            }
            if (sendState.HasValue)
            {
                whereSql.Add($"State = {(int)sendState.Value}");
            }
            if (sourceId.HasValue)
            {
                whereSql.Add($"SourceId = {sourceId.Value}");
            }
            if(templateType.HasValue)
            {
                whereSql.Add($"Template = {(int)templateType.Value}");
            }
            return string.Join(" AND ", whereSql);
        }
    }
}
