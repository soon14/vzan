using BLL.MiniApp.Conf;
using BLL.MiniApp.Dish;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.FunList;
using BLL.MiniApp.Plat;
using BLL.MiniApp.PlatChild;
using BLL.MiniApp.Qiye;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Tools;
using BLL.MiniApp.User;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Dish;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.FunctionList;
using Entity.MiniApp.Pin;
using Entity.MiniApp.Plat;
using Entity.MiniApp.PlatChild;
using Entity.MiniApp.Qiye;
using Entity.MiniApp.Stores;
using Entity.MiniApp.Tools;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.MiniApp.Helper
{
    /// <summary>
    /// 小程序模板消息逻辑类
    /// </summary>
    public static class TemplateMsg_Miniapp
    {
        /// <summary>
        /// 第一步：开启模板消息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="templateMsgType"></param>
        /// <param name="tmgu"></param>
        private static void AddNewTemplate_User(string appId, SendTemplateMessageTypeEnum templateMsgType, ref TemplateMsg_User tmgu)
        {
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appId, false);
            if (xcx == null)
            {
                return;
            }

            if (xcx.Type == (int)TmpType.小程序专业模板)
            {
                //专业版部分版本不可使用模板消息
                FunctionList functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcx.Type} and VersionId={xcx.VersionId}");
                if (functionList == null)
                {
                    return;
                }

                MessageMgr messageMgr = new MessageMgr();
                if (!string.IsNullOrEmpty(functionList.MessageMgr))
                {
                    messageMgr = JsonConvert.DeserializeObject<MessageMgr>(functionList.MessageMgr);
                    if (messageMgr.TemplateMessage == 1)
                    {
                        return;
                    }
                }
            }

            TemplateMsg msg = TemplateMsgBLL.SingleModel.GetModelByTmgType(templateMsgType);
            if (msg == null)
            {
                return;
            }

            //往微信appid加模板消息
            string errMsg = string.Empty;

            addResultModel _addResult = MsnModelHelper.addMsnToMy(xcx.AppId, msg.TitileId, msg.ColNums.Split(','), ref errMsg);
            if (!string.IsNullOrEmpty(errMsg))
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Miniapp), "用户添加小程序模板消息出错：" + errMsg);
                return;
            }

            tmgu = new TemplateMsg_User();
            tmgu.AppId = xcx.AppId;
            tmgu.Ttypeid = xcx.Type;
            tmgu.TmId = msg.Id;
            tmgu.ColNums = msg.ColNums;
            tmgu.TitleId = msg.TitileId;
            tmgu.State = 1;//启用
            tmgu.CreateDate = DateTime.Now;
            tmgu.TemplateId = _addResult.template_id;//微信公众号内的模板Id
            tmgu.TmgType = msg.TmgType;

            TemplateMsg_UserBLL.SingleModel.Add(tmgu);
        }

        /// <summary>
        /// 第二步：增加发送模板消息的机会
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openId"></param>
        /// <param name="prepay_id"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static string AddSendTranMessage(string appid, string openId, string prepay_id, int orderId)
        {
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appid, false);
            if (app == null)
            {
                return "授权信息错误";
            }

            if (!string.IsNullOrWhiteSpace(prepay_id))
            {
                //增加发送模板消息参数
                TemplateMsg_UserParam userParam = new TemplateMsg_UserParam();
                userParam.AppId = app.AppId;
                userParam.Form_IdType = 1;//form_id 为prepay_id
                userParam.OrderIdType = app.Type;
                userParam.Open_Id = openId;
                userParam.AddDate = DateTime.Now;
                userParam.Form_Id = prepay_id;
                userParam.State = 1;
                userParam.SendCount = 0;
                userParam.AddDate = DateTime.Now;
                userParam.LoseDateTime = DateTime.Now.AddDays(7);//prepay_id 有效期7天
                userParam.OrderId = orderId;
                TemplateMsg_UserParamBLL.SingleModel.Add(userParam);
            }

            return "";
        }

        #region 第三步：整理要发送的模板消息数据
        #region 足浴版模板消息

        /// <summary>
        /// 足浴版 模板消息
        /// </summary>
        /// <param name="orderInfo"></param>
        /// <param name="sendMsgType"></param>
        /// <param name="ver">不同版本</param>
        /// <returns></returns>
        public static object FootbathGetTemplateMessageData(EntGoodsOrder orderInfo, SendTemplateMessageTypeEnum sendMsgType)
        {
            
            if (orderInfo == null)
            {
                return null;
            }

            orderInfo.goodsCarts = EntGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={orderInfo.Id}");
            if (orderInfo.goodsCarts == null && orderInfo.goodsCarts.Count <= 0)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"订单：{ orderInfo.OrderNum} |找不到订单明细"));
                return null;
            }
            EntGoods serviceInfo = EntGoodsBLL.SingleModel.GetModel($"id={orderInfo.goodsCarts[0].FoodGoodsId}");
            if (serviceInfo == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"订单：{ orderInfo.OrderNum} |找不到服务详情"));
                return null;
            }
            TechnicianInfo technicianInfo = TechnicianInfoBLL.SingleModel.GetModel($"id={orderInfo.goodsCarts[0].technicianId}");
            if (technicianInfo == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"订单：{ orderInfo.OrderNum} |找不到技师信息"));
                return null;
            }
            FootBath storeInfo = FootBathBLL.SingleModel.GetModel($"appId={orderInfo.aId}");
            if (storeInfo == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"订单：{ orderInfo.OrderNum} |找不到门店信息"));
                return null;
            }
            object postData = new object();
            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.足浴预约成功通知:
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },//订单号
                        keyword2 = new { value = orderInfo.AccepterTelePhone, color = "#000000" },//电话
                        keyword3 = new { value = serviceInfo.name, color = "#000000" },//预约服务
                        keyword4 = new { value = technicianInfo.jobNumber, color = "#000000" },//技师号码
                        keyword5 = new { value = orderInfo.goodsCarts[0].showReservationTime, color = "#000000" },//预约时间
                        keyword6 = new { value = orderInfo.CreateDateStr, color = "#000000" },//下单时间
                        keyword7 = new { value = orderInfo.Message, color = "#000000" },//备注
                    };
                    break;

                case SendTemplateMessageTypeEnum.足浴预约取消通知:
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },//订单号
                        keyword2 = new { value = orderInfo.AccepterTelePhone, color = "#000000" },//电话
                        keyword3 = new { value = serviceInfo.name, color = "#000000" },//原预约项目
                        keyword4 = new { value = technicianInfo.jobNumber, color = "#000000" },//技师号码
                        keyword5 = new { value = orderInfo.goodsCarts[0].showReservationTime, color = "#000000" },//原预约时间
                        keyword6 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//取消时间
                        keyword7 = new { value = "商家取消", color = "#000000" },//取消原因
                    };
                    break;

                case SendTemplateMessageTypeEnum.足浴退款通知:
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },//订单号
                        keyword2 = new { value = serviceInfo.name, color = "#000000" },//商品名称
                        keyword3 = new { value = storeInfo.StoreName, color = "#000000" },//退款商家
                        keyword4 = new { value = $"{orderInfo.BuyPrice * 0.01}元", color = "#000000" },//退款金额
                        keyword5 = new { value = "商家取消预约", color = "#000000" },//退款原因
                        keyword6 = new { value = orderInfo.outOrderDate.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//退款时间
                        keyword7 = new { value = storeInfo.TelePhone, color = "#000000" },//客服电话
                    };
                    break;

                case SendTemplateMessageTypeEnum.足浴预约超时通知:
                    //预订单与订单不同提示
                    string timeOutRemark = orderInfo.OrderType == (int)EntOrderType.预约订单 ? "商家还未处理您的预订单,若有需要,请主动与商家联系" : "您已超过预定时间没有到场享受服务";
                    postData = new
                    {
                        keyword1 = new { value = serviceInfo.name, color = "#000000" },//预约项目名称
                        keyword2 = new { value = orderInfo.goodsCarts[0].showReservationTime, color = "#000000" },//预约时间
                        keyword3 = new { value = timeOutRemark, color = "#000000" },//超时说明
                    };
                    break;

                case SendTemplateMessageTypeEnum.足浴已预约活动开始提醒:
                    postData = new
                    {
                        keyword1 = new { value = storeInfo.StoreName, color = "#000000" },//店铺店名
                        keyword2 = new { value = serviceInfo.name, color = "#000000" },//预约内容
                        keyword3 = new { value = technicianInfo.jobNumber, color = "#000000" },//预约技师
                        keyword4 = new { value = orderInfo.goodsCarts[0].showReservationTime, color = "#000000" },//开始时间
                        keyword5 = new { value = storeInfo.Address, color = "#000000" },//门店地址
                        keyword6 = new { value = "您的服务将于半小时后开始，请及时到店享受服务", color = "#000000" },//温馨提示
                    };
                    break;

                default:
                    postData = null;
                    break;
            }
            return postData;
        }

        #endregion 足浴版模板消息

        #region 多门店模板消息

        public static object MutilStoreGetTemplateMessageData(EntGoodsOrder orderInfo, SendTemplateMessageTypeEnum sendMsgType)
        {
            if (orderInfo == null)
            {
                return null;
            }

            orderInfo.goodsCarts = EntGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={orderInfo.Id}");
            if (orderInfo.goodsCarts == null && orderInfo.goodsCarts.Count <= 0)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"订单：{ orderInfo.OrderNum} |找不到订单明细"));
                return null;
            }
            
            string foodGoodsIds = string.Join(",", orderInfo.goodsCarts.Select(s=>s.FoodGoodsId).Distinct());
            List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(foodGoodsIds);
            string goodsNames = string.Empty;
            foreach (EntGoodsCart goodsCart in orderInfo.goodsCarts)
            {
                EntGoods goods = entGoodsList?.FirstOrDefault(f=>f.id == goodsCart.FoodGoodsId);
                if (goods == null)
                {
                    log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"订单：{ orderInfo.OrderNum} |找不到商品详情，商品id：{goodsCart.FoodGoodsId}"));
                    continue;
                }
                if (!string.IsNullOrEmpty(goodsCart.SpecInfo))
                {
                    goodsNames += $"{goods.name}({goodsCart.SpecInfo}),";
                }
                else
                {
                    goodsNames += $"{goods.name}";
                }
            }
            goodsNames = goodsNames.TrimEnd(',');

            FootBath storeInfo = FootBathBLL.SingleModel.GetModel($"appId={orderInfo.aId}");
            if (storeInfo == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"订单：{ orderInfo.OrderNum} |找不到门店信息"));
                return null;
            }
            object postData = new object();
            string stateStr = string.Empty;
            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.多门店订单支付成功通知:
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },//订单号
                        keyword2 = new { value = orderInfo.CreateDateStr, color = "#000000" },//下单时间
                        keyword3 = new { value = goodsNames, color = "#000000" },//商品清单
                        keyword4 = new { value = orderInfo.PayDateStr, color = "#000000" },//支付时间
                        keyword5 = new { value = orderInfo.BuyModeStr, color = "#000000" },//支付方式
                        keyword6 = new { value = orderInfo.BuyPriceStr, color = "#000000" },//支付金额
                        keyword7 = new { value = orderInfo.StateStr, color = "#000000" },//订单状态
                    };
                    break;

                case SendTemplateMessageTypeEnum.多门店订单取消通知:
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },//订单号
                        keyword2 = new { value = orderInfo.MultiStoreGetWayStr, color = "#000000" },//订单类型
                        keyword3 = new { value = goodsNames, color = "#000000" },//商品详情
                        keyword4 = new { value = orderInfo.CreateDateStr, color = "#000000" },//下单时间
                        keyword5 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//,取消时间,
                        keyword6 = new { value = "商家取消", color = "#000000" },//取消原因
                    };
                    break;

                case SendTemplateMessageTypeEnum.多门店退款通知:
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },//订单号
                        keyword2 = new { value = goodsNames, color = "#000000" },//商品名称
                        keyword3 = new { value = storeInfo.StoreName, color = "#000000" },//退款商家
                        keyword4 = new { value = $"{orderInfo.BuyPrice * 0.01}元", color = "#000000" },//退款金额
                        keyword5 = new { value = "商家取消订单", color = "#000000" },//退款原因
                        keyword6 = new { value = orderInfo.outOrderDate.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//退款时间
                        keyword7 = new { value = storeInfo.TelePhone, color = "#000000" },//客服电话
                    };
                    break;

                case SendTemplateMessageTypeEnum.多门店反馈处理结果通知:
                    string result = orderInfo.State == (int)MiniAppEntOrderState.待接单 ? "拒绝退款" : "同意退款";
                    postData = new
                    {
                        keyword1 = new { value = $"您对订单号：{orderInfo.OrderNum}发起的退款申请，商家已反馈", color = "#000000" },//反馈内容
                        keyword2 = new { value = result, color = "#000000" },//反馈状态
                        keyword3 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//处理时间
                    };
                    break;

                case SendTemplateMessageTypeEnum.多门店订单确认通知:
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },//订单号
                        keyword2 = new { value = storeInfo.StoreName, color = "#000000" },//商户名称
                        keyword3 = new { value = goodsNames, color = "#000000" },//订单信息
                        keyword4 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//确认时间
                    };
                    break;

                case SendTemplateMessageTypeEnum.多门店订单配送通知:
                    stateStr = orderInfo.State == (int)MiniAppEntOrderState.待确认送达 ? "配送中" : "确认送达";
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.CreateDateStr, color = "#000000" },//下单时间
                        keyword2 = new { value = storeInfo.StoreName, color = "#000000" },//所属店铺
                        keyword3 = new { value = orderInfo.OrderNum, color = "#000000" },//订单编号
                        keyword4 = new { value = orderInfo.Address, color = "#000000" },//配送地址
                        keyword5 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//配送时间
                        keyword6 = new { value = goodsNames, color = "#000000" },//配送商品
                        keyword7 = new { value = stateStr, color = "#000000" },//配送状态
                    };
                    break;

                case SendTemplateMessageTypeEnum.多门店订单发货提醒:
                    stateStr = orderInfo.State == (int)MiniAppEntOrderState.待收货 ? "配送中" : "已接收";
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },//订单号
                        keyword2 = new { value = storeInfo.StoreName, color = "#000000" },//店铺名称
                        keyword3 = new { value = orderInfo.StateStr, color = "#000000" },//订单状态
                        keyword4 = new { value = goodsNames, color = "#000000" },//商品清单
                        keyword5 = new { value = orderInfo.AccepterName, color = "#000000" },//收货人
                        keyword6 = new { value = orderInfo.AccepterTelePhone, color = "#000000" },//收货人电话
                        keyword7 = new { value = orderInfo.Address, color = "#000000" },//目的地
                    };
                    break;

                default:
                    postData = null;
                    break;
            }
            return postData;
        }
        
        #endregion 多门店模板消息

        #region 专业版模板消息

        /// <summary>
        ///
        /// </summary>
        /// <param name="outOrderRemark">退款原因,退款通知模板消息使用</param>
        /// <param name="orderInfo"></param>
        /// <param name="sendMsgType"></param>
        /// <returns></returns>
        public static object EnterpriseGetTemplateMessageData(EntGoodsOrder orderInfo, SendTemplateMessageTypeEnum sendMsgType, string outOrderRemark = "")
        {
            object postData = null;
            if (orderInfo == null)
            {
                return postData;
            }

            //是否是拼团
            bool isGroup = orderInfo.GroupId > 0;
            //状态
            string stateStr = orderInfo.StateStr;
            //商品名称
            string goodsNames = string.Empty;
            //发货地址
            string shipAddress = string.Empty;

            Store store = StoreBLL.SingleModel.GetModelByRid(orderInfo.aId);
            shipAddress = store?.Address;
            EntGroupSponsor group = null;
            if (isGroup)
            {
                group = EntGroupSponsorBLL.SingleModel.GetModel(orderInfo.GroupId);
                if (group == null)
                {
                    log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"订单：找不到拼团资料：orderInfo.GroupId = {orderInfo.GroupId}"));
                    return postData;
                }
                if (group.State == 2)
                {
                    stateStr = "待发货";
                }
                else
                {
                    stateStr = "拼团中";
                }

                EntGroupsRelation groupR = EntGroupsRelationBLL.SingleModel.GetModel(group.EntGoodRId);
                if (groupR == null)
                {
                    log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"订单：找不到拼团资料：groupR.Id = {group.EntGoodRId}"));
                    return postData;
                }

                EntGoods groupGoods = EntGoodsBLL.SingleModel.GetModel(groupR.EntGoodsId);
                if (groupGoods == null)
                {
                    log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"订单：找不到拼团资料：groupGoods.Id = {groupR.EntGoodsId}"));
                    return postData;
                }
                goodsNames = groupGoods.name;

                EntGroupsRelation entgroup = EntGroupsRelationBLL.SingleModel.GetModel(group.EntGoodRId);
                if (entgroup == null)
                {
                    log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"订单：找不到拼团资料：EntGroupsRelation.Id = {group.EntGoodRId}"));
                    return postData;
                }
                group.GroupPrice = entgroup.GroupPriceStr;
            }
            else
            {
                orderInfo.goodsCarts = EntGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={orderInfo.Id}");
                if (orderInfo.goodsCarts == null && orderInfo.goodsCarts.Count <= 0)
                {
                    log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"订单：{ orderInfo.OrderNum} |找不到订单明细"));
                    return postData;
                }

                string foodGoodsIds = string.Join(",", orderInfo.goodsCarts.Select(s => s.FoodGoodsId).Distinct());
                List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(foodGoodsIds);
                foreach (var goodsCart in orderInfo.goodsCarts)
                {
                    EntGoods goods = entGoodsList?.FirstOrDefault(f => f.id == goodsCart.FoodGoodsId);
                    if (goods == null)
                    {
                        log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"订单：{ orderInfo.OrderNum} |找不到商品详情，商品id：{goodsCart.FoodGoodsId}"));
                        continue;
                    }
                    if (!string.IsNullOrEmpty(goodsCart.SpecInfo))
                    {
                        goodsNames += $"{goods.name}({goodsCart.SpecInfo}),";
                    }
                    else
                    {
                        goodsNames += $"{goods.name}";
                    }
                }
                goodsNames = goodsNames.TrimEnd(',');
            }
            
            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.专业版订单支付成功通知:
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },//订单号
                        keyword2 = new { value = orderInfo.CreateDateStr, color = "#000000" },//下单时间
                        keyword3 = new { value = goodsNames, color = "#000000" },//商品清单
                        keyword4 = new { value = orderInfo.PayDateStr, color = "#000000" },//支付时间
                        keyword5 = new { value = orderInfo.BuyModeStr, color = "#000000" },//支付方式
                        keyword6 = new { value = orderInfo.BuyPriceStr, color = "#000000" },//支付金额
                        keyword7 = new { value = stateStr, color = "#000000" },//订单状态
                        keyword8 = new { value = orderInfo.AccepterName, color = "#000000" },//联系人姓名
                        keyword9 = new { value = orderInfo.AccepterTelePhone, color = "#000000" },//联系人手机号码
                    };
                    break;

                case SendTemplateMessageTypeEnum.专业版订单发货提醒:
                    postData = new
                    {
                        //发货时间,订单号,收货人,收货电话,发货地址,订单内容
                        keyword1 = new { value = orderInfo.DistributeDateStr, color = "#000000" },
                        keyword2 = new { value = orderInfo.OrderNum, color = "#000000" },
                        keyword3 = new { value = orderInfo.AccepterName, color = "#000000" },
                        keyword4 = new { value = orderInfo.AccepterTelePhone, color = "#000000" },
                        keyword5 = new { value = shipAddress, color = "#000000" },
                        keyword6 = new { value = goodsNames, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.专业版订单强行发货通知:
                case SendTemplateMessageTypeEnum.专业版订单取消通知:
                    string cancelValue = "已取消";
                    if(sendMsgType == SendTemplateMessageTypeEnum.专业版订单强行发货通知)
                    {
                        cancelValue = "由于商家已发货，您的订单未能申请退款，如有疑问请与商家联系";
                    }
                    postData = new
                    {
                        //下单时间,订单编号,商品名称,订单金额,状态更新
                        keyword1 = new { value = orderInfo.CreateDateStr, color = "#000000" },
                        keyword2 = new { value = orderInfo.OrderNum, color = "#000000" },
                        keyword3 = new { value = goodsNames, color = "#000000" },
                        keyword4 = new { value = orderInfo.BuyPriceStr, color = "#000000" },
                        keyword5 = new { value = cancelValue, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.专业版订单退款通知:
                    postData = new
                    {
                        //下单时间,支付金额,退款金额,退款原因,退款时间,退款状态
                        keyword1 = new { value = orderInfo.CreateDateStr, color = "#000000" },
                        keyword2 = new { value = orderInfo.BuyPriceStr, color = "#000000" },
                        keyword3 = new { value = orderInfo.refundFeeStr, color = "#000000" },
                        keyword4 = new { value = outOrderRemark, color = "#000000" },
                        keyword5 = new
                        {
                            value = orderInfo.outOrderDate == Convert.ToDateTime("0001-01-01 00:00:00") ?
                                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : orderInfo.outOrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                            color = "#000000"
                        },
                        keyword6 = new { value = orderInfo.BuyMode == 1 ? "退款中" : "退款成功", color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.拼团拼团成功提醒:
                    if (group.State != 2)
                    {
                        log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"拼团未成功： {orderInfo.GroupId}"));
                        return postData;
                    }

                    postData = new
                    {
                        //成团时间,成团人数,商品名称,拼团价,订单号,订单状态
                        keyword1 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                        keyword2 = new { value = group.GroupSize, color = "#000000" },
                        keyword3 = new { value = goodsNames, color = "#000000" },
                        keyword4 = new { value = group.GroupPrice, color = "#000000" },
                        keyword5 = new { value = orderInfo.OrderNum, color = "#000000" },
                        keyword6 = new { value = "待发货", color = "#000000" },
                    };
                    break;

                default:
                    postData = null;
                    break;
            }
            return postData;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="outOrderRemark">退款原因,退款通知模板消息使用</param>
        /// <param name="orderInfo"></param>
        /// <param name="sendMsgType"></param>
        /// <returns></returns>
        public static object EnterpriseGetTemplateMessageData(EntUserForm form, SendTemplateMessageTypeEnum sendMsgType, string outOrderRemark = "")
        {
            object postData = null;
            if (form == null)
            {
                return postData;
            }

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(form.aid);
            if (xcx == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"专业版 - 产品预约：找不到小程序权限信息：Id = {form.aid}"));
                return postData;
            }
            Account account = AccountBLL.SingleModel.GetModel(xcx.AccountId);
            if (account == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"专业版 - 产品预约：找不到小程序拥有者信息：Id = {xcx.AccountId}"));
                return postData;
            }
            Store store = StoreBLL.SingleModel.GetModelByRid(xcx.Id);
            if (store == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"专业版 - 产品预约：找不到店铺资料：Id = {xcx.Id}"));
                return postData;
            }

            try
            {
                form.formremark = JsonConvert.DeserializeObject<EntFormRemark>(form.remark);
            }
            catch (Exception)
            {
                form.formremark = new EntFormRemark();
            }
            //预约表单内容
            string[] reservationFormData = form.formdatajson.Split(new string[] { "\",\"" }, StringSplitOptions.None);
            if (reservationFormData == null || !reservationFormData.Any())
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"专业版 - 产品预约：预约表单内容缺失 ：Id = {form.id}"));
                return postData;
            }
            //截取配送时间
            string reservationTime = reservationFormData.FirstOrDefault(s => s.Contains("预约时间\":\""))?.Substring(7) ?? string.Empty;
            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.专业版产品预约成功通知:
                    postData = new
                    {
                        //预约产品,时间,电话,地址
                        keyword1 = new { value = form.formremark.goods.name, color = "#000000" },
                        keyword2 = new { value = reservationTime, color = "#000000" },
                        keyword3 = new { value = account.ConsigneePhone, color = "#000000" },
                        keyword4 = new { value = store.Address, color = "#000000" },
                    };
                    break;

                default:
                    postData = null;
                    break;
            }
            return postData;
        }

        #endregion 专业版模板消息

        #region 餐饮版模板消息
        public static object FoodGetTemplateMessageData(Food store, FoodGoodsOrder orderInfo, SendTemplateMessageTypeEnum sendMsgType)
        {
            List<FoodGoodsCart> modelDtl = FoodGoodsCartBLL.SingleModel.GetList($" GoodsOrderId = {orderInfo.Id} ") ?? new List<FoodGoodsCart>();
            
            string foodGoodsIds = string.Join(",",modelDtl.Select(s=>s.FoodGoodsId).Distinct());
            List<FoodGoods> foodGoodsList = FoodGoodsBLL.SingleModel.GetListByIds(foodGoodsIds);
            modelDtl.ForEach(x =>
            {
                FoodGoods goods = foodGoodsList?.FirstOrDefault(f=>f.Id == x.FoodGoodsId) ?? new FoodGoods();
                x.goodsMsg = goods;
            });

            string modelDtlName = "";
            modelDtlName = string.Join("+", modelDtl.Select(x => x.goodsMsg.GoodsName));

            object postData = new object();
            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.餐饮订单支付成功通知:
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },
                        keyword2 = new { value = orderInfo.CreateDateStr, color = "#000000" },
                        keyword3 = new { value = modelDtlName, color = "#000000" },
                        keyword4 = new { value = orderInfo.PayDateStr, color = "#000000" },
                        keyword5 = new { value = Enum.GetName(typeof(miniAppBuyMode), orderInfo.BuyMode), color = "#000000" },
                        keyword6 = new { value = orderInfo.BuyPriceStr, color = "#000000" },
                        //keyword7 = new { value = Enum.GetName(typeof(miniAppFoodOrderState), model.State), color = "#000000" },
                        keyword7 = new { value = miniAppFoodOrderState.待接单.ToString(), color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.餐饮订单配送通知:
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.CreateDateStr, color = "#000000" },
                        keyword2 = new { value = store.FoodsName, color = "#000000" },
                        keyword3 = new { value = orderInfo.OrderNum, color = "#000000" },
                        keyword4 = new { value = orderInfo.Address, color = "#000000" },
                        keyword5 = new { value = orderInfo.DistributeDateStr, color = "#000000" },
                        keyword6 = new { value = modelDtlName, color = "#000000" },
                        keyword7 = new { value = Enum.GetName(typeof(miniAppFoodOrderState), orderInfo.State), color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.餐饮退款申请通知:
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },
                        keyword2 = new { value = modelDtlName, color = "#000000" },
                        keyword3 = new { value = orderInfo.BuyPriceStr, color = "#000000" },
                        keyword4 = new { value = orderInfo.AccepterTelePhone, color = "#000000" },
                        keyword5 = new { value = orderInfo.AccepterName, color = "#000000" },
                        keyword6 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                        keyword7 = new { value = string.Empty, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.餐饮退款成功通知:
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },
                        keyword2 = new { value = modelDtlName, color = "#000000" },
                        keyword3 = new { value = orderInfo.outOrderDate.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                        keyword4 = new { value = orderInfo.BuyPriceStr, color = "#000000" },
                        //keyword5 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                        keyword5 = new { value = "1-7个工作日", color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.餐饮订单拒绝通知:
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },
                        keyword2 = new { value = store.FoodsName, color = "#000000" },
                        keyword3 = new { value = orderInfo.Remark, color = "#000000" },
                        keyword4 = new { value = modelDtlName, color = "#000000" },
                        keyword5 = new { value = orderInfo.BuyPriceStr, color = "#000000" },
                        keyword6 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                    };
                    break;

                default:

                    break;
            }
            return postData;
        }

        #endregion 餐饮版模板消息

        #region 排队模板消息

        public static object SortQueueGetTemplateMessageData(string storeName, SortQueue sortQueue, SendTemplateMessageTypeEnum sendMsgType)
        {
            object postData = null;
            if (sortQueue == null)
            {
                return postData;
            }

            //当前门店排队队列
            List<SortQueue> all_SortQueues = SortQueueBLL.SingleModel.GetListByQueueing(sortQueue.aId, sortQueue.storeId);
            if (all_SortQueues == null || !all_SortQueues.Any())
            {
                return postData;
            }

            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.排队拿号排队成功通知:
                    postData = new
                    {
                        keyword1 = new { value = storeName, color = "#000000" },//店铺名
                        keyword2 = new { value = sortQueue.sortNo, color = "#000000" },//排队号码
                        keyword3 = new { value = sortQueue.createDateStr, color = "#000000" },//排队时间
                        keyword4 = new { value = all_SortQueues.Count(s => s.id < sortQueue.id), color = "#000000" },//等候人数
                    };
                    break;

                case SendTemplateMessageTypeEnum.排队拿号排队即将排到通知:
                    postData = new
                    {
                        //餐厅名称、您的排号、还需等待、取号时间
                        keyword1 = new { value = storeName, color = "#000000" },
                        keyword2 = new { value = sortQueue.sortNo, color = "#000000" },
                        keyword3 = new { value = $"{all_SortQueues.Count(s => s.id < sortQueue.id)} 桌", color = "#000000" },
                        keyword4 = new { value = sortQueue.createDateStr, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.排队拿号排队到号通知:
                    postData = new
                    {
                        //餐厅名称、排队号码、当前叫号、排队时间
                        keyword1 = new { value = storeName, color = "#000000" },
                        keyword2 = new { value = sortQueue.sortNo, color = "#000000" },
                        keyword3 = new { value = all_SortQueues.FirstOrDefault().sortNo, color = "#000000" },
                        keyword4 = new { value = sortQueue.createDateStr, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.排队拿号排队到号通知_通用:
                    postData = new
                    {
                        //您的排号、还需等待、取号时间
                        keyword1 = new { value = sortQueue.sortNo, color = "#000000" },
                        keyword2 = new { value = all_SortQueues.FirstOrDefault().sortNo, color = "#000000" },
                        keyword3 = new { value = sortQueue.createDateStr, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.排队拿号排队即将排到通知_通用:
                    postData = new
                    {
                        //您的排号、还需等待、取号时间
                        keyword1 = new { value = sortQueue.sortNo, color = "#000000" },
                        keyword2 = new { value = $"{all_SortQueues.Count(s => s.id < sortQueue.id)} 人", color = "#000000" },
                        keyword3 = new { value = sortQueue.createDateStr, color = "#000000" },
                    };
                    break;

                default:

                    break;
            }
            return postData;
        }

        /// <summary>
        /// 智慧餐厅排队提醒模板数据
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="sortQueue"></param>
        /// <param name="sendMsgType"></param>
        /// <returns></returns>
        public static object SortQueueTemplateMessageData(string storeName, DishQueueUp sortQueue, SendTemplateMessageTypeEnum sendMsgType)
        {
            object postData = null;
            if (sortQueue == null)
            {
                return postData;
            }

            ////当前门店排队队列
            //List<SortQueue> all_SortQueues = new SortQueueBLL().GetListByQueueing(sortQueue.aId, sortQueue.storeId);
            //if (all_SortQueues == null || !all_SortQueues.Any()) return postData;

            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.排队拿号排队到号通知:
                    postData = new
                    {
                        //餐厅名称、排队号码、当前叫号、排队时间
                        keyword1 = new { value = storeName, color = "#000000" },
                        keyword2 = new { value = sortQueue.q_haoma, color = "#000000" },
                        keyword3 = new { value = sortQueue.q_haoma, color = "#000000" },
                        keyword4 = new { value = sortQueue.q_addtime_str, color = "#000000" },
                    };
                    break;

                default:

                    break;
            }
            return postData;
        }

        /// <summary>
        /// 充值成功
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="sortQueue"></param>
        /// <param name="sendMsgType"></param>
        /// <returns></returns>
        public static object DishMessageData(string storeName = "", DishVipCard card = null, DishCardAccountLog log = null, double sumMoney = 0, SendTemplateMessageTypeEnum sendMsgType = SendTemplateMessageTypeEnum.None, DishOrder dishOrder = null)
        {
            object postData = null;
            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.充值成功通知:
                    postData = new
                    {
                        keyword1 = new { value = storeName, color = "#000000" },//店铺名称
                        keyword2 = new { value = "￥" + sumMoney.ToString("f2") , color = "#000000" },//充值金额
                        keyword3 = new { value = "￥" + log.account_money.ToString("f2") , color = "#000000" },//支付金额
                        keyword4 = new { value = "￥" + card.account_balance.ToString("f2") , color = "#000000" },//当前余额
                        keyword5 = new { value = log.account_info, color = "#000000" },//充值详情
                        keyword6 = new { value = log.add_time.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//充值时间
                    };
                    break;
                case SendTemplateMessageTypeEnum.下单成功通知:
                    dishOrder.carts = DishShoppingCartBLL.SingleModel.GetCartsByOrderId(dishOrder.id) ?? new List<DishShoppingCart>();
                    string orderContent = string.Empty;
                    if (dishOrder.carts.Count > 0)
                    {
                        orderContent = string.Join("\r\n", dishOrder.carts.Select(p => p.goods_name + (string.IsNullOrEmpty(p.goods_attr) ? "" : "(" + p.goods_attr + ")") + "x" + p.goods_number));
                    }

                    postData = new
                    {
                        keyword1 = new { value = storeName, color = "#000000" },//店铺名称
                        keyword2 = new { value = dishOrder.order_sn, color = "#000000" },//订单编号
                        keyword3 = new { value = dishOrder.order_status_txt + "," + dishOrder.pay_status_txt, color = "#000000" },//订单状态
                        keyword4 = new { value = orderContent, color = "#000000" },//订单内容
                        keyword5 = new { value = dishOrder.order_amount.ToString("f2") + "元", color = "#000000" },//订单金额
                        keyword6 = new { value = dishOrder.settlement_total_fee.ToString("f2") + "元", color = "#000000" },//支付金额
                        keyword7 = new { value = dishOrder.pay_end_time.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//下单时间
                    };
                    break;
                case SendTemplateMessageTypeEnum.智慧餐厅退款通知:
                    postData = new
                    {
                        keyword1 = new { value = "￥" + dishOrder.refundMoney.ToString("f2"), color = "#000000" },//退款金额
                        keyword2 = new { value = storeName, color = "#000000" },//退款商家
                        keyword3 = new { value = dishOrder.order_sn, color = "#000000" },//订单编号
                        keyword4 = new { value = dishOrder.refundReason, color = "#000000" },//退款原因
                        keyword5 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//退款时间
                    };
                    break;
                case SendTemplateMessageTypeEnum.智慧餐厅订单支付成功:
                    dishOrder.carts = DishShoppingCartBLL.SingleModel.GetCartsByOrderId(dishOrder.id) ?? new List<DishShoppingCart>();
                    string orderContent2 = string.Empty;
                    if (dishOrder.carts.Count > 0)
                    {
                        orderContent2 = string.Join("\r\n", dishOrder.carts.Select(p => p.goods_name + (string.IsNullOrEmpty(p.goods_attr) ? "" : "(" + p.goods_attr + ")") + "x" + p.goods_number));
                    }
                    //支付金额,下单门店,订单编号,订单状态,商品清单,支付时间
                    postData = new
                    {
                        keyword1 = new { value = "￥" + dishOrder.settlement_total_fee.ToString("f2"), color = "#000000" },//支付金额
                        keyword2 = new { value = storeName, color = "#000000" },//下单门店
                        keyword3 = new { value = dishOrder.order_sn, color = "#000000" },//订单编号
                        keyword4 = new { value = dishOrder.order_status_txt + "," + dishOrder.pay_status_txt, color = "#000000" },//订单状态
                        keyword5 = new { value = orderContent2, color = "#000000" },//商品清单
                        keyword6 = new { value = dishOrder.pay_end_time.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//支付时间
                    };
                    break;
                default:

                    break;
            }
            return postData;
        }

        #endregion 排队模板消息

        #region 拼团基础版模板消息

        /// <summary>
        /// 拼团基础版 - 模板消息内容拼接
        /// </summary>
        /// <param name="outOrderRemark">退款原因</param>
        /// <param name="groupUser"></param>
        /// <param name="sendMsgType"></param>
        /// <returns></returns>
        public static object GroupGetTemplateMessageData(string outOrderRemark, GroupUser groupUser, SendTemplateMessageTypeEnum sendMsgType)
        {
            object postData = new object();
            if (groupUser == null)
            {
                return postData;
            }

            //发货地址
            string shipAddress = string.Empty;
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(groupUser.AppId);
            if (xcx != null)
            {
                Store store = StoreBLL.SingleModel.GetModelByRid(xcx.Id);
                shipAddress = store?.Address;
            }

            Groups groups = GroupsBLL.SingleModel.GetModel(groupUser.GroupId);  //团购商品资料
            if (groups == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"拼接拼团数据 groups_null:groupUser.GroupId = {groupUser.GroupId}"));
                return postData;
            }

            GroupSponsor groupSponsor = GroupSponsorBLL.SingleModel.GetModel(groupUser.GroupSponsorId); //开团情况
            
            //支付方式
            string payTypeStr = string.Empty;
            switch (groupUser.PayType)
            {
                case 0:
                    payTypeStr = "微信支付";
                    break;

                case 1:
                    payTypeStr = "储值支付";
                    break;
            }
            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.拼团拼团成功提醒:

                    List<GroupUser> groupUsers = GroupUserBLL.SingleModel.GetList($" GroupSponsorId = {groupUser.GroupSponsorId} and state = 0 "); //团员资料
                    if (groupUsers == null || !groupUsers.Any())
                    {
                        log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"找不到开团记录 groupUsers_null<groupUser.GroupSponsorId = {groupUser.GroupSponsorId}>"));
                        return postData;
                    }

                    postData = new
                    {
                        keyword1 = new { value = groupUsers.OrderByDescending(g => g.Id).FirstOrDefault().PayTime.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//成团时间
                        keyword2 = new { value = groups.GroupSize, color = "#000000" },//成团人数
                        keyword3 = new { value = groups.GroupName, color = "#000000" },//商品名称
                        keyword4 = new { value = (groups.DiscountPrice * 0.01).ToString("0.00"), color = "#000000" },//拼团价
                        keyword5 = new { value = groupUser.OrderNo, color = "#000000" },//订单号
                        keyword6 = new { value = "待发货", color = "#000000" },//订单状态 --成团成功默认状态待发货
                    };
                    break;

                case SendTemplateMessageTypeEnum.拼团基础版订单支付成功通知:
                    postData = new
                    {
                        keyword1 = new { value = groupUser.OrderNo, color = "#000000" }, //订单号码
                        keyword2 = new { value = groupUser.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" }, //下单时间
                        keyword3 = new { value = groups.GroupName, color = "#000000" }, //商品清单
                        keyword4 = new { value = groupUser.PayTime.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" }, //支付时间
                        keyword5 = new { value = payTypeStr, color = "#000000" }, //支付方式
                        keyword6 = new { value = (groupUser.BuyPrice * 0.01).ToString("0.00"), color = "#000000" }, //支付金额
                        keyword7 = new { value = groupUser.IsGroup == 1 ? (groupSponsor.State == 2 ? "待发货" : "待成团") : "待发货", color = "#000000" }, //订单状态
                        keyword8 = new { value = groupUser.UserName, color = "#000000" },//联系人姓名
                        keyword9 = new { value = groupUser.Phone, color = "#000000" },//联系人手机号码
                    };
                    break;

                case SendTemplateMessageTypeEnum.拼团基础版订单发货提醒:
                    postData = new
                    {
                        //发货时间,订单号,收货人,收货电话,发货地址,订单内容
                        keyword1 = new { value = groupUser.SendGoodTime.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                        keyword2 = new { value = groupUser.OrderNo, color = "#000000" },
                        keyword3 = new { value = groupUser.UserName, color = "#000000" },
                        keyword4 = new { value = groupUser.Phone, color = "#000000" },
                        keyword5 = new { value = shipAddress, color = "#000000" },
                        keyword6 = new { value = groups.GroupName, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.拼团基础版订单取消通知:
                    postData = new
                    {
                        //订单编号,下单时间,商品名称,订单金额,状态更新
                        keyword1 = new { value = "该订单未生成订单号", color = "#000000" },
                        keyword2 = new { value = groupUser.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                        keyword3 = new { value = groups.GroupName, color = "#000000" },
                        keyword4 = new { value = (groupUser.BuyPrice * 0.01).ToString("0.00"), color = "#000000" },
                        keyword5 = new { value = "已取消", color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.拼团基础版订单退款通知:
                    postData = new
                    {
                        //下单时间,支付金额,退款金额,退款原因,退款时间,退款状态
                        keyword1 = new { value = groupUser.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                        keyword2 = new { value = (groupUser.BuyPrice * 0.01).ToString("0.00"), color = "#000000" },
                        keyword3 = new { value = (groupUser.BuyPrice * 0.01).ToString("0.00"), color = "#000000" },
                        keyword4 = new { value = outOrderRemark, color = "#000000" },
                        keyword5 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//默认当前时间
                        keyword6 = new { value = groupUser.PayType == 0 ? "退款中" : "退款成功", color = "#000000" },
                    };
                    break;

                default:break;
            }

            return postData;
        }

        #endregion 拼团基础版模板消息

        #region 砍价模板消息

        /// <summary>
        /// 砍价模板消息 - 模板消息内容拼接
        /// </summary>
        /// <param name="outOrderRemark">退款原因</param>
        /// <param name="groupUser"></param>
        /// <param name="sendMsgType"></param>
        /// <returns></returns>
        public static object BargainGetTemplateMessageData(BargainUser bargainUser, SendTemplateMessageTypeEnum sendMsgType, string outOrderRemark = "")
        {
            object postData = new object();
            if (bargainUser == null)
            {
                return postData;
            }

            int aId = 0;//获取aid
            int tempType = 0;//模板类型
            string goodName = string.Empty;//商品名称
            string shipAddress = string.Empty;

            #region 拿取 aId,tempType

            Bargain curBargain = BargainBLL.SingleModel.GetModel(bargainUser.BId);
            if (curBargain != null)
            {
                goodName = curBargain.BName;
                switch (curBargain.BargainType)
                {
                    case 0:
                        Store store = StoreBLL.SingleModel.GetModel(curBargain.StoreId);
                        if (store != null)
                        {
                            aId = store.appId;
                        }
                        break;

                    default:
                        aId = curBargain.StoreId;
                        break;
                }
            }

            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModel(aId);
            if (app == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"发送砍价失败,基础参数不足 app_null:aId = {aId}"));
                return postData;
            }
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(app.TId);
            if (xcxTemplate == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"发送砍价失败,基础参数不足 xcxTemplate_null:app.TId = {app.TId}"));
                return postData;
            }
            tempType = xcxTemplate.Type;

            #endregion 拿取 aId,tempType

            switch (tempType)
            {
                case (int)TmpType.小程序电商模板测试:
                case (int)TmpType.小程序专业模板:
                    Store store = StoreBLL.SingleModel.GetModelByRid(aId);
                    shipAddress = store?.Address;
                    break;
            }

            //支付方式
            string payTypeStr = string.Empty;
            switch (bargainUser.PayType)
            {
                case 1:
                    payTypeStr = "微信支付";
                    break;

                case 2:
                    payTypeStr = "储值支付";
                    break;
            }
            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.砍价订单支付成功提醒:
                    postData = new
                    {
                        keyword1 = new { value = bargainUser.OrderId, color = "#000000" }, //订单号码
                        keyword2 = new { value = bargainUser.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" }, //下单时间
                        keyword3 = new { value = goodName, color = "#000000" }, //商品清单
                        keyword4 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" }, //支付时间
                        keyword5 = new { value = payTypeStr, color = "#000000" }, //支付方式
                        keyword6 = new { value = bargainUser.CurrentPriceStr, color = "#000000" }, //支付金额
                        keyword7 = new { value = "待发货", color = "#000000" }, //订单状态
                        keyword8 = new { value = bargainUser.AddressUserName, color = "#000000" },//联系人姓名
                        keyword9 = new { value = bargainUser.TelNumber, color = "#000000" },//联系人手机号码
                    };
                    break;

                case SendTemplateMessageTypeEnum.砍价订单发货提醒:
                    postData = new
                    {
                        //发货时间,订单号,收货人,收货电话,发货地址,订单内容
                        keyword1 = new { value = bargainUser.SendGoodsTime.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                        keyword2 = new { value = bargainUser.OrderId, color = "#000000" },
                        keyword3 = new { value = bargainUser.AddressUserName, color = "#000000" },
                        keyword4 = new { value = bargainUser.TelNumber, color = "#000000" },
                        keyword5 = new { value = shipAddress, color = "#000000" },
                        keyword6 = new { value = goodName, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.砍价订单取消通知:
                    postData = new
                    {
                        //订单编号,下单时间,商品名称,订单金额,状态更新
                        keyword1 = new { value = bargainUser.OrderId, color = "#000000" },
                        keyword2 = new { value = bargainUser.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                        keyword3 = new { value = goodName, color = "#000000" },
                        keyword4 = new { value = bargainUser.CurrentPriceStr, color = "#000000" },
                        keyword5 = new { value = "已取消", color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.砍价订单退款通知:
                    postData = new
                    {
                        //下单时间,支付金额,退款金额,退款原因,退款时间,退款状态
                        keyword1 = new { value = bargainUser.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                        keyword2 = new { value = bargainUser.CurrentPriceStr, color = "#000000" },
                        keyword3 = new { value = bargainUser.refundFeeStr, color = "#000000" },
                        keyword4 = new { value = outOrderRemark, color = "#000000" },
                        keyword5 = new { value = bargainUser.outOrderDate.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//默认当前时间
                        keyword6 = new { value = bargainUser.PayType == 1 ? "退款中" : "退款成功", color = "#000000" },
                    };
                    break;

                default:

                    break;
            }

            return postData;
        }

        #endregion 砍价模板消息

        #region 独立小程序版模板消息

        /// <summary>
        ///
        /// </summary>
        /// <param name="outOrderRemark">退款原因</param>
        /// <param name="orderInfo"></param>
        /// <param name="sendMsgType"></param>
        /// <returns></returns>
        public static object PlatChildGetTemplateMessageData(PlatChildGoodsOrder orderInfo, SendTemplateMessageTypeEnum sendMsgType, string outOrderRemark = "")
        {
            object postData = null;
            if (orderInfo == null)
            {
                return postData;
            }
            
            //状态
            string stateStr = orderInfo.StateStr;
            //商品名称
            string goodsNames = string.Empty;
            //发货地址
            string shipAddress = string.Empty;

            PlatStore store = PlatStoreBLL.SingleModel.GetModelByAId(orderInfo.AId);
            shipAddress = store?.Location;
            //购物车
            orderInfo.CartList = PlatChildGoodsCartBLL.SingleModel.GetListByOrderIds(orderInfo.Id.ToString());
            if (orderInfo.CartList == null && orderInfo.CartList.Count <= 0)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"模板消息：{ orderInfo.OrderNum} |找不到订单明细"));
                return postData;
            }
            //商品
            string goodsids = string.Join(",", orderInfo.CartList.Select(s => s.GoodsId).Distinct());
            List<PlatChildGoods> goodslist = PlatChildGoodsBLL.SingleModel.GetListByIds(goodsids);
            foreach (PlatChildGoodsCart cartitem in orderInfo.CartList)
            {
                PlatChildGoods goods = goodslist?.FirstOrDefault(f => f.Id == cartitem.GoodsId);
                if (goods == null)
                {
                    log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"模板消息：{ orderInfo.OrderNum} |找不到商品详情，商品id：{cartitem.GoodsId}"));
                    continue;
                }
                if (!string.IsNullOrEmpty(cartitem.SpecInfo))
                {
                    goodsNames += $"{goods.Name}({cartitem.SpecInfo}),";
                }
                else
                {
                    goodsNames += $"{goods.Name}";
                }
            }
            goodsNames = goodsNames.TrimEnd(',');
            if (goodsNames.Length > 18) //截取，以防内容过多,模板消息变成空白
            {
                goodsNames = goodsNames.Substring(0, 18) + "...";
            }

            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.独立小程序版订单支付成功通知:
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },//订单号
                        keyword2 = new { value = orderInfo.AddTimeStr, color = "#000000" },//下单时间
                        keyword3 = new { value = goodsNames, color = "#000000" },//商品清单
                        keyword4 = new { value = orderInfo.PayTimeStr, color = "#000000" },//支付时间
                        keyword5 = new { value = orderInfo.BuyModeStr, color = "#000000" },//支付方式
                        keyword6 = new { value = orderInfo.BuyPriceStr, color = "#000000" },//支付金额
                        keyword7 = new { value = stateStr, color = "#000000" },//订单状态
                        keyword8 = new { value = orderInfo.AccepterName, color = "#000000" },//联系人姓名
                        keyword9 = new { value = orderInfo.AccepterTelePhone, color = "#000000" },//联系人手机号码
                    };

                    break;

                case SendTemplateMessageTypeEnum.独立小程序版订单发货提醒:
                    postData = new
                    {
                        //发货时间,订单号,收货人,收货电话,发货地址,订单内容
                        keyword1 = new { value = orderInfo.SendTimeStr, color = "#000000" },
                        keyword2 = new { value = orderInfo.OrderNum, color = "#000000" },
                        keyword3 = new { value = orderInfo.AccepterName, color = "#000000" },
                        keyword4 = new { value = orderInfo.AccepterTelePhone, color = "#000000" },
                        keyword5 = new { value = shipAddress, color = "#000000" },
                        keyword6 = new { value = goodsNames, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.独立小程序版订单取消通知:
                    postData = new
                    {
                        //订单编号,下单时间,商品名称,订单金额,状态更新
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },
                        keyword2 = new { value = orderInfo.AddTimeStr, color = "#000000" },
                        keyword3 = new { value = goodsNames, color = "#000000" },
                        keyword4 = new { value = orderInfo.BuyPriceStr, color = "#000000" },
                        keyword5 = new { value = "已取消", color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.独立小程序版订单退款通知:
                    postData = new
                    {
                        //下单时间,支付金额,退款金额,退款原因,退款时间,退款状态
                        keyword1 = new { value = orderInfo.AddTimeStr, color = "#000000" },
                        keyword2 = new { value = orderInfo.BuyPriceStr, color = "#000000" },
                        keyword3 = new { value = orderInfo.BuyPriceStr, color = "#000000" },
                        keyword4 = new { value = outOrderRemark, color = "#000000" },
                        keyword5 = new
                        {
                            value = orderInfo.RefundTime == Convert.ToDateTime("0001-01-01 00:00:00") ?
                                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : orderInfo.RefundTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            color = "#000000"
                        },
                        keyword6 = new { value = orderInfo.BuyMode == 1 ? "退款中" : "退款成功", color = "#000000" },
                    };
                    break;

                default:
                    postData = null;
                    break;
            }
            return postData;
        }

        #endregion 独立小程序版模板消息

        #region 企业智推版模板消息
        /// <summary>
        ///
        /// </summary>
        /// <param name="outOrderRemark">退款原因</param>
        /// <param name="orderInfo"></param>
        /// <param name="sendMsgType"></param>
        /// <returns></returns>
        public static object QiyeGetTemplateMessageData(QiyeGoodsOrder orderInfo, SendTemplateMessageTypeEnum sendMsgType, string outOrderRemark = "")
        {
            object postData = null;
            if (orderInfo == null)
            {
                return postData;
            }

            //状态
            string stateStr = orderInfo.StateStr;
            //商品名称
            string goodsNames = string.Empty;
            //发货地址
            string shipAddress = string.Empty;

            QiyeStore store = QiyeStoreBLL.SingleModel.GetModelByAId(orderInfo.AId);
            shipAddress = store?.Location;
            //购物车
            orderInfo.CartList = QiyeGoodsCartBLL.SingleModel.GetListByOrderIds(orderInfo.Id.ToString());
            if (orderInfo.CartList == null && orderInfo.CartList.Count <= 0)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"模板消息：{ orderInfo.OrderNum} |找不到订单明细"));
                return postData;
            }
            //商品
            string goodsids = string.Join(",", orderInfo.CartList.Select(s => s.GoodsId).Distinct());
            List<QiyeGoods> goodslist = QiyeGoodsBLL.SingleModel.GetListByIds(goodsids);
            foreach (QiyeGoodsCart cartitem in orderInfo.CartList)
            {
                QiyeGoods goods = goodslist?.FirstOrDefault(f => f.Id == cartitem.GoodsId);
                if (goods == null)
                {
                    log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"模板消息：{ orderInfo.OrderNum} |找不到商品详情，商品id：{cartitem.GoodsId}"));
                    continue;
                }
                if (!string.IsNullOrEmpty(cartitem.SpecInfo))
                {
                    goodsNames += $"{goods.Name}({cartitem.SpecInfo}),";
                }
                else
                {
                    goodsNames += $"{goods.Name}";
                }
            }
            goodsNames = goodsNames.TrimEnd(',');
            if (goodsNames.Length > 18) //截取，以防内容过多,模板消息变成空白
            {
                goodsNames = goodsNames.Substring(0, 18) + "...";
            }

            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.企业智推版订单支付成功通知:
                    postData = new
                    {
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },//订单号
                        keyword2 = new { value = orderInfo.AddTimeStr, color = "#000000" },//下单时间
                        keyword3 = new { value = goodsNames, color = "#000000" },//商品清单
                        keyword4 = new { value = orderInfo.PayTimeStr, color = "#000000" },//支付时间
                        keyword5 = new { value = orderInfo.BuyModeStr, color = "#000000" },//支付方式
                        keyword6 = new { value = orderInfo.BuyPriceStr, color = "#000000" },//支付金额
                        keyword7 = new { value = stateStr, color = "#000000" },//订单状态
                        keyword8 = new { value = orderInfo.AccepterName, color = "#000000" },//联系人姓名
                        keyword9 = new { value = orderInfo.AccepterTelePhone, color = "#000000" },//联系人手机号码
                    };

                    break;
                case SendTemplateMessageTypeEnum.企业智推版订单发货提醒:
                    postData = new
                    {
                        //发货时间,订单号,收货人,收货电话,发货地址,订单内容
                        keyword1 = new { value = orderInfo.SendTimeStr, color = "#000000" },
                        keyword2 = new { value = orderInfo.OrderNum, color = "#000000" },
                        keyword3 = new { value = orderInfo.AccepterName, color = "#000000" },
                        keyword4 = new { value = orderInfo.AccepterTelePhone, color = "#000000" },
                        keyword5 = new { value = shipAddress, color = "#000000" },
                        keyword6 = new { value = goodsNames, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.企业智推版订单取消通知:
                    postData = new
                    {
                        //订单编号,下单时间,商品名称,订单金额,状态更新
                        keyword1 = new { value = orderInfo.OrderNum, color = "#000000" },
                        keyword2 = new { value = orderInfo.AddTimeStr, color = "#000000" },
                        keyword3 = new { value = goodsNames, color = "#000000" },
                        keyword4 = new { value = orderInfo.BuyPriceStr, color = "#000000" },
                        keyword5 = new { value = "已取消", color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.企业智推版订单退款通知:
                    postData = new
                    {
                        //下单时间,支付金额,退款金额,退款原因,退款时间,退款状态
                        keyword1 = new { value = orderInfo.AddTimeStr, color = "#000000" },
                        keyword2 = new { value = orderInfo.BuyPriceStr, color = "#000000" },
                        keyword3 = new { value = orderInfo.BuyPriceStr, color = "#000000" },
                        keyword4 = new { value = outOrderRemark, color = "#000000" },
                        keyword5 = new
                        {
                            value = orderInfo.RefundTime == Convert.ToDateTime("0001-01-01 00:00:00") ?
                                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : orderInfo.RefundTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            color = "#000000"
                        },
                        keyword6 = new { value = orderInfo.BuyMode == 1 ? "退款中" : "退款成功", color = "#000000" },
                    };
                    break;

                default:
                    postData = null;
                    break;
            }
            return postData;
        }

        #endregion 独立小程序版模板消息

        #region 达达配送模板消息

        /// <summary>
        /// 拼团基础版 - 模板消息内容拼接
        /// </summary>
        /// <param name="outOrderRemark">退款原因</param>
        /// <param name="groupUser"></param>
        /// <param name="sendMsgType"></param>
        /// <returns></returns>
        public static object DadaGetTemplateMessageData(DadaOrder dadaorder, DateTime paytime, int paytype, string goodinfo, SendTemplateMessageTypeEnum sendMsgType)
        {
            object postData = new object();
            if (dadaorder == null)
            {
                return postData;
            }

            //支付方式
            string payTypeStr = string.Empty;
            switch (paytype)
            {
                case 1:
                    payTypeStr = "微信支付";
                    break;

                case 2:
                    payTypeStr = "储值支付";
                    break;
            }
            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.达达配送接单通知:
                    postData = new
                    {
                        keyword1 = new { value = dadaorder.origin_id, color = "#000000" }, //订单号码
                        keyword2 = new { value = goodinfo, color = "#000000" }, //商品清单
                        keyword3 = new { value = paytime.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" }, //支付时间
                        keyword4 = new { value = payTypeStr, color = "#000000" }, //支付方式
                        keyword5 = new { value = dadaorder.cargo_price, color = "#000000" }, //支付金额
                        keyword6 = new { value = "待取餐", color = "#000000" }, //订单状态
                        keyword7 = new { value = dadaorder.dm_name, color = "#000000" },//达达配送员
                        keyword8 = new { value = dadaorder.dm_mobile, color = "#000000" },//达达配送员手机号码
                        keyword9 = new { value = dadaorder.info, color = "#000000" },//订单备注
                    };
                    break;

                case SendTemplateMessageTypeEnum.达达配送配送中通知:
                    postData = new
                    {
                        keyword1 = new { value = dadaorder.origin_id, color = "#000000" }, //订单号码
                        keyword2 = new { value = goodinfo, color = "#000000" }, //商品清单
                        keyword3 = new { value = paytime.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" }, //支付时间
                        keyword4 = new { value = dadaorder.cargo_price, color = "#000000" }, //支付金额
                        keyword5 = new { value = dadaorder.update_time.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//配送时间
                        keyword6 = new { value = dadaorder.dm_name, color = "#000000" },//达达配送员
                        keyword7 = new { value = dadaorder.dm_mobile, color = "#000000" },//达达配送员电话
                        keyword8 = new { value = dadaorder.info, color = "#000000" },//订单备注
                    };
                    break;

                case SendTemplateMessageTypeEnum.达达配送已送达通知:
                    postData = new
                    {
                        keyword1 = new { value = dadaorder.origin_id, color = "#000000" },//订单号
                        keyword2 = new { value = goodinfo, color = "#000000" }, //商品清单
                        keyword3 = new { value = dadaorder.cargo_price, color = "#000000" }, //支付金额
                        keyword4 = new { value = dadaorder.update_time.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },//送达时间
                        keyword5 = new { value = dadaorder.dm_name, color = "#000000" },//达达配送员
                        keyword6 = new { value = dadaorder.dm_mobile, color = "#000000" },//达达配送员电话
                    };
                    break;

                default:

                    break;
            }

            return postData;
        }

        #endregion 达达配送模板消息

        #region 拼享惠模板消息
        /// <summary>
        /// 拼享惠模板消息内容拼接
        /// </summary>
        /// <param name="complaint"></param>
        /// <param name="sendMsgType"></param>
        /// <returns></returns>
        public static object PinGetTemplateMessageData(SendTemplateMessageTypeEnum sendMsgType, PinComplaint complaint = null, PinGoodsOrder order = null, PinStore store = null)
        {
            object postData = null;

            C_UserInfo userInfo = null;
            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.拼享惠发送申诉结果通知:
                    if (complaint == null)
                    {
                        return postData;
                    }
                    userInfo = C_UserInfoBLL.SingleModel.GetModel(complaint.userId);
                    if (userInfo == null)
                    {
                        return postData;
                    }
                    postData = new
                    {
                        keyword1 = new { value = userInfo.NickName, color = "#000000" },//申诉人
                        keyword2 = new { value = complaint.addTimeStr, color = "#000000" }, //发布时间
                        keyword3 = new { value = complaint.result, color = "#000000" }, //申诉结果
                    };
                    break;
                case SendTemplateMessageTypeEnum.拼享惠订单支付成功买家通知:
                    if (order == null || store == null)
                    {
                        return postData;
                    }

                    userInfo = C_UserInfoBLL.SingleModel.GetModel(order.userId);
                    if (userInfo == null)
                    {
                        return postData;
                    }
                    string menu = order.goodsPhotoModel.name;
                    if (order.specificationPhotoModel != null)
                    {
                        menu += $" {order.specificationPhotoModel.name}";
                    }
                    postData = new
                    {
                        keyword1 = new { value = order.outTradeNo, color = "#000000" },//订单号码
                        keyword2 = new { value = order.addtimeStr, color = "#000000" }, //下单时间
                        keyword3 = new { value = store.storeName, color = "#000000" },//商家名称
                        keyword4 = new { value = menu, color = "#000000" }, //商品清单
                        keyword5 = new { value = order.paytimeStr, color = "#000000" }, //支付时间
                        //keyword6 = new { value = order.paywayStr, color = "#000000" }, //支付方式
                        keyword6 = new { value = order.moneyStr, color = "#000000" }, //支付金额
                        keyword7 = new { value = order.stateStr, color = "#000000" }, //订单状态
                    };
                    break;
                case SendTemplateMessageTypeEnum.拼享惠订单取消通知:
                    if (order == null || store == null)
                    {
                        return postData;
                    }

                    userInfo = C_UserInfoBLL.SingleModel.GetModel(order.userId);
                    if (userInfo == null)
                    {
                        return postData;
                    }
                    menu = order.goodsPhotoModel.name;
                    if (order.specificationPhotoModel != null)
                    {
                        menu += $" {order.specificationPhotoModel.name}";
                    }
                    postData = new
                    {
                        keyword1 = new { value = order.outTradeNo, color = "#000000" },//订单号
                        keyword2 = new { value = store.storeName, color = "#000000" }, //商家名称
                        keyword3 = new { value = menu, color = "#000000" },//商品名称
                        keyword4 = new { value = order.addtimeStr, color = "#000000" }, //下单时间
                        keyword5 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" }, //取消时间
                        keyword6 = new { value = "商家取消交易", color = "#000000" }, //取消原因
                    };
                    break;
                case SendTemplateMessageTypeEnum.拼享惠退款通知:
                    if (order == null || store == null)
                    {
                        return postData;
                    }

                    userInfo = C_UserInfoBLL.SingleModel.GetModel(order.userId);
                    if (userInfo == null)
                    {
                        return postData;
                    }
                    menu = order.goodsPhotoModel.name;
                    if (order.specificationPhotoModel != null)
                    {
                        menu += $" {order.specificationPhotoModel.name}";
                    }
                    postData = new
                    {
                        keyword1 = new { value = order.outTradeNo, color = "#000000" },//订单号
                        keyword2 = new { value = store.storeName, color = "#000000" }, //退款商家
                        keyword3 = new { value = menu, color = "#000000" },//商品名称
                        keyword4 = new { value = order.moneyStr, color = "#000000" }, //退款金额
                        keyword5 = new { value = "商家操作退款，退款将会在1到7个工作日内到账", color = "#000000" }, //退款原因
                        keyword6 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" }, //退款时间
                        keyword7 = new { value = store.phone, color = "#000000" }//客服电话
                    };
                    break;
                case SendTemplateMessageTypeEnum.拼享惠订单配送通知:
                    if (order == null || store == null)
                    {
                        return postData;
                    }

                    userInfo = C_UserInfoBLL.SingleModel.GetModel(order.userId);
                    if (userInfo == null)
                    {
                        return postData;
                    }
                    menu = order.goodsPhotoModel.name;
                    if (order.specificationPhotoModel != null)
                    {
                        menu += $" {order.specificationPhotoModel.name}";
                    }
                    postData = new
                    {
                        keyword1 = new { value = order.outTradeNo, color = "#000000" },//订单编号
                        keyword2 = new { value = store.storeName, color = "#000000" }, //店铺名称
                        keyword3 = new { value = menu, color = "#000000" },//配送商品
                        keyword4 = new { value = order.address, color = "#000000" }, //配送地址
                        keyword5 = new { value = "已发货", color = "#000000" }, //配送状态
                        keyword6 = new { value = order.addtimeStr, color = "#000000" }, //下单时间
                    };
                    break;
            }
            return postData;
        }
        #endregion

        /// <summary>
        /// 预约模板消息
        /// </summary>
        public static object GetReservationTempMsgData(FoodReservation reserveInfo, SendTemplateMessageTypeEnum sendMsgType, EntGoodsOrder refundEntOrder = null, FoodGoodsOrder refundOrder = null)
        {
            string storeName = string.Empty;

            switch (reserveInfo.Type)
            {
                case (int)miniAppReserveType.到店扫码:
                case (int)miniAppReserveType.预约支付:
                    storeName = FoodBLL.SingleModel.GetModel(reserveInfo.FoodId)?.FoodsName;
                    break;

                case (int)miniAppReserveType.预约购物_专业版:
                    storeName = StoreBLL.SingleModel.GetModelByRid(reserveInfo.AId)?.name;
                    break;
            }
            //兼容旧店铺名存储逻辑
            if (string.IsNullOrWhiteSpace(storeName))
            {
                string appId = XcxAppAccountRelationBLL.SingleModel.GetModel(reserveInfo.AId)?.AppId;
                OpenAuthorizerConfig XUserList = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(appId, reserveInfo.AId);
                storeName = XUserList?.nick_name;
            }

            var postData = new object();
            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.预约点餐商家接单通知:
                    postData = new
                    {
                        keyword1 = new { value = reserveInfo.Id, color = "#000000" },
                        keyword2 = new { value = storeName, color = "#000000" },
                        keyword3 = new { value = reserveInfo.UserName, color = "#000000" },
                        keyword4 = new { value = reserveInfo.Contact, color = "#000000" },
                        keyword5 = new { value = reserveInfo.DinnerTime.ToString(), color = "#000000" },
                        keyword6 = new { value = reserveInfo.Note, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.预约点餐就座通知:
                    var orders = FoodGoodsOrderBLL.SingleModel.GetOrderByReservation(reserveInfo.Id);
                    var payfee = (orders.Sum(thisOrder => thisOrder.BuyPrice) * 0.01).ToString("F2");
                    var tableNo = FoodTableBLL.SingleModel.GetModel(reserveInfo.TableId)?.Scene;
                    postData = new
                    {
                        keyword1 = new { value = reserveInfo.Id, color = "#000000" },
                        keyword2 = new { value = reserveInfo.CreateDate.ToString(), color = "#000000" },
                        keyword3 = new { value = storeName, color = "#000000" },
                        keyword4 = new { value = reserveInfo.DinnerTime.ToString(), color = "#000000" },
                        keyword5 = new { value = reserveInfo.UserName, color = "#000000" },
                        keyword6 = new { value = reserveInfo.Contact, color = "#000000" },
                        keyword7 = new { value = "到店下单", color = "#000000" },
                        keyword8 = new { value = payfee, color = "#000000" },
                        keyword9 = new { value = tableNo, color = "#000000" },
                        keyword10 = new { value = reserveInfo.Note, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.预约点餐扫码就座通知:
                    postData = new
                    {
                        keyword1 = new { value = reserveInfo.Id, color = "#000000" },
                        keyword2 = new { value = reserveInfo.CreateDate.ToString(), color = "#000000" },
                        keyword3 = new { value = storeName, color = "#000000" },
                        keyword4 = new { value = reserveInfo.DinnerTime.ToString(), color = "#000000" },
                        keyword5 = new { value = reserveInfo.UserName, color = "#000000" },
                        keyword6 = new { value = reserveInfo.Contact, color = "#000000" },
                        keyword7 = new { value = "到店就餐", color = "#000000" },
                        keyword8 = new { value = reserveInfo.Note, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.预约点餐取消通知:
                    postData = new
                    {
                        keyword1 = new { value = reserveInfo.Id, color = "#000000" },
                        keyword2 = new { value = storeName, color = "#000000" },
                        keyword3 = new { value = reserveInfo.DinnerTime.ToString(), color = "#000000" },
                        keyword4 = new { value = reserveInfo.UserName, color = "#000000" },
                        keyword5 = new { value = reserveInfo.Contact, color = "#000000" },
                        keyword6 = new { value = reserveInfo.Note, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.预约点餐退款通知:
                    if (refundOrder == null)
                    {
                        return null;
                    }

                    var refundTips = refundOrder?.BuyMode == (int)miniAppBuyMode.储值支付 || refundEntOrder?.BuyMode == (int)miniAppBuyMode.储值支付 ? "金额已退回储值账号" : "退款金额1-7个工作日到账";
                    postData = new
                    {
                        keyword1 = new { value = reserveInfo.Id, color = "#000000" },
                        keyword2 = new { value = reserveInfo.CreateDate.ToString(), color = "#000000" },
                        keyword3 = new { value = storeName, color = "#000000" },
                        keyword4 = new { value = reserveInfo.DinnerTime.ToString(), color = "#000000" },
                        keyword5 = new { value = reserveInfo.UserName, color = "#000000" },
                        keyword6 = new { value = reserveInfo.Contact, color = "#000000" },
                        keyword7 = new { value = (refundOrder.BuyPrice * 0.01).ToString("F2"), color = "#000000" },
                        keyword8 = new { value = Enum.GetName(typeof(miniAppBuyMode), refundOrder.BuyMode) + "账号", color = "#000000" },
                        keyword9 = new { value = "预约退款", color = "#000000" },
                        keyword10 = new { value = refundTips, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.预约购物接单通知:
                    postData = new
                    {
                        keyword1 = new { value = reserveInfo.Id, color = "#000000" },
                        keyword2 = new { value = storeName, color = "#000000" },
                        keyword3 = new { value = "商家已接受您的预约", color = "#000000" },
                        keyword4 = new { value = reserveInfo.Contact, color = "#000000" },
                        keyword5 = new { value = reserveInfo.UserName, color = "#000000" },
                        keyword6 = new { value = reserveInfo.DinnerTime.ToString(), color = "#000000" },
                        keyword7 = new { value = "请在指定预约日期，到店自取", color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.预约购物自取通知:
                    postData = new
                    {
                        keyword1 = new { value = reserveInfo.Id, color = "#000000" },
                        keyword2 = new { value = reserveInfo.CreateDate.ToString(), color = "#000000" },
                        keyword3 = new { value = storeName, color = "#000000" },
                        keyword4 = new { value = reserveInfo.DinnerTime.ToString(), color = "#000000" },
                        keyword5 = new { value = reserveInfo.UserName, color = "#000000" },
                        keyword6 = new { value = reserveInfo.Contact, color = "#000000" },
                        keyword7 = new { value = "已到店自取成功", color = "#000000" },
                        keyword8 = new { value = reserveInfo.Note, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.预约购物退款通知:
                    if (refundEntOrder == null)
                    {
                        return null;
                    }
                    postData = new
                    {
                        keyword1 = new { value = reserveInfo.Id, color = "#000000" },
                        keyword2 = new { value = reserveInfo.CreateDate.ToString(), color = "#000000" },
                        keyword3 = new { value = storeName, color = "#000000" },
                        keyword4 = new { value = reserveInfo.DinnerTime.ToString(), color = "#000000" },
                        keyword5 = new { value = reserveInfo.UserName, color = "#000000" },
                        keyword6 = new { value = reserveInfo.Contact, color = "#000000" },
                        keyword7 = new { value = (refundEntOrder.BuyPrice * 0.01).ToString("F2"), color = "#000000" },
                        keyword8 = new { value = Enum.GetName(typeof(miniAppBuyMode), refundEntOrder.BuyMode) + "账号", color = "#000000" },
                        keyword9 = new { value = "预约退款", color = "#000000" },
                        keyword10 = new { value = "退款金额1-7个工作日到账", color = "#000000" },
                    };
                    break;

                default:
                    postData = null;
                    break;
            }
            return postData;
        }

        /// <summary>
        /// 退换货模板消息
        /// </summary>
        public static object GetReturnDeliveryTempMsgData(EntGoodsOrder order, SendTemplateMessageTypeEnum sendMsgType)
        {
            string storeName = string.Empty;

            Store store = StoreBLL.SingleModel.GetModelByAId(order.StoreId);
            store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);

            //商品名
            ReturnGoods returnInfo = ReturnGoodsBLL.SingleModel.GetByOrderId(order.Id);
            if (returnInfo.ReturnType == (int)ReturnGoodsType.专业版退换货 && !string.IsNullOrWhiteSpace(returnInfo.GoodsId))
            {
                order.goodsCarts = EntGoodsCartBLL.SingleModel.GetListByIds(returnInfo.GoodsId);
            }
            else if (returnInfo.ReturnType == (int)ReturnGoodsType.专业版退货退款)
            {
                order.goodsCarts = EntGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={order.Id}");
            }
            string goodNames = string.Join(",", order.goodsCarts.Select(cart => cart.GoodName));

            object postData = new object();
            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.退换货订单申请审核:
                    if (returnInfo == null)
                    {
                        log4net.LogHelper.WriteError(typeof(TemplateMsg_Gzh), new Exception($"退换货订单发送模板消息给用户失败：没有找到退换货信息，orderId:{order.Id}"));
                    }

                    //审核结果
                    string returnOrderState = string.Empty;
                    string tips = string.Empty;
                    if (order.State == (int)MiniAppEntOrderState.待退货)
                    {
                        returnOrderState = "商家同意退货";
                        tips = "商家已同意您的退货请求，请及时发货";
                    }
                    else if (order.State == (int)MiniAppEntOrderState.拒绝退货)
                    {
                        returnOrderState = "商家拒绝退货";
                        tips = "商家已拒绝您的退货请求，如有疑问请与商家联系";
                    }
                    //退货地址
                    string returnAddress = !string.IsNullOrWhiteSpace(store.funJoinModel.returnAddress) ? store.funJoinModel.returnAddress : "商家尚未填写退货地址，如有疑问请与商家联系";
                    postData = new
                    {
                        keyword1 = new { value = order.OrderNum, color = "#000000" },
                        keyword2 = new { value = returnInfo.AddTime, color = "#000000" },
                        keyword3 = new { value = returnOrderState, color = "#000000" },
                        keyword4 = new { value = goodNames, color = "#000000" },
                        keyword5 = new { value = returnAddress, color = "#000000" },
                        keyword6 = new { value = tips, color = "#000000" },
                    };
                    break;

                case SendTemplateMessageTypeEnum.退换货订单商家发货:
                    DeliveryFeedback deliveryInfo = DeliveryFeedbackBLL.SingleModel.GetOrderFeed(order.Id, DeliveryOrderType.专业版订单商家发货);
                    if (deliveryInfo == null)
                    {
                        log4net.LogHelper.WriteError(typeof(TemplateMsg_Gzh), new Exception($"退换货订单发送模板消息给用户失败：没有找到物流信息，orderId:{order.Id}"));
                    }
                    postData = new
                    {
                        keyword1 = new { value = DateTime.Now.ToString(), color = "#000000" },
                        keyword2 = new { value = order.OrderNum, color = "#000000" },
                        keyword3 = new { value = deliveryInfo.ContactName, color = "#000000" },
                        keyword4 = new { value = deliveryInfo.ContactTel, color = "#000000" },
                        keyword5 = new { value = deliveryInfo.Address, color = "#000000" },
                        keyword6 = new { value = goodNames, color = "#000000" },
                    };
                    break;

                default:
                    postData = null;
                    break;
            }
            return postData;
        }
        #endregion

        #region 第四步：发送模板消息

        /// <summary>
        /// 发送模板消息（通用）
        /// </summary>
        /// <param name="orderInfo"></param>
        /// <param name="templateMsgType"></param>
        /// <param name="temType"></param>
        /// <param name="postData"></param>
        public static void SendTemplateMessage(int userId, SendTemplateMessageTypeEnum templateMsgType, TmpType temType, object postData, string urlParam = "")
        {
            //兼容旧代码(部分模板已有了模板消息但不全,为统一代码方法,此处兼容)
            switch (templateMsgType)
            {
                case SendTemplateMessageTypeEnum.砍价订单支付成功提醒:
                    if (temType == TmpType.小程序电商模板)
                    {
                        templateMsgType = SendTemplateMessageTypeEnum.电商订单支付成功通知;
                    }
                    break;
            }
            SendTemplateMessage(userId, templateMsgType, (int)temType, postData, urlParam);
        }

        /// <summary>
        /// 发送模板消息
        /// </summary>
        /// <param name="groupUser"></param>
        /// <param name="templateMsgType"></param>
        /// <param name="temType"></param>
        /// <param name="postData"></param>
        /// <param name="urlParams">页面参数,传值方式eg:"?lat=0.01&lng=0.01"</param>
        public static void SendTemplateMessage(int userId, SendTemplateMessageTypeEnum templateMsgType, int tempType, object postData, string url = "", string bigKeyword = "")
        {
            string testMsg = "";
            TemplateMsg msg = TemplateMsgBLL.SingleModel.GetModelByTmgType(templateMsgType);
            if (msg == null)
            {
                return;
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Miniapp),$"发送模板消息：找不到用户信息_{userId}");
                return;
            }

            //用户有发送模板消息的机会才去发模板消息
            TemplateMsg_UserParam tmgup = TemplateMsg_UserParamBLL.SingleModel.GetModel($" Form_Id is not null and appId = '{userInfo.appId}'  and  Open_Id = '{userInfo.OpenId}' and State = 1 and LoseDateTime > now() ");
            if (tmgup != null)
            {
                TemplateMsg_User tmgu = TemplateMsg_UserBLL.SingleModel.GetModel($" AppId = '{tmgup.AppId}' and Ttypeid = {tempType} and TmgType = {(int)templateMsgType}");
                //商家若未开启过模板消息，那么默认帮商家开启
                if (tmgu == null)
                {
                    AddNewTemplate_User(tmgup.AppId, templateMsgType, ref tmgu);
                }
                else if (string.IsNullOrEmpty(tmgu.TemplateId))
                {
                    TemplateMsg_UserBLL.SingleModel.Delete(tmgu.Id);
                    AddNewTemplate_User(tmgup.AppId, templateMsgType, ref tmgu);
                }
                
                if (tmgu != null && tmgu.State == 1)
                {
                    string templateMsg = string.Empty;
                    if (postData != null)
                    {
                        //数据库所有模板都没有添加PageUrl  这里弃用msg.PageUrl 以防止数据不一致
                        MyMsnReturenModel_result result = MsnModelHelper.sendMyMsn(userInfo.appId, userInfo.OpenId, tmgu.TemplateId, url, tmgup.Form_Id, postData, string.Empty, bigKeyword, ref testMsg);
                        if(result.errcode==0)
                        {
                            //参数使用次数增加(默认是1)
                            TemplateMsg_UserParamBLL.SingleModel.addUsingCount(tmgup);
                        }
                        else //if(result.errcode==41028 || result.errcode== 41029)
                        {
                            tmgup.State = -1;
                            TemplateMsg_UserParamBLL.SingleModel.Update(tmgup);
                            SendTemplateMessage(userId,templateMsgType,tempType,postData,url,bigKeyword);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 发送砍价模板消息
        /// </summary>
        /// <param name="groupUser"></param>
        /// <param name="templateMsgType"></param>
        /// <param name="temType"></param>
        /// <param name="postData"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        public static void SendTemplateMessage(BargainUser bargainUser, SendTemplateMessageTypeEnum templateMsgType, object postData)
        {
            int aId = 0;
            int tempType = 0; //模板类型
            string storeName = string.Empty;

            #region 拿到aId,拿到storeName

            Bargain bargain = BargainBLL.SingleModel.GetModel(bargainUser.BId);
            if (bargain != null)
            {
                switch (bargain.BargainType)
                {
                    case 0:
                        Store store = StoreBLL.SingleModel.GetModel(bargain.StoreId);
                        if (store != null)
                        {
                            aId = store.appId;
                            var paramslist = ConfParamBLL.SingleModel.GetListByRId(Convert.ToInt32(aId)) ?? new List<ConfParam>();
                            storeName = paramslist.Where(w => w.Param == "nparam").FirstOrDefault()?.Value;
                        }
                        break;

                    default:
                        aId = bargain.StoreId;
                        break;
                }
            }

            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModel(aId);
            if (app == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"发送砍价失败,基础参数不足 app_null:aId = {aId}"));
                return;
            }
            storeName = XcxAppAccountRelationBLL.SingleModel.GetStoreName(storeName, app);

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(app.TId);
            if (xcxTemplate == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"发送砍价失败,基础参数不足 xcxTemplate_null:app.TId = {app.TId}"));
                return;
            }
            tempType = xcxTemplate.Type;

            #endregion 拿到aId,拿到storeName

            //兼容旧代码(部分模板已有了模板消息但不全,为统一代码方法,此处兼容)
            switch (templateMsgType)
            {
                case SendTemplateMessageTypeEnum.砍价订单支付成功提醒:
                    if (tempType == (int)TmpType.小程序电商模板)
                    {
                        templateMsgType = SendTemplateMessageTypeEnum.电商订单支付成功通知;
                    }
                    break;
            }
            SendTemplateMessage(bargainUser.UserId, templateMsgType, tempType, postData);
        }

        /// <summary>
        /// 发送订阅模板消息
        /// </summary>
        /// <param name="groupUser"></param>
        /// <param name="templateMsgType"></param>
        /// <param name="temType"></param>
        /// <param name="postData"></param>
        /// <param name="urlParams">页面参数,传值方式eg:"?lat=0.01&lng=0.01"</param>
        public static bool SendSubscribeMessage(int userId, SendTemplateMessageTypeEnum templateMsgType, TmpType tmpType, object postData, out string errorMsg, string urlParams = "")
        {
            string testMsg = string.Empty;
            TemplateMsg msg = TemplateMsgBLL.SingleModel.GetModelByTmgType(templateMsgType);
            if (msg == null)
            {
                errorMsg = $"数据库找不到[{tmpType}-{templateMsgType}]该消息模板";
                return false;
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                errorMsg = $"用户不存在_userId({userId})[{tmpType}-{templateMsgType}]";
                return false;
            }

            //用户有发送模板消息的机会才去发模板消息
            TemplateMsg_UserParam tmgup = TemplateMsg_UserParamBLL.SingleModel.GetModel($" Form_Id is not null and appId = '{userInfo.appId}'  and  Open_Id = '{userInfo.OpenId}' and State = 1 and LoseDateTime > now() ");
            if (tmgup == null)
            {
                errorMsg = $"FormId不足_userId({userId})[{tmpType}-{templateMsgType}]";
                return false;
            }

            TemplateMsg_User tmgu = TemplateMsg_UserBLL.SingleModel.GetModel($" AppId = '{tmgup.AppId}' and Ttypeid = {(int)tmpType} and TmgType = {(int)templateMsgType} ");
            //商家若未开启过模板消息，那么默认帮商家开启
            if (tmgu == null)
            {
                AddNewTemplate_User(tmgup.AppId, templateMsgType, ref tmgu);
            }
            //模板消息不存在/开通失败
            if (tmgu == null)
            {
                errorMsg = $"模板消息不存在/开通失败({tmpType}-{templateMsgType})";
                return false;
            }
            //模板消息已关闭
            if (tmgu.State == 0)
            {
                errorMsg = $"模板消息已关闭({tmpType}-{templateMsgType})";
                return false;
            }
            //发送内容为空
            if (postData == null)
            {
                errorMsg = $"发送内容为空({tmpType}-{templateMsgType})";
                return false;
            }
            //发送消息
            MyMsnReturenModel_result result = MsnModelHelper.sendMyMsn(userInfo.appId, userInfo.OpenId, tmgu.TemplateId, msg.PageUrl + urlParams, tmgup.Form_Id, postData, string.Empty, string.Empty, ref testMsg);
            //参数使用次数增加(默认是1)
            TemplateMsg_UserParamBLL.SingleModel.addUsingCount(tmgup);
            //微信接口返回结果
            if (new List<int>() { 40037, 41028, 41029, 41030, 45009 }.IndexOf(result.errcode) > -1)
            {
                //微信返回错误，记录错误码&错误信息
                errorMsg = $"errorCode:{result.errcode},msg: {result.errmsg}";
                return false;
            }
            errorMsg = result.errmsg;
            return true;
        }
        #endregion
    }
}