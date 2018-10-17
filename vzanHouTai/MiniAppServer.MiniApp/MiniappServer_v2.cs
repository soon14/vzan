using System.IO;
using BLL.MiniApp;
using Core.MiniApp;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Timers;
using log4net;
using System.Configuration;
using BLL.MiniApp.Home;
using Entity.MiniApp.Home;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Tools;
using Entity.MiniApp.Tools;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Stores;
using BLL.MiniApp.Helper;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Dish;
using BLL.MiniApp.Dish;
using BLL.MiniApp.CrmApi;
using BLL.MiniApp.Plat;
using BLL.MiniApp.PlatChild;
using BLL.MiniApp.Pin;
using BLL.MiniApp.Qiye;
using BLL.MiniApp.User;
using Entity.MiniApp.User;
using BLL.MiniApp.Im;
using Utility;

namespace MiniAppServer.MiniApp
{
    public partial class MiniAppServer_v2 : ServiceBase
    {
        #region
        private Timer _miniAppOrderConfTimer;
        private Timer _miniAppCancleOrderTimer;
        private Timer _miniAppOutOrderResultTimer;
        //电商
        private Timer _storeGoodsOrderTimeOutTimer;
        /// <summary>
        /// 小程序商品砍价
        /// </summary>
        private Timer _miniAppBargainOutOrderResultTimer;
        /// <summary>
        /// 小程序砍价商品超时未支付订单
        /// </summary>
        private Timer _miniAppBargainCancleOrderTimer;
              
        
        //拼团过期状态更新
        private Timer _outTimeGroupSTimer;
        //10天后自动完成拼团订单
        private Timer _successGroupOrderTimer;
        //半小时未支付取消支付
        private Timer _noPayGroupOrderTimer;
        //足浴
        private Timer _footbathTimeOutTimer;
        private Timer _footbathTimeOverTimer;
        private Timer _footbathTimeStartTimer;
        //小程序提交代码服务
        private Timer _miniappsumitcodeTimer;
        //模板消息自动开启服务
        private Timer _autoOpenTemplateMsgTimer;
        //智慧餐厅 - 凌晨5-6点自动清理排队数据
        private Timer _autoClearDishQueueUpTimer;
        //智慧餐厅 - 订单过期变成已取消
        private Timer _cancelOrderTimer;
        //订单15天后默认好评
        private Timer _autoCommentOrderTimer;
        //物流
        /// <summary>
        /// 物流信息实时查询
        /// </summary>
        private Timer _deliveryRealTimeTraceTimer;
        /// <summary>
        /// 物流轨迹订阅推送
        /// </summary>
        private Timer _deliverySubscribeTimer;
        /// <summary>
        /// 物流轨迹订阅同步
        /// </summary>
        private Timer _deliverySubscribeSyncTimer;
        /// <summary>
        /// Crm系统服务
        /// </summary>
        private Timer _crmStartTimer;
        /// <summary>
        /// 代理推广分销服务
        /// </summary>
        private Timer _agentDistributionStartTimer;
        /// <summary>
        /// 小程序统计
        /// </summary>
        private Timer _statisticalFlowTimer;
        /// <summary>
        /// 独立模板服务
        /// </summary>
        private Timer _platChildTimer;
        /// <summary>
        /// 秒杀活动到期服务
        /// </summary>
        private Timer _flashDealExpireTimer;
        /// <summary>
        /// 秒杀活动倒计时服务
        /// </summary>
        private Timer _flashDealCountdownTimer;
        /// <summary>
        /// 订阅模板消息发送服务
        /// </summary>
        private Timer _subscribeMsgTimer;
        /// <summary>
        /// 企业智推版
        /// </summary>
        private Timer _qiyeTimer;
        /// <summary>
        /// 专业版申请取消订单服务
        /// </summary>
        private Timer _entApplyCancelOrderTimer;

        //拼享惠订单30分钟未支付取消订单回滚库存
        private Timer _pinOrderTimer;
        //拼享惠订单自动交易完成
        private Timer _pinOrderSuccessTimer;
        /// <summary>
        /// 拼享惠检查拼团成功
        /// </summary>
        private Timer _pinGroupSuccessTimer;
        /// <summary>
        /// 拼享惠检查拼团失败
        /// </summary>
        private Timer _pinGroupTimeoutTimer;
        /// <summary>
        /// 用户跟踪记录统计服务
        /// </summary>
        private Timer _userTrackTimer;
        /// <summary>
        /// 私信记录同步服务
        /// </summary>
        private Timer _ImAddRecordTimer;
        #endregion

        public MiniAppServer_v2()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 拼团过期状态更新轮询间隔
        /// </summary>
        public int OutTimeGroupSTimerInterval
        {
            get
            {
                int time = 0;
                string intervaltime = System.Configuration.ConfigurationManager.AppSettings["OutTimeGroupSTimerInterval"];
                if (!string.IsNullOrEmpty(intervaltime))
                {
                    int.TryParse(intervaltime, out time);
                }
                if (time == 0)
                {
                    time = 100;
                }
                return 1000 * time;
            }
        }

        /// <summary>
        /// 流量统计，1个小时跑一次
        /// </summary>
        public int OutTimeCouponIncomeTimerInterval
        {
            get
            {
                int time = 0;
                string intervaltime = System.Configuration.ConfigurationManager.AppSettings["OutTimeCouponIncomeTimerInterval"];
                if (!string.IsNullOrEmpty(intervaltime))
                {
                    int.TryParse(intervaltime, out time);
                }
                if (time == 0)
                {
                    time = 60 * 60;
                }
                int threadPauseInterval = time * 1000;
                return threadPauseInterval;
            }
        }

