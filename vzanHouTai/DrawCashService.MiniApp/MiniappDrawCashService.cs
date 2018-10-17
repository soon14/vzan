
using BLL.MiniApp.Ent;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using BLL.MiniApp;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using Entity.MiniApp.Conf;
using BLL.MiniApp.Conf;
using System.Configuration;
using System.IO;

namespace DrawCashService.MiniApp
{
    public partial class MiniappDrawCashService : ServiceBase
    {
        private System.Timers.Timer DistributionDrawCashTimer;
        private System.Timers.Timer InstallCertTimer;
        
        
        private static string _installPath = ConfigurationManager.AppSettings["InstallPath"];
        private static string _folderUrl = ConfigurationManager.AppSettings["CertZipPath"];


        public MiniappDrawCashService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 线程停顿的时间间隔(单位:毫秒)
        /// </summary>
        public int DrawCashInterval
        {
            get
            {
                int time = 0;
                string intervaltime = System.Configuration.ConfigurationManager.AppSettings["DrawCashInterval"];
                if (!string.IsNullOrEmpty(intervaltime))
                {
                    int.TryParse(intervaltime, out time);
                }
                if (time == 0)
                {
                    time = 10;
                }
                int _ThreadPauseInterval = time * 1000;
                return _ThreadPauseInterval;
            }
        }

        protected override void OnStart(string[] args)
        {
            DistributionDrawCashTimer = new System.Timers.Timer(DrawCashInterval);
            DistributionDrawCashTimer.Elapsed += new ElapsedEventHandler(DistributionDrawCashTime_EventV2);
            DistributionDrawCashTimer.Enabled = true;

            InstallCertTimer = new System.Timers.Timer(10*1000);
            InstallCertTimer.Elapsed += new ElapsedEventHandler(InstallCertTimer_EventV);
            InstallCertTimer.Enabled = true;

            writeLog("小程序提现服务开始了");
        }

        protected override void OnStop()
        {
            log4net.LogHelper.WriteError(this.GetType(), new Exception("小程序分销提现服务关闭"));
        }

        #region 提现服务
        private void DistributionDrawCashTime_EventV2(object sender, ElapsedEventArgs e)
        {
            DistributionDrawCashTimer.Enabled = false;
            DistributionDrawCashTimer.Stop();
            writeLog("小程序提现服务进来了");
            //1.从提现申请列表里找出100条 分销通过审核需要提现的数据
            try
            {
                List<DrawCashApply> listDrawCashApply = DrawCashApplyBLL.SingleModel.GetList($"aid>0 and state=1 and drawState=1  and drawcashway=0 ", 100, 1);//and applytype in ({(int)DrawCashApplyType.分销收益提现},{(int)DrawCashApplyType.平台店铺提现},{(int)DrawCashApplyType.普通提现},{(int)DrawCashApplyType.拼享惠用户返现})
                if (listDrawCashApply != null && listDrawCashApply.Count > 0)
                {
                    int code = 0;
                    string resultmsg = "";
                    C_UserInfo c_UserInfo = new C_UserInfo();
                    foreach (DrawCashApply item in listDrawCashApply)
                    {
                        code = 0;
                        resultmsg = "";
                        GetDrawCashResult(item, ref code, ref resultmsg, out c_UserInfo);
                        CommandDrawCashResult(item, code, resultmsg, c_UserInfo);
                    }
                    writeLog($"本次提现队列执行完毕{listDrawCashApply.Count}条");
                }
                else
                {
                    writeLog($"本次没有找到需要提现的数据休息10分钟");
                    Thread.Sleep(600000);
                }
            }
            catch (Exception ex)
            {
                writeLog("小程序提现服务异常" + ex.Message);
            }
            finally
            {
                DistributionDrawCashTimer.Enabled = true;
                DistributionDrawCashTimer.Start();
            }
        }

