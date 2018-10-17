using BLL.MiniApp;
using BLL.MiniApp.Pin;
using Entity.MiniApp;
using Entity.MiniApp.Pin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.PinAdmin.Controllers;
using Utility;

namespace User.MiniApp.Areas.Pin.Controllers
{
    /// <summary>
    /// 平台配置
    /// 商家管理
    /// 提现配置
    /// </summary>
    public class ConfigController : BaseController
    {
        #region 商家管理

        /// <summary>
        /// 商家管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetStoreList(int aid = 0, int pageIndex = 1, int pageSize = 10, int state = -999, string storeName = "", int isAgent = 0, int isBiaogan = 0, string phone = "", string nickName = "", string fnickName = "", string fphone = "")
        {
            if (aid <= 0 || pageIndex <= 0 || pageSize <= 0)
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result);
            }
            XcxAppAccountRelation xcxAppAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxAppAccountRelation == null)
            {
                result.code = 0;
                result.msg = "小程序不存在";
                return Json(result);
            }
            int recordCount = 0;


            List<PinStore> storeList = new List<PinStore>();
            if (isAgent != 1)//查看所有商家
            {
                int biaogan = isBiaogan == 0 ? -999 : isBiaogan;
                storeList = PinStoreBLL.SingleModel.GetListByCondition(xcxAppAccountRelation.AppId, aid, pageIndex, pageSize, out recordCount, state, storeName, phone, nickName, biaogan, fnickName, fphone);
                if (storeList != null && storeList.Count > 0)
                {
                    string userIds = string.Join(",",storeList.Select(s=>s.userId).Distinct());
                    List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);
                    storeList.ForEach(store =>
                    {
                        store.goodsCount = PinGoodsBLL.SingleModel.GetCountByStoreId(store.id);
                        store.agentInfo = PinAgentBLL.SingleModel.GetModelByUserId(store.userId);
                        if (store.agentInfo != null)
                        {
                            store.agentFee = PinAgentBLL.SingleModel.GetAgentFee(store.agentInfo.id);
                        }
                        if (store.agentId > 0)
                        {
                            store.fuserInfo = PinAgentBLL.SingleModel.GetUserInfoByAgentId(store.agentId);
                        }
                        C_UserInfo userInfo = userInfoList?.FirstOrDefault(f=>f.Id == store.userId);
                        store.nickName = userInfo != null ? userInfo.NickName : string.Empty;
                    });
                }
            }
            else//查看代理用户
            {
                List<PinAgent> agentList = PinAgentBLL.SingleModel.GetListByAid_State(xcxAppAccountRelation.AppId, aid, pageSize, pageIndex, out recordCount, storeName, phone, nickName, fnickName, fphone);
                if (agentList != null && agentList.Count > 0)
                {
                    foreach (var agent in agentList)
                    {
                        PinStore store = PinStoreBLL.SingleModel.GetModelByAid_UserId(agent.aId, agent.userId);
                        if (store != null && store.state != -1)
                        {
                            store.goodsCount = PinGoodsBLL.SingleModel.GetCountByStoreId(store.id);
                            store.agentInfo = agent;
                            store.userId = agent.userId;
                        }
                        else
                        {
                            store = new PinStore() { storeName = "未开通店铺", userId = agent.userId };
                            store.agentInfo = agent;
                        }
                        store.agentFee = PinAgentBLL.SingleModel.GetAgentFee(store.agentInfo.id);
                        C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(agent.userId);
                        store.nickName = userInfo != null ? userInfo.NickName : string.Empty;
                        if (agent.fuserId > 0)
                        {
                            store.fuserInfo = C_UserInfoBLL.SingleModel.GetModel(agent.fuserId);
                        }
                        storeList.Add(store);
                    }
                }
            }

            result.code = 1;
            result.obj = new { list = storeList, recordCount };
            return Json(result);
        }

        public ActionResult StoreEdit(PinStore store, string act = "")
        {
            if (store == null || store.aId <= 0 || store.id <= 0)
            {
                if (act != "save")
                {
                    return Content("参数错误");
                }
                else
                {
                    result.code = 0;
                    result.msg = "参数错误";
                    return Json(result);
                }
            }
            PinStore model = PinStoreBLL.SingleModel.GetModelByAid_Id(store.aId, store.id);
            if (act != "save")
            {
                return View(model);
            }
            else
            {
                if (model == null)
                {
                    result.code = 0;
                    result.msg = "店铺不存在";
                    return Json(result);
                }
                if (DateTime.Compare(store.endDate, store.startDate) < 0)
                {
                    result.code = 0;
                    result.msg = "请输入合理的时间范围";
                    return Json(result);
                }
                model.storeName = store.storeName;
                model.logo = store.logo;
                model.startDate = store.startDate;
                model.endDate = store.endDate;
                model.state = store.state;
                model.rz = store.rz;
                model.loginName = store.loginName;
                if (!string.IsNullOrEmpty(store.password))
                {
                    model.password = DESEncryptTools.GetMd5Base32(store.password);
                }
                if (PinStoreBLL.SingleModel.Update(model, "storeName,logo,startDate,endDate,state,rz,loginName,password"))
                {
                    result.code = 1;
                    result.msg = "保存成功";
                    return Json(result);
                }
                else
                {
                    result.code = 0;
                    result.msg = "保存失败";
                    return Json(result);
                }
            }
        }

        public ActionResult DelStore(int aid, int id)
        {
            if (aid <= 0 || id <= 0)
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result);
            }
            PinStore model = PinStoreBLL.SingleModel.GetModelByAid_Id(aid, id);
            if (model == null)
            {
                result.code = 0;
                result.msg = "店铺不存在";
                return Json(result);
            }
            else
            {
                model.state = -1;
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(model.userId);
                if (userInfo == null)
                {
                    result.code = 0;
                    result.msg = "商家不存在";
                    return Json(result);
                }
                userInfo.StoreId = 0;
                if (PinStoreBLL.SingleModel.Update(model, "state") && C_UserInfoBLL.SingleModel.Update(userInfo, "storeId"))
                {
                    result.code = 1;
                    result.msg = "操作成功";
                    return Json(result);
                }
                else
                {
                    result.code = 0;
                    result.msg = "操作失败";
                    return Json(result);
                }
            }
        }

        public ActionResult EnterStoreManage(int aId = 0, int id = 0)
        {
            result.code = 1;
            result.msg = Utility.DESEncryptTools.DESEncrypt(id.ToString());
            return Json(result);
        }


        public ActionResult SetAgent(int aid = 0, int userId = 0, int state = -1)
        {
            int agentMoney = Utility.IO.Context.GetRequestInt("agentMoney", 0);//代理费用

            if (aid <= 0 || userId <= 0)
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                result.code = 0;
                result.msg = "用户不存在";
                return Json(result);
            }

            string msg = "";
            result.code = PinAgentBLL.SingleModel.SetAgent(aid, userInfo.Id, state, out msg,agentMoney);
            result.msg = msg;
            return Json(result);
        }

        public ActionResult SetBiaoGan(int aid = 0, int sId = 0, int state = -1)
        {
            if (aid <= 0 || sId <= 0)
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result);
            }
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(aid, sId);
            if (store == null)
            {
                result.msg = "门店不存在或者已关闭";
                return Json(result);
            }
            store.biaogan = state;
            result.code = PinStoreBLL.SingleModel.Update(store, "biaogan") ? 1 : 0;
            result.msg = result.code > 0 ? "操作成功" : "操作失败";
            return Json(result);
        }
        #endregion 商家管理

        #region 平台配置

        /// <summary>
        /// 平台配置
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public ActionResult Setting(int aid = 0)
        {
            if (aid <= 0)
            {
                return Content("参数错误");
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelById(aid);
            if (xcx == null)
            {
                return Content("小程序不存在");
            }

            List<PinPicture> picList = PinPictureBLL.SingleModel.GetListByAid_Type(aid, 0);
            picList.ForEach(pic => pic.funModel = funList.Where(fun => fun.id == pic.funId).FirstOrDefault());

            ViewData["picList"] = picList;

            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(aid);
            if (platform == null)
            {
                platform = new PinPlatform() { aid = aid };
                platform.Id = Convert.ToInt32(PinPlatformBLL.SingleModel.Add(platform));
            }
            platform.poster = Microsoft.JScript.GlobalObject.unescape(platform.poster);
            return View(platform);
        }

        public ActionResult saveSetting(int aid = 0, int openKf = -1, int freeDays = 0, string kfPhone = "", int agentFee = 0, int agentExtract = 0, int orderExtract = 0, int agentProtectDays = 0, string ServiceWeiXin = "", int orderSuccessDays = 0, int firstExtract = 0, int secondExtract = 0, int agentSearchPort = 0, int addStorePort = 0)
        {
            if (aid <= 0 || (openKf != 0 && openKf != 1))
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result);
            }
            if (freeDays <= 0)
            {
                result.code = 0;
                result.msg = "免费体验天数：请输入大于0的整数";
                return Json(result);
            }
            if (openKf == 1 && string.IsNullOrEmpty(kfPhone))
            {
                result.code = 0;
                result.msg = "请输入客服号码";
                return Json(result);
            }
            if (agentFee <= 0)
            {
                result.code = 0;
                result.msg = "代理费不能少于0元";
                return Json(result);
            }
            if (agentProtectDays <= 0)
            {
                result.code = 0;
                result.msg = "代理保护期不能小于0";
                return Json(result);
            }
            if (agentExtract <= 0)
            {
                result.code = 0;
                result.msg = "入驻提成不能少于零";
                return Json(result);
            }
            if (orderExtract <= 0)
            {
                result.code = 0;
                result.msg = "订单提成不能少于零";
                return Json(result);
            }


            if (firstExtract < 0 || secondExtract < 0)
            {
                result.code = 0;
                result.msg = "直接分佣比例或者间接分佣比例不能少于零";
                return Json(result);
            }


            if ((firstExtract + secondExtract) > 1000)
            {
                result.code = 0;
                result.msg = "直接分佣比例跟间接分佣比例之和不能大于100%";
                return Json(result);
            }



            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(aid);
            if (platform == null)
            {
                result.code = 0;
                result.msg = "平台不存在";
                return Json(result);
            }
            platform.openKf = openKf;
            platform.freeDays = freeDays;
            platform.kfPhone = kfPhone;
            platform.agentFee = agentFee;
            platform.agentExtract = agentExtract;
            platform.orderExtract = orderExtract;
            platform.agentProtectDays = agentProtectDays;
            platform.ServiceWeiXin = ServiceWeiXin;
            platform.orderSuccessDays = orderSuccessDays;
            platform.FirstExtract = firstExtract;
            platform.SecondExtract = secondExtract;
            platform.AgentSearchPort = agentSearchPort;
            platform.AddStorePort = addStorePort;
            if (PinPlatformBLL.SingleModel.Update(platform, "AddStorePort,FirstExtract,SecondExtract,AgentSearchPort,freedays,openkf,kfphone,agentFee,agentExtract,orderExtract,agentProtectDays,ServiceWeiXin"))
            {
                result.code = 1;
                result.msg = "保存成功";
                return Json(result);
            }
            else
            {
                result.code = 0;
                result.msg = "保存失败";
                return Json(result);
            }
        }

        /// <summary>
        /// 添加广告图片
        /// </summary>
        /// <param name="pic"></param>
        /// <param name="act"></param>
        /// <returns></returns>
        public ActionResult AddPic(PinPicture pic, string act = "")

        {
            if (pic == null || pic.aid <= 0)
            {
                if (act == "save")
                {
                    result.code = 0;
                    result.msg = "参数错误";
                    return Json(result);
                }
                return Content("参数错误");
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelById(pic.aid);
            if (act == "save")
            {
                if (xcx == null)
                {
                    result.code = 0;
                    result.msg = "小程序不存在";
                    return Json(result);
                }
                if (string.IsNullOrEmpty(pic.img))
                {
                    result.code = 0;
                    result.msg = "请选择广告图片";
                    return Json(result);
                }

                if (pic.funId == 1)
                {
                    if (string.IsNullOrEmpty(pic.target))
                    {
                        result.msg = "请选择跳转目标";
                        return Json(result);
                    }
                    pic.target = $"/pages/shopping/goodInfo/goodInfo?gid={pic.target}";
                }
                else
                {
                    pic.target = "/pages/store/applyEnter/applyEnter";
                }

                pic.Id = Convert.ToInt32(PinPictureBLL.SingleModel.Add(pic));
                if (pic.Id > 0)
                {
                    result.code = 1;
                    result.msg = "保存成功";
                    return Json(result);
                }
                else
                {
                    result.code = 0;
                    result.msg = "保存失败";
                    return Json(result);
                }
            }
            else
            {
                if (xcx == null)
                {
                    return Content("小程序不存在");
                }

                return View(funList);
            }
        }

        /// <summary>
        /// 删除广告图片
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult delPic(int aid = 0, int id = 0)
        {
            if (aid <= 0 || id <= 0)
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelById(aid);
            if (xcx == null)
            {
                result.code = 0;
                result.msg = "小程序不存在";
                return Json(result);
            }

            PinPicture pic = PinPictureBLL.SingleModel.GetModelByAid_Id(aid, id);
            if (pic == null)
            {
                result.code = 0;
                result.msg = "数据不存在";
                return Json(result);
            }
            pic.state = -1;
            if (PinPictureBLL.SingleModel.Update(pic, "state"))
            {
                result.code = 1;
                result.msg = "操作成功";
                return Json(result);
            }
            else
            {
                result.code = 0;
                result.msg = "操作失败";
                return Json(result);
            }
        }
        public ActionResult SavePoster(int aid = 0, string poster = "")
        {
            if (aid <= 0 || string.IsNullOrEmpty(poster))
            {
                result.msg = "参数错误";
                return Json(result);
            }


            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(aid);
            if (platform == null)
            {
                result.code = 0;
                result.msg = "平台不存在";
                return Json(result);
            }
            platform.posterbk = platform.poster;
            platform.poster = poster;
            if (PinPlatformBLL.SingleModel.Update(platform, "posterbk,poster"))
            {
                result.code = 1;
                result.msg = "保存成功";
            }
            else
            {
                result.msg = "保存失败";
            }
            return Json(result);
        }
        #endregion 平台配置
        #region 提现配置
        public ActionResult TxSetting(int aid = 0)
        {
            if (aid <= 0)
            {
                return Content("参数错误");
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelById(aid);
            if (xcx == null)
            {
                return Content("小程序不存在");
            }

            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(xcx.Id);
            if (platform == null)
            {
                platform = new PinPlatform() { aid = xcx.Id };
                platform.Id = Convert.ToInt32(PinPlatformBLL.SingleModel.Add(platform));
            }
            return View(platform);

        }
        public ActionResult SaveTxSetting(int aid = 0, int minTxMoney = 0, int dealDays = 0, int serviceFee = 0, int toWx = 0, int toBank = 0, int qrcodeServiceFee = 0, int agentServiceFee = 0)
        {
            if (aid <= 0 || serviceFee < 0 || qrcodeServiceFee < 0 || agentServiceFee < 0)
            {
                result.msg = "参数错误";
                return Json(result);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelById(aid);
            if (xcx == null)
            {
                result.msg = "小程序不存在";
                return Json(result);
            }

            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(xcx.Id);
            if (platform == null)
            {
                result.msg = "平台信息错误";
                return Json(result);
            }
            platform.minTxMoney = minTxMoney;
            platform.dealDays = dealDays;
            platform.serviceFee = serviceFee;
            platform.qrcodeServiceFee = qrcodeServiceFee;
            platform.agentServiceFee = agentServiceFee;
            //platform.toWx = toWx;
            platform.toBank = toBank;
            result.code = PinPlatformBLL.SingleModel.Update(platform, "minTxMoney,dealDays,serviceFee,qrcodeServiceFee,agentServiceFee") ? 1 : 0;
            result.msg = result.code == 1 ? "保存成功" : "保存失败";
            return Json(result);
        }
        #endregion

        public ActionResult GetGoodsList(int aid = 0, int pageIndex = 1, int pageSize = 15)
        {
            if (aid <= 0)
            {
                result.msg = "参数错误";
                return Json(result);
            }
            int recordCount = 0;
            List<PinGoods> goodsList = PinGoodsBLL.SingleModel.GetListByAid_State(aid, (int)PinEnums.GoodsState.上架, pageIndex, pageSize, out recordCount);
            if (goodsList != null && goodsList.Count > 0)
            {
                string storeIds = string.Join(",",goodsList.Select(s=>s.storeId).Distinct());
                List<PinStore> pinStoreList = PinStoreBLL.SingleModel.GetListByIds(storeIds);
                goodsList.ForEach(goods =>
                {
                    PinStore store = pinStoreList?.FirstOrDefault(f=>f.id == goods.storeId);
                    if (store != null)
                    {
                        goods.storeName = store.storeName;
                    }
                });
            }
            result.code = 1;
            result.obj = new { list = goodsList, recordCount };
            return Json(result);
        }


        #region 代理等级配置

        /// <summary>
        /// 代理等级配置
        /// </summary>
        /// <returns></returns>
        public ActionResult AgentLevelConfig()
        {
            int aid = Utility.IO.Context.GetRequestInt("aid", 0);
            if (aid <= 0)
            {
                return Content("参数错误");
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelById(aid);
            if (xcx == null)
            {
                return Content("小程序不存在");
            }

            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(xcx.Id);
            if (platform == null)
            {
                platform = new PinPlatform() { aid = xcx.Id };
                platform.Id = Convert.ToInt32(PinPlatformBLL.SingleModel.Add(platform));
            }

            ViewBag.PinAgentLevelConfigList = PinAgentLevelConfigBLL.SingleModel.GetListByAid(aid);

            ViewBag.aid = aid;

            return View(platform);
        }

        /// <summary>
        /// 保存代理等级配置
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveAgentLevelConfig(PinAgentLevelConfig agentLevelConfig)
        {

            if (agentLevelConfig == null)
            {
                result.msg = "数据不能为空!";
                return Json(result);
            }

            PinAgentLevelConfig model = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(agentLevelConfig.Id);
            if (model == null)
            {
                result.msg = "该等级不存在数据库里!";
                return Json(result);
            }
            model.AgentFee = agentLevelConfig.AgentFee;
            model.AgentExtract = agentLevelConfig.AgentExtract;
            model.OrderExtract = agentLevelConfig.OrderExtract;
            model.SecondAgentExtract = agentLevelConfig.SecondAgentExtract;
            model.SecondOrderExtract = agentLevelConfig.SecondOrderExtract;
            model.UpdateTime = DateTime.Now;


            if (PinAgentLevelConfigBLL.SingleModel.Update(model, "AgentFee,AgentExtract,OrderExtract,SecondAgentExtract,SecondOrderExtract,UpdateTime"))
            {
                result.code = 1;
                result.msg = "保存成功";
            }
            else
            {
                result.msg = "保存失败";
            }
            return Json(result);
        }

        /// <summary>
        /// 保存代理政策
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveAgentPolicy()
        {
            int aid = Utility.IO.Context.GetRequestInt("aid", 0);
            string agentPolicy = Utility.IO.Context.GetRequest("agentPolicy", string.Empty);

            if (aid <= 0 || string.IsNullOrEmpty(agentPolicy))
            {
                result.msg = "参数错误";
                return Json(result);
            }


            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(aid);
            if (platform == null)
            {
                result.code = 0;
                result.msg = "平台不存在";
                return Json(result);
            }

            platform.AgentPolicy = agentPolicy;
            if (PinPlatformBLL.SingleModel.Update(platform, "AgentPolicy"))
            {
                result.code = 1;
                result.msg = "保存成功";
            }
            else
            {
                result.msg = "保存失败";
            }
            return Json(result);
        }

        /// <summary>
        /// 保存代理配置
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveAgentSetting()
        {
            int aid = Utility.IO.Context.GetRequestInt("aid", 0);
            int agentProtectDays = Utility.IO.Context.GetRequestInt("agentProtectDays", 0);
            int jumpExtract = Utility.IO.Context.GetRequestInt("jumpExtract", 0);
            int agentSearchPort = Utility.IO.Context.GetRequestInt("agentSearchPort", 0);

            if (aid <= 0)
            {
                result.msg = "参数错误";
                return Json(result);
            }
            if (agentProtectDays <= 0)
            {
                result.msg = "代理保护期不能小于0";
                return Json(result);
            }
            if (jumpExtract <= 0)
            {

                result.msg = "越级分佣比不能少于零";
                return Json(result);
            }
            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(aid);
            if (platform == null)
            {
                result.msg = "平台不存在";
                return Json(result);
            }


            platform.agentProtectDays = agentProtectDays;
            platform.AgentSearchPort = agentSearchPort;
            platform.JumpExtract = jumpExtract;


            if (PinPlatformBLL.SingleModel.Update(platform, "agentProtectDays,AgentSearchPort,JumpExtract"))
            {
                result.code = 1;
                result.msg = "保存成功";
            }
            else
            {
                result.msg = "保存失败";
            }
            return Json(result);
        }
        #endregion



        public ActionResult AgentMgr(int aid = 0)
        {
            if (aid <= 0)
            {
                return Content("参数错误");
            }
            ViewBag.aid = aid;
            return View();
        }


        /// <summary>
        /// 获取代理商列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAgentList()
        {
            int aid = Utility.IO.Context.GetRequestInt("aid", 0);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            string nickName = Utility.IO.Context.GetRequest("nickName", string.Empty);
            string phone = Utility.IO.Context.GetRequest("phone", string.Empty);
            string fnickName = Utility.IO.Context.GetRequest("fnickName", string.Empty);
            string fphone = Utility.IO.Context.GetRequest("fphone", string.Empty);
            if (aid <= 0)
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAppAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxAppAccountRelation == null)
            {
                result.code = 0;
                result.msg = "小程序不存在";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            int recordCount = 0;


            List<PinAgent> agentList = PinAgentBLL.SingleModel.GetListByAid_State(xcxAppAccountRelation.AppId, aid, pageSize, pageIndex, out recordCount, phone: phone, nickName: nickName, fnickName: fnickName, fphone: fphone);
            if (agentList != null && agentList.Count > 0)
            {
                string userIds = string.Join(",",agentList.Select(s=>s.userId).Distinct());
                string fuserIds = string.Join(",",agentList.Where(w=>w.fuserId>0)?.Select(s=>s.fuserId).Distinct());
                if(!string.IsNullOrEmpty(fuserIds))
                {
                    if(!string.IsNullOrEmpty(userIds))
                    {
                        userIds = userIds + ","+fuserIds;
                    }
                    else
                    {
                        userIds = fuserIds;
                    }
                }
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);
                agentList.ForEach(x =>
                {
                    C_UserInfo userInfo = userInfoList?.FirstOrDefault(f=>f.Id == x.userId);
                    if (userInfo != null)
                    {
                        x.userInfo = userInfo;
                    }

                    PinAgentLevelConfig pinAgentLevel = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(x.AgentLevel, aid);
                    if (pinAgentLevel != null)
                    {
                        x.AgentLevelName = pinAgentLevel.LevelName;
                        x.AgentLevelTimeList.Add(new { Time = -1, Txt = "请选择", Money = 0 });
                        x.AgentLevelTimeList.Add(new { Time = 1, Txt = "1年", Money = pinAgentLevel.AgentFee * 0.01 });
                        x.AgentLevelTimeList.Add(new { Time = 2, Txt = "2年", Money = pinAgentLevel.AgentFee * 0.01 * 2 });
                        x.AgentLevelTimeList.Add(new { Time = 3, Txt = "3年", Money = pinAgentLevel.AgentFee * 0.01 * 3 });
                        x.AgentLevelTimeList.Add(new { Time = 4, Txt = "4年", Money = pinAgentLevel.AgentFee * 0.01 * 4 });
                        x.AgentLevelTimeList.Add(new { Time = 5, Txt = "5年", Money = pinAgentLevel.AgentFee * 0.01 * 5 });
                    }


                    if (x.fuserId > 0)
                    {
                        PinAgentDec pinAgentDec = new PinAgentDec();

                        C_UserInfo fuserInfo = userInfoList?.FirstOrDefault(f=>f.Id == x.fuserId);
                        if (fuserInfo != null)
                        {
                            pinAgentDec.UserId = fuserInfo.Id;
                            pinAgentDec.UserName = fuserInfo.NickName;
                            pinAgentDec.Phone = fuserInfo.TelePhone;
                        }

                        PinAgent fpinAgent = PinAgentBLL.SingleModel.GetModelByUserId(x.fuserId);
                        if (fpinAgent != null)
                        {
                            PinAgentLevelConfig pinFAgentLevel = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(fpinAgent.AgentLevel, aid);
                            if (pinFAgentLevel != null)
                            {
                                pinAgentDec.AgentLevelName = pinFAgentLevel.LevelName;
                                pinAgentDec.AgentLevel = pinFAgentLevel.LevelId;

                            }
                        }

                        x.pinAgentDec = pinAgentDec;


                    }

                    x.TotalAgentMoney = PinAgentBLL.SingleModel.GetAgentFee(x.id);



                });
            }


            result.code = 1;
            result.obj = new { list = agentList, recordCount };
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取可升级的代理等级
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUpdateAgentLevelList()
        {
            int aid = Utility.IO.Context.GetRequestInt("aid", 0);
            int agentLevel = Utility.IO.Context.GetRequestInt("agentLevel", 0);

            if (aid <= 0)
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAppAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxAppAccountRelation == null)
            {
                result.code = 0;
                result.msg = "小程序不存在";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            List<PinAgentLevelConfig> list = PinAgentLevelConfigBLL.SingleModel.GetUpdatePinAgentLevelList(agentLevel, aid);
            list.Insert(0, new PinAgentLevelConfig() { LevelId = -1, LevelName = "请选择" });

            result.code = 1;
            result.obj = new { list = list };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 代理升级
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateAgentLevel()
        {
            int aid = Utility.IO.Context.GetRequestInt("aid", 0);
            int agentLevel = Utility.IO.Context.GetRequestInt("agentLevel", 0);
            int agentId = Utility.IO.Context.GetRequestInt("agentId", 0);
            int agentFee= Utility.IO.Context.GetRequestInt("agentFee", 0);
            if (aid <= 0 || agentId <= 0)
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result);
            }
            XcxAppAccountRelation xcxAppAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxAppAccountRelation == null)
            {
                result.code = 0;
                result.msg = "小程序不存在";
                return Json(result);
            }

            PinAgent pinAgent = PinAgentBLL.SingleModel.GetModel(agentId);
            if (pinAgent == null)
            {
                result.code = 0;
                result.msg = "代理商不存在";
                return Json(result);
            }
            if (pinAgent.state == 0)
            {
                result.code = 0;
                result.msg = "代理商不可用";
                return Json(result);
            }
            int oldAgentLevel = pinAgent.AgentLevel;//保存之前的等级
            DateTime oldAddtime = pinAgent.addTime;
            pinAgent.AgentLevel = agentLevel;
            pinAgent.addTime = DateTime.Now;
            PinAgentLevelConfig pinAgentLevel = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(agentLevel, aid);
            if (pinAgentLevel == null)
            {
                result.code = 0;
                result.msg = "升级的等级不存在";
                return Json(result);
            }
            if (pinAgentLevel.State == -1)
            {
                result.code = 0;
                result.msg = "升级的等级被删除了";
                return Json(result);
            }
            agentFee = agentFee < 0 ? pinAgentLevel.AgentFee : agentFee;

            if (!PinAgentBLL.SingleModel.UpdateAgentLevel(pinAgent, agentFee, oldAgentLevel, oldAddtime))
            {
                result.code = 0;
                result.msg = "升级失败";
                return Json(result);
            }

            result.code = 1;
            result.msg = "升级成功";
            return Json(result);



        }


        /// <summary>
        /// 代理续费
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddAgentTime()
        {
            int aid = Utility.IO.Context.GetRequestInt("aid", 0);
            int timeLength = Utility.IO.Context.GetRequestInt("timeLength", 0);
            int agentId = Utility.IO.Context.GetRequestInt("agentId", 0);
            if (aid <= 0 || agentId <= 0 || timeLength <= 0)
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result);
            }
            XcxAppAccountRelation xcxAppAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxAppAccountRelation == null)
            {
                result.code = 0;
                result.msg = "小程序不存在";
                return Json(result);
            }

            PinAgent pinAgent = PinAgentBLL.SingleModel.GetModel(agentId);
            if (pinAgent == null)
            {
                result.code = 0;
                result.msg = "代理商不存在";
                return Json(result);
            }
            if (pinAgent.state == 0)
            {
                result.code = 0;
                result.msg = "代理商不可用";
                return Json(result);
            }


            PinAgentLevelConfig pinAgentLevel = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(pinAgent.AgentLevel, aid);
            if (pinAgentLevel == null)
            {
                result.code = 0;
                result.msg = "续期失败,代理商的等级不存在";
                return Json(result);
            }
            if (pinAgentLevel.State == -1)
            {
                result.code = 0;
                result.msg = "续期失败,代理商的等级被删除了";
                return Json(result);
            }

            if (!PinAgentBLL.SingleModel.AddAgentTime(pinAgent, pinAgentLevel.AgentFee*timeLength,timeLength))
            {
                result.code = 0;
                result.msg = "续期失败";
                return Json(result);
            }



            result.code = 1;
            result.msg = "续期成功";
            return Json(result);



        }


        /// <summary>
        /// 获取代理商花费记录
        /// </summary>
        /// <returns></returns>
        public ActionResult GetChangeLogList()
        {
            int aid = Utility.IO.Context.GetRequestInt("aid", 0);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int agentId = Utility.IO.Context.GetRequestInt("agentId", 0);

            if (aid <= 0)
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result,JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAppAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxAppAccountRelation == null)
            {
                result.code = 0;
                result.msg = "小程序不存在";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            int recordCount = 0;

            List<PinAgentChangeLog> list = PinAgentChangeLogBLL.SingleModel.GetListByAgentId(agentId, aid,out recordCount, pageIndex, pageSize);

            result.code = 1;
            result.obj = new { list = list, recordCount };
            return Json(result, JsonRequestBehavior.AllowGet);
        }


    }
}