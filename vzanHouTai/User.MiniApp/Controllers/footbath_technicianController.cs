using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.User;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utility.IO;

namespace User.MiniApp.Controllers
{
    public partial class footbathController : configController
    {
        // GET: footbath_technician：足浴版小程序-技师管理

        #region 技师管理
        /// <summary>
        /// 技师管理视图
        /// </summary>
        /// <returns></returns>
        public ActionResult TechnicianList()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            ViewBag.appId = appId;
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
            if (storeModel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "500" });
            }
            List<EntGoods> serviceList = EntGoodsBLL.SingleModel.GetServicList(appId);


            //扫码绑定代码
            string sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
            Session["qrcodekey"] = sessonid;
            if (null == RedisUtil.Get<LoginQrCode>("SessionID:" + sessonid))
            {
                LoginQrCode wxkey = new LoginQrCode();
                wxkey.SessionId = sessonid;
                wxkey.IsLogin = false;
                RedisUtil.Set<LoginQrCode>("SessionID:" + sessonid, wxkey);
            }
            return View(serviceList);
        }


        /// <summary>
        /// 获取技师列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTechnicianList()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }

            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string jobNumber = Context.GetRequest("jobNumber", string.Empty);
            string name = Context.GetRequest("name", string.Empty);
            int sex = Context.GetRequestInt("sex", -1);
            string phone = Context.GetRequest("phone", string.Empty);
            int itemId = Context.GetRequestInt("itemId", 0);
            int isShow = Context.GetRequestInt("isShow", -1);
            int state = Context.GetRequestInt("state", -1);

            int recordCount = 0;
            List<TechnicianInfo> technicianList = TechnicianInfoBLL.SingleModel.GetTechnicianList(storeModel.Id, jobNumber, name, sex, phone, itemId, isShow, state, pageIndex, pageSize, out recordCount);
            if (technicianList != null && technicianList.Count > 0)
            {
                foreach (TechnicianInfo info in technicianList)
                {
                    info.switchModel = JsonConvert.DeserializeObject<TechnicianSwitch>(info.switchConfig);
                    info.wxname = UserBaseInfoBLL.SingleModel.GetWxname(info.unionId);
                    info.showBirthday = info.birthday.ToString("yyyy-MM-dd");
                    if (!string.IsNullOrEmpty(info.itemId))
                    {
                        info.serviceList = EntGoodsBLL.SingleModel.GetServerListByIds(info.appId, info.itemId);
                    }

                }
            }
            return Json(new { isok = true, recordCount = recordCount, list = technicianList });
        }
        /// <summary>
        /// 保存技师添加编辑信息
        /// </summary>
        /// <param name="postdata"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult SaveTechnicianInfo(TechnicianInfo postdata)
        {
            if (postdata.appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(postdata.appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={postdata.appId}");
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙store_null" });
            }

            if (string.IsNullOrEmpty(postdata.jobNumber))
            {
                return Json(new { isok = false, msg = "请输入工号" });
            }
            if (postdata.jobNumber.Length > 8)
            {
                return Json(new { isok = false, msg = "工号不能超过5字" });
            }
            if (string.IsNullOrEmpty(postdata.name))
            {
                return Json(new { isok = false, msg = "请输入姓名" });
            }
            if (postdata.name.Length > 8)
            {
                return Json(new { isok = false, msg = "姓名不能超过8字" });
            }
            if (string.IsNullOrEmpty(postdata.headImg))
            {
                return Json(new { isok = false, msg = "请上传头像" });
            }
            if (postdata.desc != null && postdata.desc.Length > 20)
            {
                return Json(new { isok = false, msg = "简介不能超过20字" });
            }
            if (postdata.baseCount < 0 || postdata.baseCount > 999)
            {
                return Json(new { isok = false, msg = "订单数范围：0~999" });
            }
            if (postdata.itemId == null || string.IsNullOrEmpty(postdata.itemId))
            {
                return Json(new { isok = false, msg = "请选择可服务项目" });
            }
            if (!string.IsNullOrEmpty(postdata.showBirthday))
            {
                try
                {
                    postdata.birthday = Convert.ToDateTime(postdata.showBirthday);
                }
                catch
                {
                    return Json(new { isok = false, msg = "非法生日日期" });
                }
            }

            List<MySqlParameter> parameters = new List<MySqlParameter>();

            if (string.IsNullOrWhiteSpace(postdata.TelPhone) || postdata.TelPhone.Length != 11)
            {
                return Json(new { isok = false, msg = "请输入正确的手机号码,以便用于关联登录技师端" });
            }
            //手机只能被输入一次
            if (!string.IsNullOrEmpty(postdata.TelPhone))
            {
                if (TechnicianInfoBLL.SingleModel.ValidPhone(postdata))
                {
                    return Json(new { isok = false, msg = "该手机已被使用.", wxError = true });
                }
            }

            postdata.switchConfig = JsonConvert.SerializeObject(postdata.switchModel);
            bool isok = false;
            if (postdata.id > 0)//保存编辑
            {
                TechnicianInfo model = TechnicianInfoBLL.SingleModel.GetModelById(postdata.id);
                if (model == null)
                {
                    return Json(new { isok = false, msg = "系统繁忙model_null" });
                }
                if (TechnicianInfoBLL.SingleModel.ValidJobNumber(postdata))
                {
                    return Json(new { isok = false, msg = "该工号已存在" });
                }
                //微信号只能被绑定一次
                if (!string.IsNullOrEmpty(postdata.unionId))
                {

                    if (TechnicianInfoBLL.SingleModel.ValidWeChat(postdata))
                    {
                        return Json(new { isok = false, msg = "该微信号已被绑定", wxError = true });
                    }
                }
                model.name = postdata.name;
                model.jobNumber = postdata.jobNumber;
                model.photo = postdata.photo;
                model.itemId = postdata.itemId;
                model.headImg = postdata.headImg;
                model.switchConfig = postdata.switchConfig;
                model.sex = postdata.sex;
                model.desc = postdata.desc;
                model.birthday = postdata.birthday;
                model.baseCount = postdata.baseCount;
                model.updateDate = postdata.updateDate;
                model.TelPhone = postdata.TelPhone;
                model.unionId = postdata.unionId;
                isok = TechnicianInfoBLL.SingleModel.Update(model, "name,jobnumber,photo,itemid,headimg,switchconfig,sex,desc,birthday,basecount,unionid,updatedate,TelPhone");
            }
            else//新增
            {
                postdata.storeId = storeModel.Id;
                if (TechnicianInfoBLL.SingleModel.ValidJobNumber(postdata))
                {
                    return Json(new { isok = false, msg = "该工号已存在" });
                }
                if (TechnicianInfoBLL.SingleModel.ValidWeChat(postdata))
                {
                    return Json(new { isok = false, msg = "该微信号被绑定", wxError = true });
                }
                postdata.storeId = storeModel.Id;
                postdata.state = 0;
                postdata.id = Convert.ToInt32(TechnicianInfoBLL.SingleModel.Add(postdata));
                isok = postdata.id > 0;
                if (isok)
                {
                    //初始化技师值班表
                    Rota rota = new Rota();
                    rota.aid = postdata.appId;
                    rota.storeId = postdata.storeId;
                    rota.tid = postdata.id;
                    rota.dayType = 0;//单周值班表
                    RotaBLL.SingleModel.Add(rota);
                    rota.dayType = 1;//双周值班表
                    RotaBLL.SingleModel.Add(rota);
                }
            }

            string msg = isok ? "保存成功" : "保存失败";
            return Json(new { isok = true, msg = msg });
        }
        /// <summary>
        /// 删除技师或修改技师状态(删除技师同时要删除技师值班表数据)
        /// </summary>
        /// <returns></returns>
        public ActionResult DelOrChangeState()
        {
            try
            {
                int appId = Context.GetRequestInt("appId", 0);
                if (appId <= 0)
                {
                    return Json(new { isok = false, msg = "系统繁忙appid_null" }, JsonRequestBehavior.AllowGet);
                }
                if (dzaccount == null)
                {
                    return Json(new { isok = false, msg = "系统繁忙auth_null" });
                }
                XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
                if (appAcountRelation == null)
                {
                    return Json(new { isok = false, msg = "系统繁忙relation_null" });
                }
                FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
                if (storeModel == null)
                {
                    return Json(new { isok = false, msg = "系统繁忙store_null" });
                }
                int id = Context.GetRequestInt("id", 0);
                if (id <= 0)
                {
                    return Json(new { isok = false, msg = "系统繁忙id_null" });
                }
                int state = Context.GetRequestInt("state", -2);
                if (state < -1)
                {
                    return Json(new { isok = false, msg = "系统繁忙state_null" });
                }
                TechnicianInfo model = TechnicianInfoBLL.SingleModel.GetModelById(id);
                if (model == null)
                {
                    return Json(new { isok = false, msg = "系统繁忙model_null" });
                }
                model.updateDate = DateTime.Now;
                model.state = state;
                string stateName = Enum.GetName(typeof(TechnicianState), state);
                bool isok = TechnicianInfoBLL.SingleModel.Update(model, "state,updatedate");
                if (isok && model.state == -1)
                {
                    RotaBLL.SingleModel.deleteRotaByTid(model.id);
                }
                string msg = isok ? "操作成功" : "操作失败";
                return Json(new { isok = isok, msg = msg, stateName = stateName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// 技师编辑微信绑定
        /// </summary>
        /// <param name="wxkey"></param>
        /// <returns></returns>
        public ActionResult WXBind(string wxkey)
        {
            LoginQrCode lcode = RedisUtil.Get<LoginQrCode>("SessionID:" + wxkey);
            if (string.IsNullOrEmpty(wxkey))
            {
                return Json(new { isok = false });
            }
            if (lcode == null)
            {
                return Json(new { isok = false });
            }
            if (!lcode.IsLogin)
            {
                return Json(new { isok = false });
            }

            if (lcode.WxUser != null)
            {
                Account accountmodel = null;
                if (!string.IsNullOrEmpty(lcode.WxUser.openid))
                {
                    
                    UserBaseInfo userInfo = UserBaseInfoBLL.SingleModel.GetModelByOpenId(lcode.WxUser.openid, lcode.WxUser.serverid);
                    if (userInfo == null)
                    {
                        userInfo = new UserBaseInfo();
                        userInfo.openid = lcode.WxUser.openid;
                        userInfo.nickname = lcode.WxUser.nickname;
                        userInfo.headimgurl = lcode.WxUser.headimgurl;
                        userInfo.sex = lcode.WxUser.sex;
                        userInfo.country = lcode.WxUser.country;
                        userInfo.city = lcode.WxUser.city;
                        userInfo.province = lcode.WxUser.province;
                        userInfo.unionid = lcode.WxUser.unionid;
                        UserBaseInfoBLL.SingleModel.Add(userInfo);
                    }
                    accountmodel = AccountBLL.SingleModel.GetAccountByWeixinUser(lcode.WxUser);
                    if (accountmodel == null)
                    {
                        return Json(new { isok = false });
                    }
                    
                    Member member = MemberBLL.SingleModel.GetMemberByAccountId(accountmodel.Id.ToString());
                    member.LastModified = DateTime.Now;//记录登录时间
                    MemberBLL.SingleModel.Update(member);
                    RedisUtil.Remove("SessionID:" + wxkey);
                    //重置二维码
                    string sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
                    if (null == RedisUtil.Get<LoginQrCode>("SessionID:" + sessonid))
                    {
                        LoginQrCode newWxkey = new LoginQrCode();
                        newWxkey.SessionId = sessonid;
                        newWxkey.IsLogin = false;
                        RedisUtil.Set<LoginQrCode>("SessionID:" + sessonid, newWxkey);
                    }
                    return Json(new { isok = true, unionId = userInfo.unionid, key = sessonid });
                }
            }
            return Json(new { isok = false });
        }

        /// <summary>
        /// 排班表（视图）
        /// </summary>
        /// <returns></returns>
        public ActionResult Rota()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            ViewBag.appId = appId;
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "500" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
            if (storeModel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "500" });
            }
            storeModel.switchModel = JsonConvert.DeserializeObject<SwitchModel>(storeModel.SwitchConfig);
            ViewBag.week = ServiceTimeBLL.SingleModel.IsSingleWeek(DateTime.Now);
            return View(storeModel.switchModel);
        }
        /// <summary>
        /// 保存早中晚班值班时间
        /// </summary>
        /// <param name="switchModel"></param>
        /// <returns></returns>
        public ActionResult saveWorkTime(SwitchModel switchModel)
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙store_null" });
            }
            storeModel.switchModel = JsonConvert.DeserializeObject<SwitchModel>(storeModel.SwitchConfig);
            storeModel.switchModel.morningShift = switchModel.morningShift;
            storeModel.switchModel.noonShift = switchModel.noonShift;
            storeModel.switchModel.eveningShift = switchModel.eveningShift;

            storeModel.SwitchConfig = JsonConvert.SerializeObject(storeModel.switchModel);
            storeModel.UpdateDate = DateTime.Now;
            bool isok = FootBathBLL.SingleModel.Update(storeModel, "SwitchConfig,UpdateDate");
            string msg = isok ? "保存成功" : "保存失败";
            return Json(new { isok = isok, msg = msg });
        }
        /// <summary>
        /// 获取值班列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetRotaList()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }

            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string jobNumber = Context.GetRequest("jobNumber", string.Empty);
            int daytype = Context.GetRequestInt("daytype", -1);
            int workType = Context.GetRequestInt("workType", -1);

            List<Rota> rotaList = new List<Rota>();
            int recordCount = 0;

            string tids = string.Empty;
            if (!string.IsNullOrEmpty(jobNumber) || workType > -1)
            {
                List<TechnicianInfo> technicianInfoList = TechnicianInfoBLL.SingleModel.GetTechnicianListByJobNumeberOrWorkType(storeModel.Id, jobNumber, workType);
                if (technicianInfoList == null || technicianInfoList.Count <= 0)
                {
                    return Json(new { isok = true, recordCount = recordCount, list = rotaList });
                }
                tids = string.Join(",", technicianInfoList.Select(t => t.id.ToString()).ToArray());

            }

            rotaList = RotaBLL.SingleModel.GetRotaList(storeModel.appId, tids, daytype, pageSize, pageIndex, out recordCount);
            if (rotaList != null && rotaList.Count > 0)
            {
                rotaList.ForEach(r =>
                {
                    TechnicianInfo info = TechnicianInfoBLL.SingleModel.GetModelById(r.tid);
                    if (info != null)
                    {
                        r.tname = info.jobNumber;
                    }
                    r.mondayState = RotaBLL.SingleModel.GetDayState(r.monday);
                    r.tuesdayState = RotaBLL.SingleModel.GetDayState(r.tuesday);
                    r.wensdayState = RotaBLL.SingleModel.GetDayState(r.wensday);
                    r.thursdayState = RotaBLL.SingleModel.GetDayState(r.thursday);
                    r.fridayState = RotaBLL.SingleModel.GetDayState(r.friday);
                    r.saturdayState = RotaBLL.SingleModel.GetDayState(r.saturday);
                    r.sundayState = RotaBLL.SingleModel.GetDayState(r.sunday);
                });
            }
            return Json(new { isok = true, recordCount = recordCount, list = rotaList });
        }

        /// <summary>
        /// 保存值班信息
        /// </summary>
        /// <param name="rotaInfo"></param>
        /// <returns></returns>
        public ActionResult SaveRotaInfo(Rota rotaInfo)
        {
            if (rotaInfo.aid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(rotaInfo.aid, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={rotaInfo.aid}");
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }
            if (rotaInfo.Id <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙rotaId_null" });
            }
            Rota model = RotaBLL.SingleModel.GetModelById(rotaInfo.aid, rotaInfo.Id);
            if (model == null)
            {
                return Json(new { isok = false, msg = "系统繁忙rota_null" });
            }
            model.monday = RotaBLL.SingleModel.GetDayStr(rotaInfo.mondayState);
            model.tuesday = RotaBLL.SingleModel.GetDayStr(rotaInfo.tuesdayState);
            model.wensday = RotaBLL.SingleModel.GetDayStr(rotaInfo.wensdayState);
            model.thursday = RotaBLL.SingleModel.GetDayStr(rotaInfo.thursdayState);
            model.friday = RotaBLL.SingleModel.GetDayStr(rotaInfo.fridayState);
            model.saturday = RotaBLL.SingleModel.GetDayStr(rotaInfo.saturdayState);
            model.sunday = RotaBLL.SingleModel.GetDayStr(rotaInfo.sundayState);
            bool isok = RotaBLL.SingleModel.Update(model, "monday,tuesday,wensday,thursday,friday,saturday,sunday");
            string msg = isok ? "保存成功" : "保存失败";
            return Json(new { isok = isok, msg = msg });
        }
        #endregion
        #region 足浴技师端
        /// <summary>
        /// 关联客户端小程序
        /// </summary>
        /// <returns></returns>
        public ActionResult LinkClient()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "系统繁忙appid_null!", code = "500" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return View("PageError", new Return_Msg() { Msg = "系统繁忙relation_null!", code = "500" });
            }
            ViewBag.appId = appId;
            FootbathXcxRelation xcxRelation = FootbathXcxRelationBLL.SingleModel.GetModelByTechnicianAid(appId);
            List<XcxAppAccountRelation> clientAccountRelationList = XcxAppAccountRelationBLL.SingleModel.GetClientList(dzaccount.Id.ToString());
            List<FootbathClientModel> clientModelList = new List<FootbathClientModel>();
            if (clientAccountRelationList != null && clientAccountRelationList.Count > 0)
            {
                foreach (XcxAppAccountRelation client in clientAccountRelationList)
                {
                    FootbathClientModel clientModel = new FootbathClientModel();
                    clientModel.aid = client.Id;
                    OpenAuthorizerConfig authModel = OpenAuthorizerConfigBLL.SingleModel.GetListByaccoundidAndRid(dzaccount.Id.ToString(), client.Id).FirstOrDefault();
                    clientModel.xcxName = "未绑定";
                    if (authModel != null)
                    {
                        clientModel.xcxName = authModel.nick_name;
                    }
                    clientModel.relation = FootbathXcxRelationBLL.SingleModel.GetModelByClientAid(client.Id);
                    clientModelList.Add(clientModel);
                }
            }
            if (xcxRelation != null)
            {
                xcxRelation.clientXcxName = "未绑定";
                foreach (FootbathClientModel clienModel in clientModelList)
                {
                    if (clienModel.aid == xcxRelation.clientAid)
                    {
                        xcxRelation.clientXcxName = clienModel.xcxName;
                    }
                }
            }
            return View(new { xcxRelation, clientModelList, appId });
        }
        /// <summary>
        /// 关联客户端
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveClientLink()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appId_null" });
            }
            int clientAid = Context.GetRequestInt("clientAid", 0);
            if (clientAid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙clientAid_null" });
            }
            XcxAppAccountRelation clientRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(clientAid, dzaccount.Id.ToString());
            if (clientRelation == null)
            {
                return Json(new { isok = false, msg = "客户端小程序不存在" });
            }
            XcxAppAccountRelation technicianRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (technicianRelation == null)
            {
                return Json(new { isok = false, msg = "技师端小程序不存在" });
            }
            FootbathXcxRelation clientXcxRelation = FootbathXcxRelationBLL.SingleModel.GetModelByClientAid(clientRelation.Id);
            if (clientXcxRelation != null)
            {
                clientXcxRelation.state = -1;
                if (!FootbathXcxRelationBLL.SingleModel.Update(clientXcxRelation, "state"))
                {
                    return Json(new { isok = false, msg = "系统繁忙，client_error" });
                }

            }
            FootbathXcxRelation technicianXcxRelation = FootbathXcxRelationBLL.SingleModel.GetModelByTechnicianAid(technicianRelation.Id);
            if (technicianXcxRelation != null)
            {
                technicianXcxRelation.state = -1;
                if (!FootbathXcxRelationBLL.SingleModel.Update(technicianXcxRelation, "state"))
                {
                    return Json(new { isok = false, msg = "系统繁忙，technician_error" });
                }
            }
            bool isok = false;
            string msg = string.Empty;
            FootbathXcxRelation model = FootbathXcxRelationBLL.SingleModel.GetModelByTechnicianAidAndClientAid(dzaccount.Id.ToString(), technicianRelation.Id, clientRelation.Id);
            if (model != null)
            {
                model.state = 0;
                isok = FootbathXcxRelationBLL.SingleModel.Update(model, "state");
            }
            else
            {
                model = new FootbathXcxRelation();
                model.clientAid = clientRelation.Id;
                model.technicianAid = technicianRelation.Id;
                model.accountId = dzaccount.Id.ToString();
                isok = Convert.ToInt32(FootbathXcxRelationBLL.SingleModel.Add(model)) > 0;
            }
            msg = isok ? "操作成功" : "操作失败";
            return Json(new { isok, msg });
        }
        /// <summary>
        /// 取消关联
        /// </summary>
        /// <returns></returns>
        public ActionResult CancelClientLink()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appId_null" });
            }
            int clientAid = Context.GetRequestInt("clientAid", 0);
            if (clientAid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙clientAid_null" });
            }
            XcxAppAccountRelation clientRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(clientAid, dzaccount.Id.ToString());
            if (clientRelation == null)
            {
                return Json(new { isok = false, msg = "客户端小程序不存在" });
            }
            XcxAppAccountRelation technicianRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (technicianRelation == null)
            {
                return Json(new { isok = false, msg = "技师端小程序不存在" });
            }
            FootbathXcxRelation XcxRelation = FootbathXcxRelationBLL.SingleModel.GetValidModel(dzaccount.Id.ToString(), technicianRelation.Id, clientRelation.Id);
            if (XcxRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙_relation_null" });
            }
            XcxRelation.state = -1;
            if (FootbathXcxRelationBLL.SingleModel.Update(XcxRelation, "state"))
            {
                return Json(new { isok = true, msg = "操作成功" });
            }
            else
            {
                return Json(new { isok = false, msg = "操作失败" });
            }
        }
        #endregion

    }
}