        /// <summary>
        /// 获取提现结果
        /// </summary>
        /// <param name="item"></param>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        public void GetDrawCashResult(DrawCashApply item, ref int code, ref string msg, out C_UserInfo userinfo)
        {
            item.drawState = (int)DrawCashState.提现失败;
            userinfo = C_UserInfoBLL.SingleModel.GetModel(item.userId);
            if (userinfo != null)
            {
                PayCenterSetting setting = PayCenterSettingBLL.SingleModel.GetPayCenterSetting(userinfo.appId);
                if (setting == null)
                {
                    msg = "提现失败更新提现申请记录失败PayCenterSetting为NULL";
                }
                else
                {
                    WxPayData data = new WxPayData();
                    data.SetValue("openid", userinfo.OpenId);//openid
                    data.SetValue("amount", item.cashMoney);//取款金额
                    data.SetValue("re_user_name", ReplaceSpecialChar(userinfo.NickName, '?'));//收款用户姓名
                    data.SetValue("desc", string.Format("{0},小程序{1}提现{2}元", ReplaceSpecialChar(userinfo.NickName, '?'),Enum.GetName(typeof(DrawCashApplyType),item.applyType), item.cashMoneyStr));
                    data.SetValue("partner_trade_no", item.partner_trade_no);//订单号
                    data.SetValue("spbill_create_ip", ConfigurationManager.AppSettings["IP"]);//订单号

                    WxPayData result = WxPayApi.CompanyPay(data, setting);
                    if (result != null)
                    {
                        try
                        {
                            string resultStr = result.ToJson();
                            //企业付款（客户提现），接收返回数据
                            //----------------------
                            //判断执行提现结果
                            //----------------------
                            int i = DrawResult(result, item.cashMoney, setting);
                            if (i == 1)
                            {
                                msg = "提现成功";
                                code = 1;
                                item.drawState = (int)DrawCashState.提现成功;
                            }
                            else if (i == -2)
                            {
                                code = -1;
                                //表示微信那边返回错误码为“SYSTEMERROR”时，一定要使用原单号重试，否则可能造成重复支付等资金风险。
                                //该提现记录不能算失败也不能算成功,维持原状等待下次提现队列
                                msg = "返回错误码为“SYSTEMERROR”等待下次提现队列提现返回结果" + resultStr;
                            }
                            else
                            {
                                code = 0;
                                msg = result.GetValue("err_code_des").ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            msg = "提现失败发生异常" + ex.Message;
                        }
                    }
                    else
                    {
                        msg = "提现失败(证书路径不存在)";
                    }
                }
            }
            else
            {
                msg = "提现用户不存在";
            }

            item.DrawTime = DateTime.Now;
            DrawCashApplyBLL.SingleModel.Update(item, "drawState,DrawTime");
            writeLog(msg);
        }

        /// <summary>
        /// 处理提现结果
        /// </summary>
        /// <param name="item"></param>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        public void CommandDrawCashResult(DrawCashApply item, int code, string resultmsg, C_UserInfo userinfo)
        {
            //表示微信那边返回错误码为“SYSTEMERROR”时，一定要使用原单号重试，否则可能造成重复支付等资金风险。
            //该提现记录不能算失败也不能算成功,维持原状等待下次提现队列
            if (code == -1)
            {
                writeLog(resultmsg);
                return;
            }
            DrawCashApplyBLL drawCashApplyBLL = DrawCashApplyBLL.SingleModel;
            string msg = "";
            switch (item.applyType)
            {
                case (int)DrawCashApplyType.分销收益提现:
                    msg = drawCashApplyBLL.UpdateDistributionDrawCashApply(code, item, resultmsg, userinfo);
                    break;
                case (int)DrawCashApplyType.平台店铺提现:
                    msg = drawCashApplyBLL.UpdatePlayDrawCashResult(code, item, resultmsg);
                    break;
                case (int)DrawCashApplyType.普通提现: break;
                case (int)DrawCashApplyType.拼享惠平台交易:
                case (int)DrawCashApplyType.拼享惠扫码收益:
                    msg = drawCashApplyBLL.UpdatePinStoreDrawCashResult(code, item, resultmsg);
                    break;
                case (int)DrawCashApplyType.拼享惠代理收益:
                    msg = drawCashApplyBLL.UpdatePinAgentDrawCashResult(code, item, resultmsg);
                    break;
                case (int)DrawCashApplyType.拼享惠用户返现:
                    msg = drawCashApplyBLL.UpdatePxhUserDrawCashResult(code, item, resultmsg);
                    break;
            }

            if (msg.Length > 0)
            {
                writeLog(msg);
            }
        }

        /// <summary>
        /// 判断提现结果 只有返回为1才表示提现成功   
        /// </summary>
        /// <param name="result"></param>
        /// <param name="amout"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public int DrawResult(WxPayData result, int amout, PayCenterSetting setting)
        {
            if (result == null || result.GetValue("return_code") == null)//微信服务器有返回值
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "提现出错，错误信息：" + result.ToJson());
                return -1;
            }
            //业务结果:result_code（SUCCESS/FAIL，为SUCCESS才是付款成功）
            if (result.GetValue("result_code").ToString() != "SUCCESS")//提现失败
            {
                if (result.GetValue("err_code_des").ToString().Contains("原单号"))
                {
                    //表示微信那边返回系统错误 则该提现记录不能算失败也不能算成功,维持原状等待下次提现队列
                    return -2;
                }
                //如果是账户余额不足，推送消息给管理员
                if (result.GetValue("err_code").ToString() == "NOTENOUGH")
                {

                    return -3;
                }
                log4net.LogHelper.WriteInfo(this.GetType(), "提现出错，错误信息2：" + result.ToJson());
                return 0;
            }
            else//成功，修改记录
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "提现成功：" + result.ToJson());
                return 1;
            }
        }
        #endregion

        #region 自动安装证书服务
        private void InstallCertTimer_EventV(object sender, ElapsedEventArgs e)
        {
            InstallCertTimer.Enabled = false;
            InstallCertTimer.Stop();
            CertInstall erroItem = null;
            try
            {
                List<CertInstall> list = CertInstallBLL.SingleModel.GetListByNoInstall();
                if(list!=null && list.Count>0)
                {
                    writeLog("小程序证书安装服务进来了");
                    foreach (CertInstall item in list)
                    {
                        erroItem = item;
                        if (ZipHelper.UnZip(_folderUrl + item.Name, _installPath + item.Mc_Id))
                        {
                            //添加个人证书
                            X509Certificate2 certificate = new X509Certificate2(_installPath + item.Mc_Id + "\\apiclient_cert.p12", item.Password);
                            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                            store.Open(OpenFlags.ReadWrite);
                            store.Remove(certificate);   //可省略
                            store.Add(certificate);
                            store.Close();

                            item.State = -1;
                            item.UpdateTime = DateTime.Now;
                            erroItem.ErrorMsg = "";

                            //string[] files = Directory.GetFiles(_installPath + item.Mc_Id, "*.p12");
                            //if(files==null || files.Length<=0)
                            //{
                            //    item.State = -2;
                            //    item.UpdateTime = DateTime.Now;
                            //    erroItem.ErrorMsg = $"没有找到p12类型的文件";
                            //}
                            //else
                            //{
                            //    //添加个人证书
                            //    X509Certificate2 certificate = new X509Certificate2(files[0], item.Password);
                            //    X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                            //    store.Open(OpenFlags.ReadWrite);
                            //    store.Remove(certificate);   //可省略
                            //    store.Add(certificate);
                            //    store.Close();

                            //    item.State = -1;
                            //    item.UpdateTime = DateTime.Now;
                            //    erroItem.ErrorMsg = "";
                            //}
                        }
                        else
                        {
                            item.State = -2;
                            item.UpdateTime = DateTime.Now;
                            erroItem.ErrorMsg = $"解压失败【{item.Name}】";
                        }

                        CertInstallBLL.SingleModel.Update(item, "State,UpdateTime,ErrorMsg");
                    }
                }
            }
            catch (Exception ex)
            {
                if(erroItem!=null)
                {
                    erroItem.State = -2;
                    erroItem.UpdateTime = DateTime.Now;
                    erroItem.ErrorMsg = "小程序证书安装服务异常" + ex.Message;
                    CertInstallBLL.SingleModel.Update(erroItem, "State,UpdateTime,ErrorMsg");
                }
                log4net.LogHelper.WriteError(this.GetType(),ex);
            }
            finally
            {
                InstallCertTimer.Enabled = true;
                InstallCertTimer.Start();
            }
        }

        #endregion

        /// <summary>
        /// 非汉字和ascii表内的字符用指定字符表示表示
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="p">指定字符</param>
        /// <returns></returns>
        public static string ReplaceSpecialChar(string str, char p)
        {
            if (string.IsNullOrEmpty(str)) return "";
            char[] cs = str.ToCharArray();
            for (int i = 0; i < cs.Length; i++)
            {
                if (!((0x4E00 <= cs[i] && cs[i] <= 0x9FA5) || cs[i] <= 128 || (0xFF00 <= cs[i] && cs[i] <= 0xFFEF) || (0xFE10 <= cs[i] && cs[i] <= 0xFE1F)))
                {
                    cs[i] = p;
                }
            }
            return new String(cs);
        }

        private void writeLog(string msg)
        {
            log4net.LogHelper.WriteInfo(this.GetType(), msg);
        }
    }
}