        protected override void OnStart(string[] args)
        {
            WriteLog("服务开启");

            //足浴订单超时没有开始服务
            _footbathTimeOutTimer = new Timer(10 * 1000);
            _footbathTimeOutTimer.Elapsed += FootbathTimeOutTimer_Event;
            _footbathTimeOutTimer.Enabled = true;

            //足浴服务结束订单自动完成
            _footbathTimeOverTimer = new Timer(10 * 1000);
            _footbathTimeOverTimer.Elapsed += FootbathTimeOverTimer_Event;
            _footbathTimeOverTimer.Enabled = true;

            //足浴提前半小时提醒用户到场享受服务
            _footbathTimeStartTimer = new Timer(60 * 1000);
            _footbathTimeStartTimer.Elapsed += FootbathTimeStartTimer_Event;
            _footbathTimeStartTimer.Enabled = true;


            //更新小程序代码
            //_uploadMiniappCodeTimer = new Timer(60 * 1000);
            //_uploadMiniappCodeTimer.Elapsed += UploadMiniAppCode_Event;
            //_uploadMiniappCodeTimer.Enabled = true;



            //小程序订单自动完成
            _miniAppOrderConfTimer = new Timer(60 * 1000);
            _miniAppOrderConfTimer.Elapsed += _xcxGoodsOrderConfTimer_Event;
            _miniAppOrderConfTimer.Enabled = true;

            //小程序订单自动取消
            _miniAppCancleOrderTimer = new Timer(10 * 1000);
            _miniAppCancleOrderTimer.Elapsed += _xcxGoodsOrderCancleTimer_Event;
            _miniAppCancleOrderTimer.Enabled = true;

            //小程序订单获取退款结果
            _miniAppOutOrderResultTimer = new Timer(10 * 1000);
            _miniAppOutOrderResultTimer.Elapsed += _xcxGoodsOutOrderTimer_Event;
            _miniAppOutOrderResultTimer.Enabled = true;


            //小程序砍价订单获取退款结果
            _miniAppBargainOutOrderResultTimer = new Timer(10 * 1000);
            _miniAppBargainOutOrderResultTimer.Elapsed += _xcxBargainOutOrderTimer_Event;
            _miniAppBargainOutOrderResultTimer.Enabled = true;


            //小程序砍价订单超时未支付自动取消
            _miniAppBargainCancleOrderTimer = new Timer(10 * 1000);
            _miniAppBargainCancleOrderTimer.Elapsed += _xcxBargainOrderCancleTimer_Event;
            _miniAppBargainCancleOrderTimer.Enabled = true;

            //拼团过期状态更新
            _outTimeGroupSTimer = new Timer(OutTimeGroupSTimerInterval);
            _outTimeGroupSTimer.Elapsed += GroupSLongTimer_Event;
            _outTimeGroupSTimer.Enabled = true;

            //10天后拼团自动完成订单
            _successGroupOrderTimer = new Timer(10 * 1000);
            _successGroupOrderTimer.Elapsed += SuccessGroupOrderTimer_Event;
            _successGroupOrderTimer.Enabled = true;

            //半小时后拼团没支付自动取消，并返回库存
            _noPayGroupOrderTimer = new Timer(60 * 1000);
            _noPayGroupOrderTimer.Elapsed += NoPayGroupOrderTimer_Event;
            _noPayGroupOrderTimer.Enabled = true;

            //电商订单过期更新
            _storeGoodsOrderTimeOutTimer = new Timer(60 * 1000);
            _storeGoodsOrderTimeOutTimer.Elapsed += StoreGoodsOrderTimeOutTimer_Event;
            _storeGoodsOrderTimeOutTimer.Enabled = true;

            //小程序提交代码服务，10分钟跑一次
            _miniappsumitcodeTimer = new Timer(10 * 60 * 1000);
            _miniappsumitcodeTimer.Elapsed += MiniappSubmitCodeTimer_Event;
            _miniappsumitcodeTimer.Enabled = true;

            //模板消息自动开启服务,1分钟跑1次
            //_autoOpenTemplateMsgTimer = new Timer(60 * 1000);
            //_autoOpenTemplateMsgTimer.Elapsed += AutoOpenTemplateMsgTimer_Event;
            //_autoOpenTemplateMsgTimer.Enabled = true;

            //物流信息实时查询服务,1分钟跑1次
            _deliveryRealTimeTraceTimer = new Timer(60 * 1000);
            _deliveryRealTimeTraceTimer.Elapsed += DeliveryRealTime_Event;
            _deliveryRealTimeTraceTimer.Enabled = true;

            ////物流轨迹订阅推送服务,1分钟跑1次
            _deliverySubscribeTimer = new Timer(60 * 1000);
            _deliverySubscribeTimer.Elapsed += DeliverySubscribe_Event;
            _deliverySubscribeTimer.Enabled = true;

            //物流轨迹订阅同步服务,1分钟跑1次
            _deliverySubscribeSyncTimer = new Timer(60 * 1000);
            _deliverySubscribeSyncTimer.Elapsed += DeliverySubscribeSync_Event;
            //_deliveryRealTimeTraceTimer.Interval = TimeSpan.FromMinutes(10).TotalSeconds;
            _deliverySubscribeSyncTimer.Enabled = true;

            //清理 餐饮多门店 排队数据
            _autoClearDishQueueUpTimer = new Timer(60 * 60 * 1000);
            _autoClearDishQueueUpTimer.Elapsed += AutoClearDishQueueUpTimer_Event;
            _autoClearDishQueueUpTimer.Enabled = true;

            //自动取消订单
            _cancelOrderTimer = new Timer(60 * 1000);
            _cancelOrderTimer.Elapsed += AutoCancelOrderTimer_Event;
            _cancelOrderTimer.Enabled = true;

            //订单15天后默认好评
            _autoCommentOrderTimer = new Timer(60 * 1000);
            _autoCommentOrderTimer.Elapsed += AutoCommentOrderTimer_Event;
            _autoCommentOrderTimer.Enabled = true;

            //Crm系统服务
            _crmStartTimer = new Timer(60 * 1000);
            _crmStartTimer.Elapsed += CrmStartTimer_Event;
            _crmStartTimer.Enabled = true;

            //代理推广分销服务，10分钟间隔
            _agentDistributionStartTimer = new Timer(60 * 1000 * 10);
            _agentDistributionStartTimer.Elapsed += AgentDistributionStartTimer_Event;
            _agentDistributionStartTimer.Enabled = true;

            //小程序统计流量
            _statisticalFlowTimer = new Timer(60 * 1000);
            _statisticalFlowTimer.Elapsed += StatisticalFlowTimer_Event;
            _statisticalFlowTimer.Enabled = true;

            //独立模板服务
            _platChildTimer = new Timer(60 * 1000 * 10);
            _platChildTimer.Elapsed += PlatChildCancelOrderTimer_Event;
            _platChildTimer.Enabled = true;

            //秒杀活动过期服务
            _flashDealExpireTimer = new Timer(60 * 1000);
            _flashDealExpireTimer.Elapsed += FlashDealExpireTimer_Event;
            _flashDealExpireTimer.Enabled = true;

            //秒杀活动开始服务
            _flashDealCountdownTimer = new Timer(10 * 1000);
            _flashDealCountdownTimer.Elapsed += FlashDealCountdownTimer_Event;
            _flashDealCountdownTimer.Enabled = true;

            //拼享惠订单30分钟未支付取消订单回滚库存
            _pinOrderTimer = new Timer(60 * 1000);
            _pinOrderTimer.Elapsed += PinCancelOrderTimer_Event;
            _pinOrderTimer.Enabled = true;

            //拼享惠检查拼团成功返利
            _pinGroupSuccessTimer = new Timer(60 * 1000 * 5);
            _pinGroupSuccessTimer.Elapsed += PinCheckGroupSuccess_Event;
            _pinGroupSuccessTimer.Enabled = true;

            //拼享惠检查拼团失败
            _pinGroupTimeoutTimer = new Timer(60 * 1000 * 5);
            _pinGroupTimeoutTimer.Elapsed += PinCheckGroupTimeout_Event;
            _pinGroupTimeoutTimer.Enabled = true;

            //拼享惠自动交易完成
            _pinOrderSuccessTimer = new Timer(60 * 1000 * 5);
            _pinOrderSuccessTimer.Elapsed += PinOrderSuccess_Event;
            _pinOrderSuccessTimer.Enabled = true;
            //订阅模板消息发送服务
            _subscribeMsgTimer = new Timer(10 * 1000);
            _subscribeMsgTimer.Elapsed += SubscribeMsgTimer_Event;
            _subscribeMsgTimer.Enabled = true;

            //企业智推版服务
            _qiyeTimer = new Timer(60 * 1000 * 10);
            _qiyeTimer.Elapsed += QiyeTimer_Event;
            _qiyeTimer.Enabled = true;

            //用户足迹跟踪记录统计服务
            _userTrackTimer = new Timer(10 * 1000);
            _userTrackTimer.Elapsed += UserTrackTimer_Event;
            _userTrackTimer.Enabled = true;
            //用户足迹跟踪记录统计服务
            _ImAddRecordTimer = new Timer(60 * 1000);
            _ImAddRecordTimer.Elapsed += AddRecordTimer_Event;
            _ImAddRecordTimer.Enabled = true;

            //专业版申请取消订单服务(间隔10分钟)
            _entApplyCancelOrderTimer = new Timer(10*60 * 1000);
            _entApplyCancelOrderTimer.Elapsed += EntApplyCancelOrderTimer_Event;
            _entApplyCancelOrderTimer.Enabled = true;
        }

        /// <summary>
        /// 小程序取消订单
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void _xcxGoodsOrderCancleTimer_Event(object source, ElapsedEventArgs e)
        {
            _miniAppCancleOrderTimer.Enabled = false;
            _miniAppCancleOrderTimer.Stop();

            try
            {
                #region 小程序餐饮 - 1 小时未付款,则取消订单
                FoodGoodsOrderBLL.SingleModel.updateOrderStateForCancle();
                #endregion

                #region 小程序行业高级版订单 - 2 小时未付款,则取消订单 
                EntGoodsOrderBLL.SingleModel.updateOrderStateForCancle();
                #endregion
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(GetType(), ex);
            }
            finally
            {
                _miniAppCancleOrderTimer.Enabled = true;
                _miniAppCancleOrderTimer.Start();
            }
        }

        /// <summary>
        /// 小程序餐饮2小时自动完成,专业版订单10后自动完成，电商订单10天后自动完成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void _xcxGoodsOrderConfTimer_Event(object source, ElapsedEventArgs e)
        {
            _miniAppOrderConfTimer.Enabled = false;
            _miniAppOrderConfTimer.Stop();

            try
            {
                #region 小程序餐饮 - 待就餐,待送达订单 2小时 自动完成
                FoodGoodsOrderBLL.SingleModel.updateOrderStateForComplete();
                #endregion

                #region 小程序餐饮 - 付款中订单 2小时 自动取消
                FoodGoodsOrderBLL.SingleModel.updateOrderStateForCancleByPay();
                #endregion

                #region 小程序行业高级版订单 - 10天未确认收货,自动完成
                //10天未确认收货,自动完成
                EntGoodsOrderBLL.SingleModel.updateOrderStateForComplete();
                #endregion

                #region 小程序电商 - 10天未确认收货,自动完成
                
                //10天未确认收货,自动完成
                StoreGoodsOrderBLL.SingleModel.updateOrderStateForComplete();
                #endregion 

            }
            catch (Exception ex)
            {
                LogHelper.WriteError(GetType(), new Exception("小程序订单自动完成服务执行出错" + ex.Message));
            }
            finally
            {
                _miniAppOrderConfTimer.Enabled = true;
                _miniAppOrderConfTimer.Start();
            }
        }

        /// <summary>
        /// 小程序商品订单过期（订单超过30分钟取消订单）-蔡华兴
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void StoreGoodsOrderTimeOutTimer_Event(object source, ElapsedEventArgs e)
        {
            _storeGoodsOrderTimeOutTimer.Enabled = false;
            _storeGoodsOrderTimeOutTimer.Stop();

            try
            {
                #region 小程序商城
                
                StoreGoodsOrderBLL.SingleModel.StoreGoodsOrderTimeOut();
                #endregion
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(GetType(), new Exception("同城商品订单过期 服务执行出错" + ex.Message));
            }
            finally
            {
                _storeGoodsOrderTimeOutTimer.Enabled = true;
                _storeGoodsOrderTimeOutTimer.Start();
            }

        }

        /// <summary>
        /// 退款状态跟进
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void _xcxGoodsOutOrderTimer_Event(object source, ElapsedEventArgs e)
        {
            _miniAppOutOrderResultTimer.Enabled = false;
            _miniAppOutOrderResultTimer.Stop();

            try
            {
                #region 小程序餐饮 - 退款状态跟进
                
                FoodGoodsOrderBLL.SingleModel.updateOutOrderState();
                #endregion


                #region 小程序行业高级版 - 退款状态跟进
                EntGoodsOrderBLL.SingleModel.UpdateReturnOrderState();
                #endregion

            }
            catch (Exception ex)
            {
                LogHelper.WriteError(GetType(), new Exception("小程序FoodGoodsOrder,EntGoodsOrder退款状态跟进 服务执行出错" + ex.Message));
            }
            finally
            {
                _miniAppOutOrderResultTimer.Enabled = true;
                _miniAppOutOrderResultTimer.Start();
            }
        }

        /// <summary>
        /// 小程序砍价退款状态跟进
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void _xcxBargainOutOrderTimer_Event(object source, ElapsedEventArgs e)
        {
            _miniAppBargainOutOrderResultTimer.Enabled = false;
            _miniAppBargainOutOrderResultTimer.Stop();

            try
            {
                #region 小程序商品砍价 - 退款状态跟进
                
                BargainUserBLL.SingleModel.updateOutOrderState();
                #endregion

            }
            catch (Exception ex)
            {
                LogHelper.WriteError(GetType(), new Exception("小程序商品砍价退款状态跟进 服务执行出错" + ex.Message));
            }
            finally
            {
                _miniAppBargainOutOrderResultTimer.Enabled = true;
                _miniAppBargainOutOrderResultTimer.Start();
            }
        }

        /// <summary>
        /// 小程序砍价超时未支付自动取消订单回库存
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void _xcxBargainOrderCancleTimer_Event(object source, ElapsedEventArgs e)
        {
            _miniAppBargainCancleOrderTimer.Enabled = false;
            _miniAppBargainCancleOrderTimer.Stop();

            try
            {
                #region 小程序砍价 - 1 小时未付款,则取消订单回库存
                
                BargainUserBLL.SingleModel.updateOrderStateForCancle();

                #endregion

            }
            catch (Exception ex)
            {
                LogHelper.WriteError(GetType(), new Exception("小程序砍价超时未支付订单取消回库存 服务执行出错" + ex.Message));
            }
            finally
            {
                _miniAppBargainCancleOrderTimer.Enabled = true;
                _miniAppBargainCancleOrderTimer.Start();
            }
        }

        /// <summary>
        /// 足浴 - 订单超时没有开始服务
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void FootbathTimeOutTimer_Event(object source, ElapsedEventArgs e)
        {

            _footbathTimeOutTimer.Enabled = false;
            _footbathTimeOutTimer.Stop();

            try
            {
                #region 小程序足浴 - 预订单超时
                EntGoodsOrderBLL.SingleModel.timeOutFootBathOrder();
                #endregion

            }
            catch (Exception ex)
            {
                LogHelper.WriteError(GetType(), new Exception("足浴 - 订单超时没有开始服务 服务执行出错" + ex.Message));
            }
            finally
            {
                _footbathTimeOutTimer.Enabled = true;
                _footbathTimeOutTimer.Start();
            }
        }

        /// <summary>
        /// 足浴 - 预约开始前半小时自动提醒
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void FootbathTimeStartTimer_Event(object source, ElapsedEventArgs e)
        {

            _footbathTimeStartTimer.Enabled = false;
            _footbathTimeStartTimer.Stop();

            try
            {
                #region 小程序足浴 - 预约开始前半小时自动提醒
                EntGoodsOrderBLL.SingleModel.timeReadyFootBathOrder();
                #endregion

            }
            catch (Exception ex)
            {
                LogHelper.WriteError(GetType(), new Exception("足浴 - 预约开始前半小时自动提醒 服务执行出错" + ex.Message));
            }
            finally
            {
                _footbathTimeStartTimer.Enabled = true;
                _footbathTimeStartTimer.Start();
            }
        }
        /// <summary>
        /// 足浴 - 订单服务到时间自动完成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void FootbathTimeOverTimer_Event(object source, ElapsedEventArgs e)
        {

            _footbathTimeOverTimer.Enabled = false;
            _footbathTimeOverTimer.Stop();

            try
            {
                #region 小程序足浴 - 自动完成订单
                EntGoodsOrderBLL.SingleModel.timeOverFootBathOrder();
                #endregion

            }
            catch (Exception ex)
            {
                LogHelper.WriteError(GetType(), new Exception("足浴 - 订单服务到时间自动完成 服务执行出错" + ex.Message));
            }
            finally
            {
                _footbathTimeOverTimer.Enabled = true;
                _footbathTimeOverTimer.Start();
            }
        }
        
        /// <summary>
        /// 处理拼团数据过期更新状态并退款
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GroupSLongTimer_Event(object sender, ElapsedEventArgs e)
        {
            _outTimeGroupSTimer.Enabled = false;
            _outTimeGroupSTimer.Stop();

            try
            {
                #region 团购
                string strWhere = " State=1 and EndDate < NOW()";
                List<GroupSponsor> listPost = GroupSponsorBLL.SingleModel.GetList(strWhere, 500, 1);
                if (listPost != null && listPost.Count > 0)
                {
                    foreach (GroupSponsor item in listPost)
                    {
                        List<GroupUser> groupUserList = GroupUserBLL.SingleModel.GetList($"GroupId={item.GroupId} and GroupSponsorId={item.Id} and IsGroup=1");

                        if (!groupUserList.Any())
                        {
                            WriteLog("小程序拼团服务找不到可以退款的用户"+item.Id);
                            continue;
                        }

                        if (item.GroupSize != groupUserList.Count)
                        {
                            {
                                TransactionModel tranmodel = new TransactionModel();
                                tranmodel.Add($"update  GroupSponsor set state={(int)GroupState.已过期} where Id={item.Id}");

                                foreach (GroupUser groupUser in groupUserList)
                                {
                                    string msg = "";
                                    //退款
                                    if (!GroupUserBLL.SingleModel.RefundOne(groupUser, ref tranmodel, ref msg, 0))
                                    {
                                        WriteLog("ID【" + groupUser.Id + "】插入小程序拼团退款队列失败" + msg);
                                        break;
                                    }
                                }
                                if (!GroupUserBLL.SingleModel.ExecuteTransactionDataCorect(tranmodel.sqlArray, tranmodel.ParameterArray))
                                {
                                    WriteLog("xxxxxxxxxxxxxxxxxx拼团退款事务执行失败，ID=" + item.Id);
                                }
                            }
                        }
                    }
                }

                #endregion

                #region 专业版拼团

                List<EntGroupSponsor> entListPost = EntGroupSponsorBLL.SingleModel.GetGroupList((int)TmpType.小程序专业模板);
                if (entListPost != null && entListPost.Count > 0)
                {
                    foreach (EntGroupSponsor item in entListPost)
                    {
                        List<EntGoodsOrder> groupUserList = EntGoodsOrderBLL.SingleModel.GetList($"GroupId={item.Id} and ordertype=3 and state in ({(int)MiniAppEntOrderState.待发货},{(int)MiniAppEntOrderState.待自取})");
                        if (!groupUserList.Any())
                        {
                            WriteLog($"小程序专业版拼团服务找不到可以退款的用户{item.Id}");
                            item.State = (int)GroupState.已过期;
                            EntGroupSponsorBLL.SingleModel.Update(item, "State");
                            continue;
                        }
                        if (item.GroupSize != groupUserList.Count)
                        {
                            item.State = (int)GroupState.已过期;
                            if (EntGroupSponsorBLL.SingleModel.Update(item, "State"))
                            {
                                WriteLog("更新专业版拼团ID【" + item.Id + "】过期成功");

                                foreach (EntGoodsOrder groupUser in groupUserList)
                                {
                                    //退款
                                    if (EntGoodsOrderBLL.SingleModel.outOrder(groupUser, groupUser.State, groupUser.BuyMode))
                                    {
                                        WriteLog("ID【" + groupUser.Id + "】插入小程序专业版拼团退款队列成功");
                                    }
                                    else
                                    {
                                        WriteLog("ID【" + groupUser.Id + "】插入小程序专业版拼团退款队列失败");
                                    }
                                }
                            }
                            else
                            {
                                WriteLog("更新专业版拼团ID【" + item.Id + "】过期失败");
                            }
                        }
                        else if (item.GroupSize == groupUserList.Count)
                        {
                            item.State = (int)GroupState.团购成功;
                            if (EntGroupSponsorBLL.SingleModel.Update(item, "State"))
                            {
                                WriteLog("更新专业版拼团ID【" + item.Id + "】团购状态成功");
                            }
                        }
                    }
                }

                #endregion

                #region 餐饮版拼团
                
                List<EntGroupSponsor> foodlistPost = EntGroupSponsorBLL.SingleModel.GetGroupList((int)TmpType.小程序餐饮模板);
                bool isSuccess = false;
                if (foodlistPost != null && foodlistPost.Count > 0)
                {
                    foreach (EntGroupSponsor item in foodlistPost)
                    {
                        List<FoodGoodsOrder> groupUserList = FoodGoodsOrderBLL.SingleModel.GetListGroupOrder(item.Id);

                        if (item.GroupSize != groupUserList.Count)
                        {
                            item.State = (int)GroupState.已过期;
                            isSuccess = EntGroupSponsorBLL.SingleModel.Update(item, "State");
                            if (!isSuccess)
                            {
                                WriteLog("更新餐饮版拼团ID【" + item.Id + "】过期失败");
                                continue;
                            }
                            WriteLog("更新餐饮拼团ID【" + item.Id + "】过期成功");
                            if (!groupUserList.Any())
                            {
                                WriteLog("小程序餐饮版拼团服务找不到可以退款的用户"+item.Id);
                                continue;
                            }
                            foreach (FoodGoodsOrder groupUser in groupUserList)
                            {
                                //退款接口 abel
                                if (groupUser.BuyMode == (int)miniAppBuyMode.微信支付)
                                {
                                    isSuccess = FoodGoodsOrderBLL.SingleModel.outOrder(groupUser, groupUser.State);
                                }
                                else if (groupUser.BuyMode == (int)miniAppBuyMode.储值支付)
                                {
                                    SaveMoneySetUser userSaveMoney = SaveMoneySetUserBLL.SingleModel.getModelByUserId(groupUser.UserId) ?? new SaveMoneySetUser();
                                    isSuccess = FoodGoodsOrderBLL.SingleModel.outOrderBySaveMoneyUser(groupUser, userSaveMoney, groupUser.State);
                                }
                                if (!isSuccess)
                                {
                                    WriteLog($"小程序餐饮版拼团服务退款失败，失败订单ID【{groupUser.Id}】");
                                }
                            }
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                WriteLog("自动更新拼团过期服务执行出错" + ex.Message);
            }
            finally
            {
                _outTimeGroupSTimer.Enabled = true;
                _outTimeGroupSTimer.Start();
            }
        }

        /// <summary>
        /// 处理拼团数据未确认收货
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SuccessGroupOrderTimer_Event(object sender, ElapsedEventArgs e)
        {
            _successGroupOrderTimer.Enabled = false;
            _successGroupOrderTimer.Stop();
            try
            {

                string strWhere = $" State={(int)MiniappPayState.已发货} and SendGoodTime<'{DateTime.Now.AddDays(-10)}'";
                List<GroupUser> userlogs = GroupUserBLL.SingleModel.GetList(strWhere, 500, 1);
                if (userlogs != null && userlogs.Count > 0)
                {
                    foreach (GroupUser item in userlogs)
                    {
                        item.State = (int)MiniappPayState.已收货;
                        item.RecieveGoodTime = DateTime.Now;
                        if (GroupUserBLL.SingleModel.Update(item, "State,RecieveGoodTime"))
                        {
                            if (!VipRelationBLL.SingleModel.updatelevel(item.ObtainUserId, "store", item.BuyPrice))
                            {
                                log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常" + item.Id));
                            }

                            WriteLog("自动更新拼团确认收货状态,已更新" + userlogs.Count);
                        }
                        else
                        {

                            WriteLog("自动更新拼团确认收货状态服务执行出错，id=" + item.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog("自动更新拼团确认收货状态服务执行出错" + ex.Message);
            }
            finally
            {
                _successGroupOrderTimer.Enabled = true;
                _successGroupOrderTimer.Start();
            }
        }

        /// <summary>
        /// 处理拼团数据未支付，半小时不支付取消支付
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoPayGroupOrderTimer_Event(object sender, ElapsedEventArgs e)
        {
            _noPayGroupOrderTimer.Enabled = false;
            _noPayGroupOrderTimer.Stop();
            try
            {
                List<GroupUser> groupuserlist = GroupUserBLL.SingleModel.GetList($"State={(int)MiniappPayState.待支付} and CreateDate<'{DateTime.Now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss")}'");
                if (groupuserlist != null && groupuserlist.Count > 0)
                {
                    string groupIds = string.Join(",",groupuserlist.Select(s=>s.GroupId).Distinct());
                    List<Groups> groupsList = GroupsBLL.SingleModel.GetListByIds(groupIds);

                    string storeIds = string.Join(",",groupuserlist.Select(s=>s.StoreId).Distinct());
                    List<Store> storeList = StoreBLL.SingleModel.GetListByIds(storeIds);

                    string aids = string.Join(",",storeList?.Select(s=>s.appId).Distinct());
                    List<XcxAppAccountRelation> xcxAppAccountRelationList = XcxAppAccountRelationBLL.SingleModel.GetListByIds(aids);

                    string tids = string.Join(",",xcxAppAccountRelationList?.Select(s=>s.TId).Distinct());
                    List<XcxTemplate> xcxTemplateList = XcxTemplateBLL.SingleModel.GetListByIds(tids);

                    foreach (GroupUser item in groupuserlist)
                    {
                        TransactionModel tranmodel = new TransactionModel();
                        int guid = item.Id;
                        item.State = (int)MiniappPayState.取消支付;
                        //更新用户订单状态
                        tranmodel.Add($"update GroupUser set State={item.State} where id={item.Id}");

                        Groups group = groupsList?.FirstOrDefault(f=>f.Id == item.GroupId);
                        if (group == null)
                        {
                            WriteLog($"【{guid}】找不到拼团商品");
                            continue;
                        }
                        //退款成功，更新剩余数量
                        tranmodel.Add($"update groups set RemainNum ={(group.RemainNum + item.BuyNum)} where id={group.Id}");

                        if (!GroupUserBLL.SingleModel.ExecuteTransaction(tranmodel.sqlArray, tranmodel.ParameterArray))
                        {
                            WriteLog("xxxxxxxxxxxxxxxxxx拼团取消支付事务执行失败，ID=" + item.Id + "sql:" + string.Join(";", tranmodel.sqlArray));
                            continue;
                        }

                        Store store = storeList?.FirstOrDefault(f=>f.Id == group.StoreId);
                        if (store == null)
                        {
                            WriteLog($"发送模板消息,参数不足,store_null:storeId = {group.StoreId}");
                            continue;
                        }
                        XcxAppAccountRelation xcx = xcxAppAccountRelationList?.FirstOrDefault(f=>f.Id == store.appId);
                        if (xcx == null)
                        {
                            WriteLog($"发送模板消息,参数不足,XcxAppAccountRelation_null:Id = {store.appId}");
                            continue;
                        }
                        XcxTemplate xcxtemp = xcxTemplateList?.FirstOrDefault(f=>f.Id == xcx.TId);
                        if (xcxtemp == null)
                        {
                            WriteLog($"发送模板消息,参数不足,xcxtemp_null:Id = {xcx.TId}");
                            continue;
                        }

                        //发给用户取消通知
                        object groupData = TemplateMsg_Miniapp.GroupGetTemplateMessageData(string.Empty, item, SendTemplateMessageTypeEnum.拼团基础版订单取消通知);
                        TemplateMsg_Miniapp.SendTemplateMessage(item.ObtainUserId, SendTemplateMessageTypeEnum.拼团基础版订单取消通知, xcxtemp.Type, groupData);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog("自动更新拼团确认收货状态服务执行出错" + ex.Message);
            }
            finally
            {
                _noPayGroupOrderTimer.Enabled = true;
                _noPayGroupOrderTimer.Start();
            }
        }

        /// <summary>
        /// 小程序提交代码服务，两个小时运行一次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MiniappSubmitCodeTimer_Event(object sender, ElapsedEventArgs e)
        {
            _miniappsumitcodeTimer.Enabled = false;
            _miniappsumitcodeTimer.Stop();
            try
            {
                //获取3天前还处于待审核或者审核成功的记录
                List<UserXcxTemplate> usertmeplatelist = UserXcxTemplateBLL.SingleModel.GetMiniappSubmitList(-1);
                if (usertmeplatelist != null && usertmeplatelist.Count > 0)
                {
                    WriteLog($"小程序提交代码服务【{usertmeplatelist.Count}】");
                    string appIds = $"'{string.Join("','", usertmeplatelist.Select(s => s.AppId).Distinct())}'";
                    List<XcxAppAccountRelation> xcxrelationList = XcxAppAccountRelationBLL.SingleModel.GetListByAppids(appIds);
                    foreach (UserXcxTemplate item in usertmeplatelist)
                    {
                        XcxAppAccountRelation tempXcxrelationModel = xcxrelationList?.FirstOrDefault(f => f.AppId == item.AppId);
                        if (tempXcxrelationModel == null)
                        {
                            item.UpdateTime = DateTime.Now;
                            item.State = (int)XcxTypeEnum.发布失败;
                            item.Reason = "找不到授权模板数据";
                            UserXcxTemplateBLL.SingleModel.Update(item);
                        }
                        //调用接口提交小程序代码
                        XcxApiBLL.SingleModel._openType = tempXcxrelationModel.ThirdOpenType;
                        string url = XcxApiBLL.SingleModel.ReleaseCode(item.TuserId);
                        string result = HttpHelper.GetData(url);
                        XcxApiRequestJson<object> data = JsonConvert.DeserializeObject<XcxApiRequestJson<object>>(result);
                        if (data.isok == 1 || data.msg == "已发布")
                        {
                            item.UpdateTime = DateTime.Now;
                            item.State = (int)XcxTypeEnum.发布成功;
                            item.Reason = "发布成功";
                            UserXcxTemplateBLL.SingleModel.Update(item, "UpdateTime,State,Reason");
                        }
                        else if (data.msg == "正在审核中")
                        {

                        }
                        else
                        {
                            item.UpdateTime = DateTime.Now;
                            item.State = (int)XcxTypeEnum.发布失败;
                            item.Reason = data.msg + "，请重新发布";
                            UserXcxTemplateBLL.SingleModel.Update(item, "UpdateTime,State,Reason");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog("自动提交小程序代码出错：" + ex.Message);
            }
            finally
            {
                _miniappsumitcodeTimer.Enabled = true;
                _miniappsumitcodeTimer.Start();
            }
        }

        /// <summary>
        /// 自动开启模板消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoOpenTemplateMsgTimer_Event(object sender, ElapsedEventArgs e)
        {
            _autoOpenTemplateMsgTimer.Enabled = false;
            _autoOpenTemplateMsgTimer.Stop();
            try
            {

                
                TemplateMsgBLL.SingleModel.openNewTemplateMsg();
            }
            catch (Exception ex)
            {
                WriteLog("自动开启模板消息：" + ex.Message);
            }
            finally
            {
                _autoOpenTemplateMsgTimer.Enabled = true;
                _autoOpenTemplateMsgTimer.Start();
            }
        }

        /// <summary>
        /// 清理智慧餐厅排队数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoClearDishQueueUpTimer_Event(object sender, ElapsedEventArgs e)
        {
            _autoClearDishQueueUpTimer.Enabled = false;
            _autoClearDishQueueUpTimer.Stop();
            try
            {
                //每天5点-6点间清空排队数据
                if (DateTime.Now.Hour == 5)
                {
                    DishQueueUpBLL.SingleModel.ClearQueueUps();
                }
            }
            catch (Exception ex)
            {
                WriteLog("清空排队数据：" + ex.Message);
            }
            finally
            {
                _autoClearDishQueueUpTimer.Enabled = true;
                _autoClearDishQueueUpTimer.Start();
            }
        }

        /// <summary>
        /// 取消智慧餐厅过期未付款订单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoCancelOrderTimer_Event(object sender, ElapsedEventArgs e)
        {
            _cancelOrderTimer.Enabled = false;
            _cancelOrderTimer.Stop();
            try
            {
                DishOrderBLL.SingleModel.CancelOrder();
            }
            catch (Exception ex)
            {
                WriteLog("取消智慧餐厅过期未付款订单：" + ex.Message);
            }
            finally
            {
                _cancelOrderTimer.Enabled = true;
                _cancelOrderTimer.Start();
            }
        }

        /// <summary>
        /// 物流订单实时查询事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeliveryRealTime_Event(object sender, ElapsedEventArgs e)
        {
            _deliveryRealTimeTraceTimer.Enabled = false;
            _deliveryRealTimeTraceTimer.Stop();
            try
            {
                
                //找到待查询订单
                List<DeliveryFeedback> feedOrder = DeliveryFeedbackBLL.SingleModel.GetWaitForTrace(100);
                if (feedOrder == null || feedOrder.Count == 0)
                {
                    return;
                }
                
                TransactionModel tran = new TransactionModel();
                //批量查询
                foreach (DeliveryFeedback order in feedOrder)
                {
                    //查询实时物流信息
                    DeliveryData feedData = DeliveryFeedbackBLL.SingleModel.GetRealTime(order);
                    if (feedData == null)
                    {
                        WriteLog($"物流实时查询接口异常：no={order.DeliveryNo}，code={order.CompanyCode}");
                        continue;
                    }
                    if (!feedData.Success)
                    {
                        order.Status = (int)DeliveryFeedState.停止;
                        order.Reason = feedData.Reason;
                        tran.Add(DeliveryFeedbackBLL.SingleModel.BuildUpdateSql(order, "Status,Reason"));
                        continue;
                    }
                    //配送状态
                    int deliveryState = -1;
                    if (!int.TryParse(feedData.State, out deliveryState) || !Enum.IsDefined(typeof(DeliveryFeedState), deliveryState))
                    {
                        //无法识别状态
                        order.Status = (int)DeliveryFeedState.停止;
                        order.Reason = feedData.Reason;
                        tran.Add(DeliveryFeedbackBLL.SingleModel.BuildUpdateSql(order, "Status,Reason"));
                        continue;
                    }
                    //配送状态处理
                    switch (deliveryState)
                    {
                        case (int)DeliveryApiFeedState.无轨迹:
                            order.Status = (int)DeliveryFeedState.等待;
                            order.Reason = feedData.Reason;
                            break;
                        case (int)DeliveryApiFeedState.已揽收:
                        case (int)DeliveryApiFeedState.在途中:
                            order.Status = (int)DeliveryFeedState.等待;
                            order.FeedBack = JsonConvert.SerializeObject(feedData.Traces);
                            break;
                        case (int)DeliveryApiFeedState.签收:
                            order.Status = (int)DeliveryFeedState.结束;
                            order.FeedBack = JsonConvert.SerializeObject(feedData.Traces);
                            break;
                        case (int)DeliveryApiFeedState.问题件:
                            order.Status = (int)DeliveryFeedState.进行;
                            order.FeedBack = JsonConvert.SerializeObject(feedData.Traces);
                            break;
                    }
                    tran.Add(DeliveryFeedbackBLL.SingleModel.BuildUpdateSql(order, "Reason,Status,FeedBack"));
                }
                //批量更新查询结果
                bool updateResult = DeliveryFeedbackBLL.SingleModel.ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
                if (!updateResult)
                {
                    //更新事务执行失败
                    WriteLog($"物流信息实时更新错误（记录本地物流订单ID）：{string.Join(",", feedOrder.Select(feed => feed.Id))}");
                }
            }
            catch (Exception ex)
            {
                WriteLogError("物流信息实时查询错误：", ex);
            }
            finally
            {
                _deliveryRealTimeTraceTimer.Enabled = true;
                _deliveryRealTimeTraceTimer.Start();
            }
        }

        /// <summary>
        /// 订单15天后默认好评
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoCommentOrderTimer_Event(object sender, ElapsedEventArgs e)
        {
            _autoCommentOrderTimer.Enabled = false;
            _autoCommentOrderTimer.Stop();
            try
            {
                int timelength = -15;
                GoodsCommentBLL goodsCommentBLL = GoodsCommentBLL.SingleModel;
                //专业版拼团，普通商品默认评价
                goodsCommentBLL.StartEntGoodsCommentServer(timelength);
                //独立小程序
                goodsCommentBLL.StartPlatChildGoodsCommentServer(timelength);
                //团购商品默认评价
                goodsCommentBLL.StartGroupGoodsCommentServer(timelength);
                //砍价商品默认评价
                goodsCommentBLL.StartBargainGoodsCommentServer(timelength);
            }
            catch (Exception ex)
            {
                WriteLog("订单15天后默认好评：" + ex.Message);
            }
            finally
            {
                _autoCommentOrderTimer.Enabled = true;
                _autoCommentOrderTimer.Start();
            }
        }
        /// <summary>
        /// 物流订单订阅推送事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeliverySubscribe_Event(object sender, ElapsedEventArgs e)
        {
            _deliverySubscribeTimer.Enabled = false;
            _deliverySubscribeTimer.Stop();
            try
            {
                
                //找到待订阅订单
                List<DeliveryFeedback> feedOrder = DeliveryFeedbackBLL.SingleModel.GetWaitForSubscribe(100);
                if (feedOrder == null || feedOrder.Count == 0)
                {
                    return;
                }
                TransactionModel tran = new TransactionModel();
                //批量查询
                foreach (DeliveryFeedback order in feedOrder)
                {
                    DeliverySubscribeResult result = DeliveryFeedbackBLL.SingleModel.AddSubscribe(order);
                    if (result == null)
                    {
                        WriteLog($"物流订阅接口异常：no={order.DeliveryNo}，code={order.CompanyCode}");
                        continue;
                    }
                    if (result.Success)
                    {
                        order.Status = (int)DeliveryFeedState.进行;
                    }
                    else
                    {
                        //订阅失败
                        order.Status = (int)DeliveryFeedState.停止;
                        order.Reason = result.Reason;
                    }
                    tran.Add(DeliveryFeedbackBLL.SingleModel.BuildUpdateSql(order, "Status,Result"));
                }
                bool updateResult = DeliveryFeedbackBLL.SingleModel.ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
                if (!updateResult)
                {
                    //更新事务执行失败
                    WriteLog($"物流订单订阅推送更新错误（记录本地物流订单ID）：{string.Join(",", feedOrder.Select(feed => feed.Id))}");
                }
            }
            catch (Exception ex)
            {
                WriteLogError("物流订单订阅推送错误：", ex);
            }
            finally
            {
                _deliverySubscribeTimer.Enabled = true;
                _deliverySubscribeTimer.Start();
            }
        }

        /// <summary>
        /// 物流订单订阅推送同步事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeliverySubscribeSync_Event(object sender, ElapsedEventArgs e)
        {
            _deliverySubscribeSyncTimer.Enabled = false;
            _deliverySubscribeSyncTimer.Stop();
            try
            {
                
                
                //获取订阅同步队列
                List<DeliverySubscribe> subscribeFeeds = DeliverySubscribeBLL.SingleModel.GetWaitForSync(100);
                if (subscribeFeeds == null || subscribeFeeds.Count == 0)
                {
                    return;
                }

                //去除订单重复订阅信息
                var sortGroup = subscribeFeeds.GroupBy(subscribe => new { subscribe.LogisticCode, subscribe.ShipperCode });
                //取最新
                var syncSubscribe = sortGroup.Select(group => group.OrderByDescending(item => item.Id).FirstOrDefault());
                //事务
                TransactionModel tran = new TransactionModel();
                //同步最新订阅推送
                foreach (DeliverySubscribe subcribe in syncSubscribe)
                {
                    //int feedBackId = 0;
                    //if(!int.TryParse(subcribe.CallBack, out feedBackId) || feedBackId == 0)
                    //{
                    //    //无法识别回调物流订单号（CallBack字段）
                    //    subcribe.Sync = (int)DeliverySubscribeSyncState.失败;
                    //    subcribe.Reason = "无法识别回调物流订单号（CallBack字段）";
                    //    tran.Add(subscribeBLL.BuildUpdateSql(subcribe, "Sync,Reason"));
                    //    continue;
                    //}
                    //DeliveryFeedback updateFeed = new DeliveryFeedback { Id = feedBackId };

                    DeliveryFeedback updateFeed = DeliveryFeedbackBLL.SingleModel.GetByCodeAndNote(subcribe.ShipperCode, subcribe.LogisticCode);
                    if (updateFeed == null)
                    {
                        //没有找到需要同步的本地订单
                        subcribe.Sync = (int)DeliverySubscribeSyncState.失败;
                        subcribe.Reason = "找不到本地订单";
                        tran.Add(DeliverySubscribeBLL.SingleModel.BuildUpdateSql(subcribe, "Sync,Reason"));
                        continue;
                    }

                    if (!subcribe.Success)
                    {
                        //订阅推送返回跟踪失败
                        updateFeed.Status = (int)DeliveryFeedState.停止;
                        updateFeed.Reason = subcribe.Reason;
                        subcribe.Sync = (int)DeliverySubscribeSyncState.成功;
                        tran.Add(DeliveryFeedbackBLL.SingleModel.BuildUpdateSql(updateFeed, "Status,Result"));
                        tran.Add(DeliverySubscribeBLL.SingleModel.BuildUpdateSql(subcribe, "Sync"));
                        continue;
                    }

                    if (!Enum.IsDefined(typeof(DeliveryApiFeedState), subcribe.State))
                    {
                        //无法识别配送状态
                        subcribe.Sync = (int)DeliverySubscribeSyncState.失败;
                        subcribe.Reason = "异常配送状态枚举";
                        tran.Add(DeliverySubscribeBLL.SingleModel.BuildUpdateSql(subcribe, "Sync,Reason"));
                        continue;
                    }

                    switch (subcribe.State)
                    {
                        //处理配送状态
                        case (int)DeliveryApiFeedState.无轨迹:
                            updateFeed.Status = (int)DeliveryFeedState.进行;
                            break;
                        case (int)DeliveryApiFeedState.已揽收:
                        case (int)DeliveryApiFeedState.在途中:
                            updateFeed.FeedBack = subcribe.Traces;
                            updateFeed.Status = (int)DeliveryFeedState.进行;
                            break;
                        case (int)DeliveryApiFeedState.签收:
                            updateFeed.FeedBack = subcribe.Traces;
                            updateFeed.Status = (int)DeliveryFeedState.结束;
                            break;
                        case (int)DeliveryApiFeedState.问题件:
                            updateFeed.FeedBack = subcribe.Traces;
                            updateFeed.Status = (int)DeliveryFeedState.进行;
                            break;
                    }
                    tran.Add(DeliveryFeedbackBLL.SingleModel.BuildUpdateSql(updateFeed, "FeedBack,Status"));

                    subcribe.Sync = (int)DeliverySubscribeSyncState.成功;
                    tran.Add(DeliverySubscribeBLL.SingleModel.BuildUpdateSql(subcribe, "Sync"));
                }

                //丢掉旧的订阅推送
                var dumpSubscribe = sortGroup.Where(group => group.Count() > 1).Select(group => group.OrderByDescending(item => item.Id).LastOrDefault());
                foreach (var subscribe in dumpSubscribe)
                {
                    subscribe.Sync = (int)DeliverySubscribeSyncState.失败;
                    subscribe.Reason = "检测到更新的订阅推送，跳过同步";
                    tran.Add(DeliverySubscribeBLL.SingleModel.BuildUpdateSql(subscribe, "Sync,Reason"));
                }

                bool updateResult = DeliveryFeedbackBLL.SingleModel.ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
                if (!updateResult)
                {
                    //更新事务执行失败
                    WriteLog($"物流订阅推送同步错误（记录本地物流订阅ID）：{string.Join(",", subscribeFeeds.Select(feed => feed.Id))}");
                }
            }
            catch (Exception ex)
            {
                WriteLogError("物流订单订阅推送同步错误：", ex);
            }
            finally
            {
                _deliverySubscribeSyncTimer.Enabled = true;
                _deliverySubscribeSyncTimer.Start();
            }
        }

        /// <summary>
        /// Crm系统服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CrmStartTimer_Event(object sender, ElapsedEventArgs e)
        {
            _crmStartTimer.Enabled = false;
            _crmStartTimer.Stop();
            try
            {
                CrmApiDataBLL.SingleModel.CrmService();
            }
            catch (Exception ex)
            {
                WriteLogError("crm系统服务错误：", ex);
            }
            finally
            {
                _crmStartTimer.Enabled = true;
                _crmStartTimer.Start();
            }
        }

        /// <summary>
        /// 代理推广分销服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AgentDistributionStartTimer_Event(object sender, ElapsedEventArgs e)
        {
            _crmStartTimer.Enabled = false;
            _crmStartTimer.Stop();
            try
            {
                AgentDistributionRelationBLL agentDistributionRelationBLL = new AgentDistributionRelationBLL();
                agentDistributionRelationBLL.StartAgentServiceCommand();
                agentDistributionRelationBLL.StartCustomerServiceCommand();
            }
            catch (Exception ex)
            {
                WriteLogError("代理推广分销服务错误：", ex);
            }
            finally
            {
                _crmStartTimer.Enabled = true;
                _crmStartTimer.Start();
            }
        }

        /// <summary>
        /// 小程序统计流量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StatisticalFlowTimer_Event(object sender, ElapsedEventArgs e)
        {
            _statisticalFlowTimer.Enabled = false;
            _statisticalFlowTimer.Stop();
            try
            {
                PlatStatisticalFlowConfigBLL.SingleModel.RunService();
            }
            catch (Exception ex)
            {
                WriteLogError("小程序统计流量服务错误：", ex);
            }
            finally
            {
                _statisticalFlowTimer.Enabled = true;
                _statisticalFlowTimer.Start();
            }
        }

        /// <summary>
        /// 平台子模板服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlatChildCancelOrderTimer_Event(object sender, ElapsedEventArgs e)
        {
            _platChildTimer.Enabled = false;
            _platChildTimer.Stop();
            try
            {
                PlatChildGoodsOrderBLL.SingleModel.StartCancelServer(-1);
                PlatChildGoodsOrderBLL.SingleModel.StartReceiptGoodsServer(-7);
                PlatChildGoodsOrderBLL.SingleModel.StartOutOrderStateServer();
            }
            catch (Exception ex)
            {
                WriteLogError("独立模板取消订单错误：", ex);
            }
            finally
            {
                _platChildTimer.Enabled = true;
                _platChildTimer.Start();
            }
        }

        /// <summary>
        /// 拼享惠取消超时订单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PinCancelOrderTimer_Event(object sender, ElapsedEventArgs e)
        {
            _pinOrderTimer.Enabled = false;
            _pinOrderTimer.Stop();
            try
            {
                PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
                pinGoodsOrderBLL.CancelTimeoutOrder();

            }
            catch (Exception ex)
            {
                WriteLogError("拼享惠取消订单服务错误：", ex);
            }
            finally
            {
                _pinOrderTimer.Enabled = true;
                _pinOrderTimer.Start();
            }
        }

        /// <summary>
        /// 拼享惠检查拼团成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PinCheckGroupSuccess_Event(object sender, ElapsedEventArgs e)
        {
            _pinGroupSuccessTimer.Enabled = false;
            _pinGroupSuccessTimer.Stop();
            try
            {
                PinGroupBLL.SingleModel.CheckGroupSuccess();

            }
            catch (Exception ex)
            {
                WriteLogError("拼享惠检查拼团成功服务错误：", ex);
            }
            finally
            {
                _pinGroupSuccessTimer.Enabled = true;
                _pinGroupSuccessTimer.Start();
            }
        }

        /// <summary>
        /// 拼享惠检查拼团失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PinCheckGroupTimeout_Event(object sender, ElapsedEventArgs e)
        {
            _pinGroupTimeoutTimer.Enabled = false;
            _pinGroupTimeoutTimer.Stop();
            try
            {
                PinGroupBLL.SingleModel.CheckGroupTimeout();

            }
            catch (Exception ex)
            {
                WriteLogError("拼享惠检查拼团失败服务错误：", ex);
            }
            finally
            {
                _pinGroupTimeoutTimer.Enabled = true;
                _pinGroupTimeoutTimer.Start();
            }
        }

        /// <summary>
        /// 拼享惠订单自动交易完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PinOrderSuccess_Event(object sender, ElapsedEventArgs e)
        {
            _pinOrderSuccessTimer.Enabled = false;
            _pinOrderSuccessTimer.Stop();
            try
            {
                PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
                pinGoodsOrderBLL.CheckOrderSuccess();

            }
            catch (Exception ex)
            {
                WriteLogError("拼享惠订单自动交易完成服务错误：", ex);
            }
            finally
            {
                _pinOrderSuccessTimer.Enabled = true;
                _pinOrderSuccessTimer.Start();
            }
        }

        /// <summary>
        /// 秒杀活动到期结束服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FlashDealExpireTimer_Event(object sender, ElapsedEventArgs e)
        {
            _flashDealExpireTimer.Enabled = false;
            _flashDealExpireTimer.Stop();
            try
            {
                List<FlashDeal> expireDeal = FlashDealBLL.SingleModel.GetExpireDeal(100);
                expireDeal?.ForEach(deal =>
                {
                    FlashDealBLL.SingleModel.ExpireByServer(deal);
                });
            }
            catch (Exception ex)
            {
                WriteLogError("秒杀活动到期结束报错：", ex);
            }
            finally
            {
                _flashDealExpireTimer.Enabled = true;
                _flashDealExpireTimer.Start();
            }
        }

        /// <summary>
        /// 秒杀活动到期结束服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FlashDealCountdownTimer_Event(object sender, ElapsedEventArgs e)
        {
            _flashDealCountdownTimer.Enabled = false;
            _flashDealCountdownTimer.Stop();
            try
            {
                List<FlashDeal> waitForStart = FlashDealBLL.SingleModel.GetWaitForStart(100);
                if (waitForStart.Count > 0)
                {
                    FlashDealBLL.SingleModel.BeginDeals(waitForStart);
                }
            }
            catch (Exception ex)
            {
                WriteLogError("秒杀活动开始倒计时报错：", ex);
            }
            finally
            {
                _flashDealCountdownTimer.Enabled = true;
                _flashDealCountdownTimer.Start();
            }
        }

        private void SubscribeMsgTimer_Event(object sender, ElapsedEventArgs e)
        {
            _subscribeMsgTimer.Enabled = false;
            _subscribeMsgTimer.Stop();
            try
            {
                List<SubscribeMessage> messages = SubscribeMessageBLL.SingleModel.GetWaitForSend(100);
                TransactionModel updateMsgState = new TransactionModel();
                string errorMsg = string.Empty;
                messages?.ForEach(msg =>
                {
                    if (SubscribeMessageBLL.SingleModel.SendMessage(msg, out errorMsg))
                    {
                        msg.State = (int)SubscribeMsgState.发送成功;
                        msg.ErrorMsg = $"发送微信接口成功:{errorMsg}";
                    }
                    else
                    {
                        msg.ErrorMsg = errorMsg;
                        msg.State = (int)SubscribeMsgState.发送失败;
                    }
                    updateMsgState.Add(SubscribeMessageBLL.SingleModel.BuildUpdateSql(msg, "State,ErrorMsg"));
                });
                bool result = SubscribeMessageBLL.SingleModel.ExecuteTransactionDataCorect(updateMsgState.sqlArray, updateMsgState.ParameterArray);
                if (!result)
                {
                    WriteLog($"订阅模板消息>发送服务>事务异常（发送状态更新）：{string.Join(";", updateMsgState.sqlArray)}");
                }
            }
            catch (Exception ex)
            {
                WriteLogError("订阅模板消息发送服务报错：", ex);
            }
            finally
            {
                _subscribeMsgTimer.Enabled = true;
                _subscribeMsgTimer.Start();
            }
        }

        /// <summary>
        /// 企业智推版服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QiyeTimer_Event(object sender, ElapsedEventArgs e)
        {
            _qiyeTimer.Enabled = false;
            _qiyeTimer.Stop();
            try
            {
                QiyeGoodsOrderBLL.SingleModel.StartCancelServer(-1);
                QiyeGoodsOrderBLL.SingleModel.StartReceiptGoodsServer(-7);
                QiyeGoodsOrderBLL.SingleModel.StartOutOrderStateServer();
            }
            catch (Exception ex)
            {
                WriteLogError("企业智推版服务错误：", ex);
            }
            finally
            {
                _qiyeTimer.Enabled = true;
                _qiyeTimer.Start();
            }
        }

        private void UserTrackTimer_Event(object sender, ElapsedEventArgs e)
        {
            _userTrackTimer.Enabled = false;
            _userTrackTimer.Stop();
            try
            {
                List<UserTrack> record = UserTrackBLL.SingleModel.GetWaitProcess(999);
                bool result = UserTrackBLL.SingleModel.ProcessRecord(record);
                if (!result)
                {
                    WriteLog("用户跟踪记录统计服务出错，执行失败");
                }
            }
            catch (Exception ex)
            {
                WriteLogError("用户跟踪记录统计服务出错：", ex);
            }
            finally
            {
                _userTrackTimer.Enabled = true;
                _userTrackTimer.Start();
            }
        }

        private void AddRecordTimer_Event(object sender, ElapsedEventArgs e)
        {
            _ImAddRecordTimer.Enabled = false;
            _ImAddRecordTimer.Stop();
            try
            {
                ImMessageBLL.SingleModel.AddMessageRecord();
            }
            catch (Exception ex)
            {
                WriteLogError("私信同步消息服务出错：", ex);
            }
            finally
            {
                _ImAddRecordTimer.Enabled = true;
                _ImAddRecordTimer.Start();
            }
        }


        /// <summary>
        /// 专业版申请取消订单服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EntApplyCancelOrderTimer_Event(object sender, ElapsedEventArgs e)
        {
            _entApplyCancelOrderTimer.Enabled = false;
            _entApplyCancelOrderTimer.Stop();
            try
            {
                EntGoodsOrderBLL.SingleModel.StartApplyCancelOrderServer(-120);
            }
            catch (Exception ex)
            {
                WriteLogError("专业版申请取消订单服务错误：", ex);
            }
            finally
            {
                _entApplyCancelOrderTimer.Enabled = true;
                _entApplyCancelOrderTimer.Start();
            }
        }

        private void WriteLog(string msg)
        {
            LogHelper.WriteInfo(GetType(), msg);
        }
        private void WriteLogError(string msg, Exception ex)
        {
            LogHelper.WriteError(GetType(), ex);
        }
        protected override void OnStop()
        {
            LogHelper.WriteInfo(GetType(), "服务关闭成功！");
        }
    }
}
