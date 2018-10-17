using Api.MiniApp.Filters;
using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Im;
using BLL.MiniApp.Qiye;
using BLL.MiniApp.Tools;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Im;
using Entity.MiniApp.Qiye;
using Entity.MiniApp.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    [AuthCheckLoginSessionKey]
    public class apiQiyeController : InheritController
    {
        protected readonly object _lockobj = new object();
        #region 企业相关
        /// <summary>
        /// 企业信息
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public ActionResult GetQiyeInfo(int aid = 0)
        {
           
            returnObj = new Return_Msg_APP();
            if (aid <= 0)
            {
                returnObj.Msg = "无效参数";
                return Json(returnObj);
            }

            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModel(aid);
            if (xcxrelation == null)
            {
                returnObj.Msg = "模板过期";
                return Json(returnObj);
            }

            Miniapp listTemp = MiniappBLL.SingleModel.GetModelByRelationId(aid);
            if (listTemp == null)
            {
                returnObj.Msg = "无效数据";
                return Json(returnObj);
            }

            //首页数据
            Moduls moduls = ModulsBLL.SingleModel.GetModelByAppidandLevel(listTemp.Id, (int)Miapp_Miniappmoduls_Level.ModelData);
            if (moduls != null)
            {
                listTemp.StoreName = string.IsNullOrEmpty(listTemp.StoreName) ? moduls.Name : listTemp.StoreName;
            }

            List<C_Attachment> imgs = C_AttachmentBLL.SingleModel.GetListByCache(listTemp.Id, (int)AttachmentItemType.小程序智慧官网形象图, true);
            listTemp.BannersImgUrls = new List<string>();
            if (imgs != null && imgs.Count > 0)
            {
                foreach (C_Attachment item in imgs)
                {
                    listTemp.BannersImgUrls.Add(item.filepath);
                }
            }

            //联系我数据
            moduls = ModulsBLL.SingleModel.GetModelByAppidandLevel(listTemp.Id, (int)Miapp_Miniappmoduls_Level.SixModel);
            if (moduls != null)
            {
                listTemp.Phone = string.IsNullOrEmpty(listTemp.Phone) ? moduls.mobile : listTemp.Phone;
                listTemp.Address = string.IsNullOrEmpty(listTemp.Address) ? moduls.Address : listTemp.Address;
                listTemp.Location = string.IsNullOrEmpty(listTemp.Location) ? moduls.AddressPoint : listTemp.Location;
            }
           
            List<QiyeEmployee> listQiyeEmployee = QiyeEmployeeBLL.SingleModel.GetQiyeKeFu(aid, xcxrelation.AppId);
            if (listQiyeEmployee != null && listQiyeEmployee.Count > 0)
            {
                listTemp.ListKeFu = new List<QiyeKeFu>();
               listQiyeEmployee.ForEach(x =>
                {
                    listTemp.ListKeFu.Add(new QiyeKeFu()
                    {
                        EmployeeId = x.Id,
                        Name = x.Name,
                        Avatar = x.Avatar,
                        UserId = x.UserId
                    });

                });

            }

            returnObj.isok = true;
            returnObj.dataObj = listTemp;
            return Json(returnObj);
        }

        /// <summary>
        /// 企业资讯
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ActionResult GetCompanyNews(int aid = 0, int pageSize = 10, int pageIndex = 1)
        {
            returnObj = new Return_Msg_APP();
            if (aid <= 0)
            {
                returnObj.Msg = "无效参数";
                return Json(returnObj);
            }

            Miniapp bmodel = MiniappBLL.SingleModel.GetModelByRelationId(aid);
            if (bmodel == null)
            {
                returnObj.Msg = "无效数据";
                return Json(returnObj);
            }

            List<Moduls> list = ModulsBLL.SingleModel.GetListByAppidandLevel(bmodel.Id, (int)Miapp_Miniappmoduls_Level.EightModel, pageIndex, pageSize);
            int count = ModulsBLL.SingleModel.GetListByAppidandLevelCount(bmodel.Id, (int)Miapp_Miniappmoduls_Level.EightModel);
            returnObj.dataObj = new { data = list, count = count };
            returnObj.isok = true;
            return Json(returnObj);
        }


        /// <summary>
        /// 企业资讯详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetCompanyNewsDetail(int id = 0)
        {
            returnObj = new Return_Msg_APP();
            Moduls model = ModulsBLL.SingleModel.GetModel(id);
            if (model == null)
            {
                returnObj.Msg = "数据已过期";
                return Json(returnObj);
            }
            model.ViewCount += 1;
            ModulsBLL.SingleModel.Update(model);

            returnObj.dataObj = model;
            returnObj.isok = true;
            return Json(returnObj);
        }

        /// <summary>
        /// 发展历程
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ActionResult GetDevelopmentDataList(int aid = 0, int pageSize = 10, int pageIndex = 1)
        {
            returnObj = new Return_Msg_APP();
            if (aid <= 0)
            {
                returnObj.Msg = "无效参数";
                return Json(returnObj);
            }
            Miniapp model = MiniappBLL.SingleModel.GetModelByRelationId(aid);
            if (model == null)
            {
                returnObj.Msg = "无效数据";
                return Json(returnObj);
            }

            List<Development> list = DevelopmentBLL.SingleModel.GetListByAppid(model.Id, pageIndex, pageSize);
            int count = DevelopmentBLL.SingleModel.GetListByAppidCount(model.Id);
            returnObj.dataObj = new { data = list, count = count };
            returnObj.isok = true;
            return Json(returnObj);
        }
        #endregion

        #region 产品相关
        /// <summary>
        /// 获取产品类别配置 来进行样式的显示
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGoodsCategoryLevel()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);

            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            QiyeGoodsCategoryConfig qiyeGoodsCategoryConfig = QiyeGoodsCategoryConfigBLL.SingleModel.GetModelByAid(r.Id);
            QiyeSwitchModel switchModel = new QiyeSwitchModel();
            if (qiyeGoodsCategoryConfig == null)
            {
                qiyeGoodsCategoryConfig = new QiyeGoodsCategoryConfig() { Aid = r.Id, AddTime = DateTime.Now, UpdateTime = DateTime.Now };

                qiyeGoodsCategoryConfig.SwitchConfig = JsonConvert.SerializeObject(switchModel);
                if (Convert.ToInt32(QiyeGoodsCategoryConfigBLL.SingleModel.Add(qiyeGoodsCategoryConfig)) <= 0)
                {

                    returnObj.Msg = "初始化数据失败";
                    return Json(returnObj, JsonRequestBehavior.AllowGet);
                }
            }

            int productCategoryLevel = 1;
            if (!string.IsNullOrEmpty(qiyeGoodsCategoryConfig.SwitchConfig))
            {
                switchModel = Newtonsoft.Json.JsonConvert.DeserializeObject<QiyeSwitchModel>(qiyeGoodsCategoryConfig.SwitchConfig);
                productCategoryLevel = switchModel.ProductCategoryLevel;
            }

            returnObj.dataObj = new { Level = productCategoryLevel };
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取产品类别
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGoodsCategory()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);

            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int isFirstType = Context.GetRequestInt("isFirstType", 1);
            int parentId = Context.GetRequestInt("parentId", 0);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            int totalCount = 0;
            List<QiyeGoodsCategory> list = QiyeGoodsCategoryBLL.SingleModel.getListByaid(r.Id, out totalCount, isFirstType, pageSize, pageIndex, "sortNumber desc,addTime desc", parentId);


            returnObj.dataObj = new
            {
                totalCount = totalCount,
                list = list

            };
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="typeid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public ActionResult GetGoodsList(int aid, string typeid = "", int pageindex = 1, int pagesize = 10, int isFirstType = -1)
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string search = Context.GetRequest("search", string.Empty);
            string priceSort = Context.GetRequest("priceSort", string.Empty);
            string saleCountSort = Context.GetRequest("saleCountSort", string.Empty);
            string entGoodTypeIds = string.Empty;

            if (!string.IsNullOrEmpty(typeid))
            {
                typeid = EncodeHelper.ReplaceSqlKey(typeid);
                typeid = Server.UrlDecode(typeid);
            }
            List<QiyeGoods> goodslist = QiyeGoodsBLL.SingleModel.GetListGoods(aid, search, typeid, priceSort, pagesize, pageindex, isFirstType, saleCountSort);
            //log4net.LogHelper.WriteInfo(this.GetType(), JsonConvert.SerializeObject(goodslist));
            if (goodslist != null)
            {


                goodslist.ForEach((Action<QiyeGoods>)(goodModel =>
                {

                    if (!string.IsNullOrEmpty(goodModel.Categorys))
                    {
                        goodModel.CategorysStr = QiyeGoodsCategoryBLL.SingleModel.GetQiyeGoodsCategoryName(goodModel.Categorys);
                    }

                    if (!string.IsNullOrEmpty(goodModel.Plabels))
                    {
                        goodModel.PlabelStr = QiyeGoodsLabelBLL.SingleModel.GetGoodsLabel(goodModel.Plabels);
                        goodModel.PlabelStr_Arry = goodModel.PlabelStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                }));

            }
            var postdata = new
            {
                goodslist = goodslist.Select(g => new
                {
                    Id = g.Id,
                    Img = g.Img,
                    Name = g.Name,
                    Plabelstr_array = g.PlabelStr_Arry,
                    PriceFen = g.PriceFen,
                    DiscountPricestr = g.DiscountPricestr,
                    Discount = g.Discount,
                    Unit = g.Unit,
                    VirtualSalesCount = g.VirtualSalesCount,
                    SalesCount = g.SalesCount,
                    Price = g.Price,
                    PV = g.PV,
                    GoodType = g.GoodType,
                    GoodTypeStr = g.GoodTypeStr
                }),
            };

            returnObj.isok = true;
            returnObj.dataObj = postdata;
            return Json(returnObj, JsonRequestBehavior.AllowGet);


        }

        public ActionResult GetGoodInfo()
        {
            int pid = Context.GetRequestInt("pid", 0);

            returnObj = new Return_Msg_APP();
            returnObj.code = "200";

            if (pid == 0)
            {
                returnObj.Msg = "请选择产品";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            QiyeGoods goodModel = QiyeGoodsBLL.SingleModel.GetModel(pid);
            if (goodModel == null || goodModel.State == 0)
            {
                returnObj.Msg = "产品不存在或已删除";
                return Json(returnObj, JsonRequestBehavior.AllowGet);

            }

            goodModel.PV++;
            QiyeGoodsBLL.SingleModel.Update(goodModel, "PV");



            if (!string.IsNullOrEmpty(goodModel.Plabels))
            {
                //goodModel.plabelstr = DAL.Base.SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, $"SELECT group_concat(name order by sort desc) from entgoodlabel where id in ({goodModel.plabels})").ToString();
                goodModel.PlabelStr = QiyeGoodsLabelBLL.SingleModel.GetEntGoodsLabelStr(goodModel.Plabels);
                goodModel.PlabelStr_Arry = goodModel.PlabelStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }



            if (!string.IsNullOrEmpty(goodModel.Img))
            {
                goodModel.Img = goodModel.Img.Replace("http://vzan-img.oss-cn-hangzhou.aliyuncs.com", "https://i.vzan.cc/");
            }

            returnObj.isok = true;
            returnObj.dataObj = goodModel;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 员工相关
        /// <summary>
        /// 通过userId查询该用户是否绑定了名片
        /// 如果绑定了则到我的名片页否则到名片列表页面
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUserIsBind()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";

            int aid = Context.GetRequestInt("aid", 0);
            int userId = Context.GetRequestInt("userId", 0);
            string appId = Context.GetRequest("appId", string.Empty);

            if (userId <= 0 || aid <= 0 || string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }


            QiyeEmployee qiyeEmployee = QiyeEmployeeBLL.SingleModel.GetQiyeEmployeeByUserId(aid, userId, appId);
            if (qiyeEmployee != null)
            {
                returnObj.dataObj = qiyeEmployee.Id;
                returnObj.isok = true;
                returnObj.Msg = "此员工已绑定名片请跳转到名片个人名片中心";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            returnObj.Msg = "此员工未绑定名片请跳转到非员工名片列表进行绑定";
            return Json(returnObj, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 通过userId绑定员工码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BindWorkIDByUserId()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            int aid = Context.GetRequestInt("aid", 0);
            int userId = Context.GetRequestInt("userId", 0);
            string workId = Context.GetRequest("workId", string.Empty);
            string appId = Context.GetRequest("appId", string.Empty);
            if (userId <= 0 || string.IsNullOrEmpty(workId) || aid <= 0 || string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            QiyeEmployee qiyeEmployee = QiyeEmployeeBLL.SingleModel.GetQiyeEmployeeByWorkId(aid, workId, appId);
            if (qiyeEmployee == null)
            {
                returnObj.Msg = "员工码不存在";
                return Json(returnObj);
            }

            if (qiyeEmployee.UserId > 0)
            {
                returnObj.Msg = "该员工码已绑定员工";
                return Json(returnObj);
            }

            QiyeEmployee qiyeEmployeeUser = QiyeEmployeeBLL.SingleModel.GetQiyeEmployeeByUserId(aid, userId, appId);
            if (qiyeEmployeeUser != null)
            {
                returnObj.Msg = "此员工已绑定名片";
                return Json(returnObj);
            }


            qiyeEmployee.UserId = userId;
            qiyeEmployee.UpdateTime = DateTime.Now;
            if (QiyeEmployeeBLL.SingleModel.Update(qiyeEmployee, "UserId,UpdateTime"))
            {
                returnObj.dataObj = qiyeEmployee.Id;
                returnObj.isok = true;
                returnObj.Msg = "绑定成功";
                return Json(returnObj);
            }
            else
            {
                returnObj.Msg = "绑定失败";
                return Json(returnObj);
            }





        }

        /// <summary>
        /// 通过userId解除绑定名片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult JieBindWorkIDByUserId()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            int aid = Context.GetRequestInt("aid", 0);
            int userId = Context.GetRequestInt("userId", 0);
            string appId = Context.GetRequest("appId", string.Empty);
            if (userId <= 0 || aid <= 0 || string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            QiyeEmployee qiyeEmployee = QiyeEmployeeBLL.SingleModel.GetQiyeEmployeeByUserId(aid, userId, appId);
            if (qiyeEmployee == null)
            {
                returnObj.isok = true;
                returnObj.Msg = "解绑成功";
                return Json(returnObj);
            }

            qiyeEmployee.UserId = 0;
            qiyeEmployee.UpdateTime = DateTime.Now;
            if (QiyeEmployeeBLL.SingleModel.Update(qiyeEmployee, "UserId,UpdateTime"))
            {

                returnObj.isok = true;
                returnObj.Msg = "解绑成功";
                return Json(returnObj);
            }
            else
            {
                returnObj.Msg = "解绑失败";
                return Json(returnObj);
            }





        }

        /// <summary>
        /// 发动态
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PostMsg()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            int aid = Context.GetRequestInt("aid", 0);
            int userId = Context.GetRequestInt("userId", 0);
            string msgDetail = Context.GetRequest("msgDetail", string.Empty);
            string imgs = Context.GetRequest("imgs", string.Empty);
            if (userId <= 0 || aid <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            if (string.IsNullOrEmpty(msgDetail) && string.IsNullOrEmpty(imgs))
            {
                returnObj.Msg = "请输入动态信息";
                return Json(returnObj);
            }

            QiyePostMsg qiyePostMsg = new QiyePostMsg();
            qiyePostMsg.AddTime = DateTime.Now;
            qiyePostMsg.MsgDetail = msgDetail;
            qiyePostMsg.Imgs = imgs;
            qiyePostMsg.Aid = aid;
            qiyePostMsg.UserId = userId;
            int id = Convert.ToInt32(QiyePostMsgBLL.SingleModel.Add(qiyePostMsg));
            if (id > 0)
            {
                returnObj.isok = true;
                returnObj.Msg = "发布成功";
                return Json(returnObj);
            }
            else
            {
                returnObj.Msg = "发布失败";
                return Json(returnObj);
            }

        }

        /// <summary>
        /// 获取我的动态列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetListPostMsg()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            int aid = Context.GetRequestInt("aid", 0);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int userId = Context.GetRequestInt("userId", 0);
            if (userId <= 0 || aid <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            int totalCount = 0;
            List<QiyePostMsg> list = QiyePostMsgBLL.SingleModel.getListByaid(aid, userId, out totalCount, pageIndex, pageSize);
            returnObj.dataObj = new
            {
                totalCount = totalCount,
                list = list

            };
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除我的动态
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DelPostMsg()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            int aid = Context.GetRequestInt("aid", 0);
            int userId = Context.GetRequestInt("userId", 0);
            int id = Context.GetRequestInt("id", 0);
            if (userId <= 0 || aid <= 0 || id <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            QiyePostMsg qiyePostMsg = QiyePostMsgBLL.SingleModel.GetModel(id);
            if (qiyePostMsg == null)
            {
                returnObj.Msg = "数据不存在";
                return Json(returnObj);
            }
            if (qiyePostMsg.Aid != aid || qiyePostMsg.UserId != userId)
            {
                returnObj.Msg = "暂无权限";
                return Json(returnObj);
            }

            qiyePostMsg.State = -1;
            if (QiyePostMsgBLL.SingleModel.Update(qiyePostMsg, "State"))
            {
                returnObj.isok = true;
                returnObj.Msg = "删除成功";
                return Json(returnObj);
            }
            else
            {
                returnObj.Msg = "删除失败";
                return Json(returnObj);
            }

        }

        /// <summary>
        /// 获取名片信息(员工信息)
        /// </summary>
        /// <returns></returns>
        public ActionResult GetEmployee()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            int aid = Context.GetRequestInt("aid", 0);
            int userId = Context.GetRequestInt("userId", 0);
            int employeeId = Context.GetRequestInt("employeeId", 0);
            if (employeeId <= 0 || aid <= 0|| userId<=0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            QiyeEmployee qiyeEmployee = QiyeEmployeeBLL.SingleModel.GetModel(employeeId);
            if (qiyeEmployee == null || qiyeEmployee.State == -1 || qiyeEmployee.Aid != aid)
            {
                returnObj.Msg = "该名片不存在";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            Miniapp listTemp = MiniappBLL.SingleModel.GetModelByRelationId(aid);
            if (listTemp != null)
            {
                Moduls company = ModulsBLL.SingleModel.GetModelByAppidandLevel(listTemp.Id, (int)Miapp_Miniappmoduls_Level.ModelData);
                if (company != null)
                {
                    qiyeEmployee.CompanyAddress = company.Address;
                    qiyeEmployee.CompanyPhone = company.mobile;
                    qiyeEmployee.CompanyName = company.Name;
                    qiyeEmployee.Location = company.AddressPoint;
                }
                else
                {
                    qiyeEmployee.CompanyAddress = listTemp.Address;
                    qiyeEmployee.CompanyPhone = listTemp.Phone;
                    qiyeEmployee.CompanyName = listTemp.StoreName;
                    qiyeEmployee.Location = listTemp.Location;
                }
            }


            QiyeDepartment qiyeDepartment = QiyeDepartmentBLL.SingleModel.GetModel(qiyeEmployee.DepartmentId);
            if (qiyeDepartment != null)
            {
                qiyeEmployee.DepartMentName = qiyeDepartment.Name;
            }


            //员工名片 推荐商品 动态
            qiyeEmployee.listQiyeGoods = QiyeGoodsBLL.SingleModel.GetListGoods(aid, string.Empty, string.Empty, string.Empty, 4, 1, -1, "desc");
            int postMsgCount = 0;
            qiyeEmployee.listQiyePostMsg = QiyePostMsgBLL.SingleModel.getListByaid(aid, qiyeEmployee.UserId, out postMsgCount, 1, 4);

            //员工名片点赞 转发 分享
            QiyeMsgviewFavoriteShare qiyeMsgviewFavoriteShare = QiyeMsgviewFavoriteShareBLL.SingleModel.GetModelByMsgId(aid, qiyeEmployee.Id, (int)PointsDataType.名片);
            if (qiyeMsgviewFavoriteShare != null)
            {
                qiyeEmployee.DzCount = qiyeMsgviewFavoriteShare.DzCount;
                qiyeEmployee.ShareCount = qiyeMsgviewFavoriteShare.ShareCount;
                qiyeEmployee.PV = qiyeMsgviewFavoriteShare.ViewCount;
            }

            QiyeUserFavoriteMsg qiyeUserFavoriteMsg = QiyeUserFavoriteMsgBLL.SingleModel.GetUserFavoriteMsg(aid, qiyeEmployee.Id, userId, (int)PointsActionType.点赞, (int)PointsDataType.名片);
            if (qiyeUserFavoriteMsg != null && qiyeUserFavoriteMsg.State == 0)
            {
                //当前登录用户对该名片是否点赞过
                qiyeEmployee.IsDzed = true;
            }

            returnObj.dataObj = qiyeEmployee;
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 点赞、关注、浏览、私信、收藏 转发 事件记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddFavorite()
        {
            returnObj = new Return_Msg_APP();
            int actionType = Context.GetRequestInt("actiontype", (int)PointsActionType.点赞);
            int userId = Context.GetRequestInt("userid", 0);//当前登录用户
            int otherCardId = Context.GetRequestInt("othercardid", 0);//被操作对象的ID 例如 如果是名片 则是名片ID
            int aid = Context.GetRequestInt("aid", 0);
            int dataType = Context.GetRequestInt("datatype", (int)PointsDataType.名片);
            if (userId <= 0)
            {
                returnObj.Msg = "userid不能小于0";
                return Json(returnObj);
            }
            if (otherCardId <= 0)
            {
                returnObj.Msg = "名片ID不能小于0";
                return Json(returnObj);
            }
            if (aid <= 0)
            {
                returnObj.Msg = "aid不能小于0";
                return Json(returnObj);
            }

            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userinfo == null)
            {
                returnObj.Msg = "无效用户";
                return Json(returnObj);
            }

            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(userinfo.appId);
            if (xcxrelation == null)
            {
                returnObj.Msg = "模板过期";
                return Json(returnObj);
            }
            int curState = 0;
            lock (_lockobj)
            {
                switch (dataType)
                {
                    case (int)PointsDataType.名片:
                        if (actionType == (int)PointsActionType.看过 || actionType == (int)PointsActionType.扫码 || actionType == (int)PointsActionType.转发)
                        {
                            QiyeEmployee qiyeEmployee = QiyeEmployeeBLL.SingleModel.GetModel(otherCardId);
                            if (qiyeEmployee == null && qiyeEmployee.State == -1)
                            {
                                returnObj.Msg = "名片不存在";
                                return Json(returnObj);
                            }
                            if (qiyeEmployee.UserId == userId)
                            {
                                returnObj.isok = true;
                                returnObj.Msg = "屏蔽自己的浏览";
                                return Json(returnObj);
                            }
                        }
                        break;
                    case (int)PointsDataType.客户:
                        if (actionType == (int)PointsActionType.扫码 || actionType == (int)PointsActionType.转发)
                        {
                            QiyeEmployee qiyeEmployee = QiyeEmployeeBLL.SingleModel.GetModel(otherCardId);
                            if (qiyeEmployee == null && qiyeEmployee.State == -1)
                            {
                                returnObj.Msg = "名片不存在";
                                return Json(returnObj);
                            }
                            if (qiyeEmployee.UserId == userId)
                            {
                                returnObj.isok = true;
                                returnObj.Msg = "屏蔽自己的浏览";
                                return Json(returnObj);
                            }
                            //如果还没有建立客户关系则在这里建立
                            QiyeCustomer customer = QiyeCustomerBLL.SingleModel.GetModelByUserId(userId);
                            if (customer == null)
                            {
                                customer = new QiyeCustomer();
                                customer.UserId = userId;
                                customer.Aid = xcxrelation.Id;
                                customer.AddTime = DateTime.Now;
                                customer.State = 1;
                                customer.UpdateTime = DateTime.Now;
                                customer.StaffId = qiyeEmployee.Id;
                                customer.Id = Convert.ToInt32(QiyeCustomerBLL.SingleModel.Add(customer));
                            }

                            if (customer.Id <= 0)
                            {
                                returnObj.isok = false;
                                returnObj.Msg = "建立客户关系失败";
                                return Json(returnObj);
                            }

                            if (customer.StaffId <= 0)
                            {
                                customer.StaffId = qiyeEmployee.Id;
                                customer.UpdateTime = DateTime.Now;
                               if( !QiyeCustomerBLL.SingleModel.Update(customer, "StaffId,UpdateTime"))
                                {
                                    returnObj.isok = false;
                                    returnObj.Msg = "建立客户关系异常";
                                    return Json(returnObj);
                                }
                            }

                        }

                        break;
                }

                returnObj.Msg = QiyeEmployeeBLL.SingleModel.CommondFavoriteMsg(aid, otherCardId, userId, actionType, dataType, ref curState);
            }
            returnObj.isok = returnObj.Msg.ToString() == "";
            returnObj.dataObj = curState;


            return Json(returnObj);

        }

        /// <summary>
        /// 修改保存名片
        /// </summary>
        /// <param name="qiyeEmployee"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditEmployee()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            int aid = Context.GetRequestInt("aid", 0);
            int employeeId = Context.GetRequestInt("employeeId", 0);
            if (employeeId <= 0 || aid <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }



            QiyeEmployee qiyeEmployee = QiyeEmployeeBLL.SingleModel.GetModel(employeeId);
            if (qiyeEmployee == null || qiyeEmployee.State == -1 || qiyeEmployee.Aid != aid)
            {
                returnObj.Msg = "该名片不存在";
                return Json(returnObj);
            }

            string avatar = Context.GetRequest("avatar", string.Empty);
            string name = Context.GetRequest("name", string.Empty);
            string phone = Context.GetRequest("phone", string.Empty);
            string wxAccount = Context.GetRequest("wxAccount", string.Empty);
            if (string.IsNullOrEmpty(avatar) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
            {
                returnObj.Msg = "名字,手机号,头像为必填项";
                return Json(returnObj);
            }

            if (!Regex.IsMatch(phone, @"\d") || phone.Length > 11)
            {
                returnObj.Msg = "请填写正确的联系号码";
                return Json(returnObj);
            }


            qiyeEmployee.Avatar = avatar;
            qiyeEmployee.Name = name;
            qiyeEmployee.Phone = phone;
            qiyeEmployee.WxAccount = wxAccount;
            qiyeEmployee.UpdateTime = DateTime.Now;
            if (!QiyeEmployeeBLL.SingleModel.Update(qiyeEmployee, "Avatar,Name,Phone,WxAccount,UpdateTime"))
            {
                returnObj.Msg = "修改异常";
                return Json(returnObj);
            }

            returnObj.isok = true;
            returnObj.Msg = "修改成功";
            return Json(returnObj);
        }

        /// <summary>
        /// 获取我的名片码
        /// </summary>
        /// <returns></returns>
        public ActionResult GetEmployeeQrcode()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            int aid = Context.GetRequestInt("aid", 0);
            string appId = Context.GetRequest("appId", string.Empty);
            int employeeId = Context.GetRequestInt("employeeId", 0);
            if (employeeId <= 0 || aid <= 0 || string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            
            QiyeEmployee qiyeEmployee = QiyeEmployeeBLL.SingleModel.GetModel(employeeId);
            if (qiyeEmployee == null || qiyeEmployee.State == -1 || qiyeEmployee.Aid != aid)
            {
                returnObj.Msg = "该名片不存在";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            qrcodeclass qrcodemodel = new qrcodeclass();
            if (string.IsNullOrEmpty(qiyeEmployee.Qrcode))
            {
                XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModel(aid);
                if (xcxrelation == null)
                {
                    returnObj.Msg = "模板过期";
                    return Json(returnObj);
                }

                XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
                if (xcxTemplate == null)
                {
                    returnObj.Msg = "无效模板";
                    return Json(returnObj);
                }

                int count = 0;//重试次数
                bool ok = false;
                string scen = $"{employeeId}";
                while (!ok)
                {
                    count++;
                    
                    string token = "";
                    if (!XcxApiBLL.SingleModel.GetToken(xcxrelation, ref token))
                    {
                        returnObj.Msg = token;
                        return Json(returnObj);
                    }

                    qrcodemodel = CommondHelper.GetMiniAppQrcode(token,"pages/card/cardDlt", scen);
                    if (qrcodemodel != null && !string.IsNullOrEmpty(qrcodemodel.url))
                    {
                        qiyeEmployee.Qrcode = qrcodemodel.url;
                        qiyeEmployee.UpdateTime = DateTime.Now;
                        QiyeEmployeeBLL.SingleModel.Update(qiyeEmployee, "Qrcode,UpdateTime");

                        ok = true;
                    }
                    if (count > 3)
                    {
                        ok = true;
                    }

                }
            }


            if (string.IsNullOrEmpty(qiyeEmployee.Qrcode))
            {
                returnObj.dataObj = qrcodemodel;
                returnObj.Msg = "获取失败";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }


            returnObj.dataObj = qiyeEmployee.Qrcode;
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 数据雷达
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDataList()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            int aid = Context.GetRequestInt("aid", 0);
            string appId = Context.GetRequest("appId", string.Empty);
            int employeeId = Context.GetRequestInt("employeeId", 0);
            if (employeeId <= 0 || aid <= 0 || string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            QiyeEmployee qiyeEmployee = QiyeEmployeeBLL.SingleModel.GetModel(employeeId);
            if (qiyeEmployee == null || qiyeEmployee.State == -1 || qiyeEmployee.Aid != aid)
            {
                returnObj.Msg = "该名片不存在";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            //员工名片点赞 转发 分享
            DataList dataList = new DataList();
            QiyeMsgviewFavoriteShare qiyeMsgviewFavoriteShare = QiyeMsgviewFavoriteShareBLL.SingleModel.GetModelByMsgId(aid, qiyeEmployee.Id, (int)PointsDataType.名片);
            if (qiyeMsgviewFavoriteShare != null)
            {
                dataList.DzCount = qiyeMsgviewFavoriteShare.DzCount;
                dataList.ShareCount = qiyeMsgviewFavoriteShare.ShareCount;
                dataList.PV = qiyeMsgviewFavoriteShare.ViewCount;
            }
            dataList.MonthTotalCustomer = QiyeCustomerBLL.SingleModel.GetEmployeeCustomerCount(qiyeEmployee.Id, 1);
            dataList.TotalCustomer = QiyeCustomerBLL.SingleModel.GetEmployeeCustomerCount(qiyeEmployee.Id);


            returnObj.dataObj = dataList;
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 获取用户通过转发或者扫码浏览过的名片
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMyListEmployee()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            int aid = Context.GetRequestInt("aid", 0);
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            int pageIndex = Context.GetRequestInt("pageIndex", 0);
            int pageSize = Context.GetRequestInt("pageSize", 0);
            if (userId <= 0 || aid <= 0 || string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            int count = 0;
            List<QiyeEmployee> list = QiyeEmployeeBLL.SingleModel.GetMyListQiyeEmployee(aid, userId, out count, pageIndex, pageSize);
            QiyeCustomer customer = QiyeCustomerBLL.SingleModel.GetModelByUserId(userId);
            if (customer != null)
            {
                QiyeEmployee qiyeEmployee = QiyeEmployeeBLL.SingleModel.GetModel(customer.StaffId);
                if (qiyeEmployee!=null&& qiyeEmployee.State != -1)
                {
                    qiyeEmployee.Source = "来自客服";
                    list.Insert(0, qiyeEmployee);
                }
            }

            string departmentIds = string.Join(",",list?.Select(s=>s.DepartmentId));
            List<QiyeDepartment> qiyeDepartmentList = QiyeDepartmentBLL.SingleModel.GetListByIds(departmentIds);

            list?.ForEach(x =>
            {
                QiyeDepartment department = qiyeDepartmentList?.FirstOrDefault(f=>f.Id == x.DepartmentId);
                if (department != null)
                {
                    x.DepartMentName = department.Name;
                }

            });
            
            returnObj.dataObj = list;
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 客户绑定客服
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ActionResult CustomerBindEmployee(int userId = 0)
        {
            returnObj = new Return_Msg_APP();
            if (userId <= 0)
            {
                returnObj.Msg = "无效参数";
                return Json(returnObj);
            }
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userinfo == null)
            {
                returnObj.Msg = "无效用户";
                return Json(returnObj);
            }

            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(userinfo.appId);
            if (xcxrelation == null)
            {
                returnObj.Msg = "模板过期";
                return Json(returnObj);
            }

            QiyeCustomer customer = QiyeCustomerBLL.SingleModel.GetModelByUserId(userId);
            if (customer == null)
            {
                customer = new QiyeCustomer();
                customer.UserId = userId;
                customer.Aid = xcxrelation.Id;
                customer.AddTime = DateTime.Now;
                customer.State = 1;
                customer.UpdateTime = DateTime.Now;
                customer.Id = Convert.ToInt32(QiyeCustomerBLL.SingleModel.Add(customer));
            }

            if (customer.Id <= 0)
            {
                returnObj.Msg = "无效客户";
                return Json(returnObj);
            }

            if (customer.StaffId <= 0)
            {
                QiyeEmployee model = QiyeEmployeeBLL.SingleModel.GetKeFuModel(customer.Aid, xcxrelation.AppId);
                if (model != null)
                {
                    customer.StaffId = model.Id;
                    customer.UpdateTime = DateTime.Now;
                    QiyeCustomerBLL.SingleModel.Update(customer, "StaffId,UpdateTime");
                }
            }
            if(customer.StaffId>0)
            {
                QiyeEmployee model = QiyeEmployeeBLL.SingleModel.GetModel(customer.StaffId);
                returnObj.dataObj = model;
            }
            customer.PV += 1;
            QiyeCustomerBLL.SingleModel.Update(customer,"pv");

            returnObj.isok = true;
            return Json(returnObj);
        }

        /// <summary>
        /// 员工获取客户数据列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ActionResult GetCustomerList(int userId=0,int pageSize=10,int pageIndex=1,string name="")
        {
            returnObj = new Return_Msg_APP();
            if (userId <= 0)
            {
                returnObj.Msg = "无效参数";
                return Json(returnObj);
            }
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if(userinfo==null)
            {
                returnObj.Msg = "无效用户";
                return Json(returnObj);
            }
            QiyeEmployee model = QiyeEmployeeBLL.SingleModel.GetModelByUserId(userId);
            if(model==null)
            {
                returnObj.Msg = "无效员工";
                return Json(returnObj);
            }

            int count = 0;
            List<QiyeCustomer> list = QiyeCustomerBLL.SingleModel.GetDataListApi(userId,model.Id,pageSize,pageIndex,ref count,name, userinfo.appId);
            returnObj.dataObj = new { list = list, count = count };

            returnObj.isok = true;
            return Json(returnObj);
        }

        /// <summary>
        /// 客户与员工聊天信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ActionResult GetEmployeeMessage(int userId = 0, int pageSize = 10, int pageIndex = 1, string name = "")
        {
            returnObj = new Return_Msg_APP();
            if (userId <= 0)
            {
                returnObj.Msg = "无效参数";
                return Json(returnObj);
            }

            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userinfo == null)
            {
                returnObj.Msg = "无效用户";
                return Json(returnObj);
            }

            int count = 0;
            //List<ImMessage> list = _imMessageBLL.GetListByTuserId(userId,pageSize,pageIndex,ref count,name);

            List<ImMessage> list = ImContactBLL.SingleModel.GetListByQiye(userId,userinfo.appId, pageSize, pageIndex, ref count, name);
            returnObj.dataObj = new { list = list, count = count };

            returnObj.isok = true;
            return Json(returnObj);
        }

        /// <summary>
        /// 修改客户备注
        /// </summary>
        /// <param name="id"></param>
        /// <param name="desc"></param>
        /// <param name="phoneDesc"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditeCustomerDesc(int id=0,string desc="",string phoneDesc="")
        {
            returnObj = new Return_Msg_APP();
            if (id <= 0)
            {
                returnObj.Msg = "无效参数";
                return Json(returnObj);
            }
            QiyeCustomer model = QiyeCustomerBLL.SingleModel.GetModel(id);
            if(model==null)
            {
                returnObj.Msg = "无效客户";
                return Json(returnObj);
            }
            model.Desc = desc;
            model.PhoneDesc = phoneDesc;
            model.UpdateTime = DateTime.Now;
            returnObj.isok = QiyeCustomerBLL.SingleModel.Update(model, "desc,PhoneDesc,UpdateTime");
            returnObj.Msg = returnObj.isok ? "保存成功" : "保存失败";

            return Json(returnObj);
        }

        /// <summary>
        /// 获取店铺信息
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public ActionResult GetStoreInfo(int aid=0)
        {
            returnObj = new Return_Msg_APP();
            if (aid<=0)
            {
                returnObj.Msg = "无效参数";
                return Json(returnObj);
            }

            QiyeStore store = QiyeStoreBLL.SingleModel.GetModelByAId(aid);
            returnObj.dataObj = store;
            returnObj.isok = true;
            returnObj.Msg = "成功";

            return Json(returnObj);
        }
        #endregion

        #region 订单相关
        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="attrSpacStr"></param>
        /// <param name="specInfo">商品规格(格式)：规格1：属性1 规格2：属性2 如:（颜色：白色 尺码：M）</param>
        /// <param name="specImg"></param>
        /// <param name="goodId"></param>
        /// <param name="userId"></param>
        /// <param name="qty"></param>
        /// <param name="gotoBuy">立即购买，1：立即购买，0：添加到购物车</param>
        /// <returns></returns>
        public ActionResult AddGoodsCarData(string attrSpacStr = "", string specInfo = "", string specImg = "", int goodId = 0, int userId = 0, int qty = 0, int gotoBuy = 0)
        {
            returnObj = new Return_Msg_APP();

            if (qty <= 0)
            {
                returnObj.Msg = "数量必须大于0";
                return Json(returnObj);
            }

            QiyeGoods good = QiyeGoodsBLL.SingleModel.GetModel(goodId);
            if (good == null)
            {
                returnObj.Msg = "未找到该商品";
                return Json(returnObj);
            }
            QiyeStore store = QiyeStoreBLL.SingleModel.GetModelByAId(good.AId);
            if (store == null)
            {
                returnObj.Msg = "店铺已关闭";
                return Json(returnObj);
            }
            if (!string.IsNullOrWhiteSpace(attrSpacStr))
            {
                log4net.LogHelper.WriteInfo(this.GetType(), $"{JsonConvert.SerializeObject(good.GASDetailList)},{attrSpacStr}");
                if (!good.GASDetailList.Any(x => x.Id.Equals(attrSpacStr)))
                {
                    returnObj.Msg = "商品已过期";
                    return Json(returnObj);
                }
            }
            if (!(good.State == 1 && good.Tag == 1))
            {
                returnObj.Msg = "无法添加失效商品";
                return Json(returnObj);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                returnObj.Msg = "用户不存在";
                return Json(returnObj);
            }

            QiyeGoodsCart car = QiyeGoodsCartBLL.SingleModel.GetModelBySpec(userInfo.Id, goodId, attrSpacStr, 0);
            //商品价格
            int price = Convert.ToInt32(good.Price * 100);
            price = Convert.ToInt32(!string.IsNullOrWhiteSpace(attrSpacStr) ? good.GASDetailList.First(x => x.Id.Equals(attrSpacStr)).Price * 100 : good.Price * 100);

            if (car == null || gotoBuy == 1)
            {
                car = new QiyeGoodsCart
                {
                    StoreId = store.Id,
                    GoodsName = good.Name,
                    GoodsId = good.Id,
                    SpecIds = attrSpacStr,
                    Count = qty,
                    Price = price,
                    SpecInfo = specInfo,
                    SpecImg = specImg,//规格图片
                    UserId = userInfo.Id,
                    AddTime = DateTime.Now,
                    State = 0,
                    GoToBuy = gotoBuy,
                    AId = good.AId,
                };

                //加入购物车
                int id = Convert.ToInt32(QiyeGoodsCartBLL.SingleModel.Add(car));
                if (id > 0)
                {
                    int cartcount = QiyeGoodsCartBLL.SingleModel.GetCartGoodsCountByUserId(userInfo.Id);
                    returnObj.Msg = "成功";
                    returnObj.dataObj = new { id = id, count = cartcount };
                    returnObj.isok = true;
                    return Json(returnObj);
                }
            }
            else
            {
                car.Count += qty;
                if (QiyeGoodsCartBLL.SingleModel.Update(car, "Count"))
                {
                    int cartcount = QiyeGoodsCartBLL.SingleModel.GetCartGoodsCountByUserId(userInfo.Id);
                    returnObj.dataObj = new { id = car.Id, count = cartcount };
                    returnObj.Msg = "成功";
                    returnObj.isok = true;
                    return Json(returnObj);
                }
            }

            returnObj.Msg = "失败";
            return Json(returnObj);
        }

        /// <summary>
        /// 购物车列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ActionResult GetGoodsCarList(int pageSize = 10, int pageIndex = 1, int userId = 0)
        {
            returnObj = new Return_Msg_APP();
            try
            {
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
                if (userInfo == null)
                {
                    returnObj.Msg = "用户不存在";
                    return Json(returnObj);
                }

                XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(userInfo.appId);
                if (xcxrelation == null)
                {
                    returnObj.Msg = "未绑定小程序或模板已过期";
                    return Json(returnObj);
                }

                int count = 0;
                List<QiyeGoodsCart> carList = QiyeGoodsCartBLL.SingleModel.GetListByUserId(xcxrelation.Id, userInfo.Id, pageSize, pageIndex, ref count);

                if (carList == null || carList.Count <= 0)
                {
                    returnObj.Msg = "购物车为空";
                    return Json(returnObj);
                }

                //获取会员信息
                VipRelation vipInfo = VipRelationBLL.SingleModel.GetModelByUserid(userInfo.Id);
                VipLevel levelInfo = vipInfo != null ? VipLevelBLL.SingleModel.GetModel(vipInfo.levelid) : null;

                //#region 会员打折
                carList.ForEach(g => g.OriginalPrice = g.Price);
                VipLevelBLL.SingleModel.GetVipDiscount(ref carList, vipInfo, levelInfo, userInfo.Id, "Discount", "Price");
                //#endregion 会员打折

                //获取商品详细资料
                List<QiyeGoods> goods = new List<QiyeGoods>();
                goods = QiyeGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", carList.Select(x => x.GoodsId))}) ");

                if (goods == null || goods.Count <= 0)
                {
                    returnObj.Msg = "商品已过期";
                    returnObj.dataObj = carList;
                    return Json(returnObj);
                }

                QiyeGoods curGood = new QiyeGoods();
                carList.ForEach(c =>
                {
                    curGood = goods.FirstOrDefault(g => g.Id == c.GoodsId);
                    if (curGood != null && curGood.Id > 0)
                    {
                        //多规格处理
                        if (curGood.GASDetailList != null && curGood.GASDetailList.Count > 0)
                        {
                            List<GoodsSpecDetail> detaillist = curGood.GASDetailList.ToList();
                            detaillist?.ForEach(g =>
                            {
                                g.OriginalPrice = g.Price;
                                g.Discount = c.Discount;
                                float discountPrice = g.Price * (c.Discount * 0.01F);
                                g.DiscountPrice = discountPrice < 0.01 ? 0.01F : discountPrice;
                            });
                            curGood.SpecDetail = JsonConvert.SerializeObject(detaillist);
                        }
                        curGood.Description = string.Empty;
                        c.GoodsInfo = curGood;
                    }
                });

                returnObj.dataObj = new { carList = carList, count = count, goodsCount = carList.Sum(s => s.Count) };
                returnObj.isok = true;
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                returnObj.dataObj = ex;
                return Json(returnObj);
            }
        }

        /// <summary>
        /// 修改或删除购物车商品
        /// </summary>
        /// <param name="cartStr"></param>
        /// <param name="userId"></param>
        /// <param name="type">0为更新,-1为删除</param>
        /// <returns></returns>
        public ActionResult UpdateOrDeleteGoodsCarData(string cartStr = "", int userId = 0, int type = 0)
        {
            returnObj = new Return_Msg_APP();
            if (string.IsNullOrEmpty(cartStr))
            {
                returnObj.Msg = "数据不能为空";
                return Json(returnObj);
            }
            List<QiyeGoodsCart> goodsCarModel = JsonConvert.DeserializeObject<List<QiyeGoodsCart>>(cartStr);
            if (userId <= 0)
            {
                returnObj.Msg = "userid不能为空";
                return Json(returnObj);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                returnObj.Msg = "找不到用户";
                return Json(returnObj);
            }
            if (goodsCarModel == null || goodsCarModel.Count <= 0)
            {
                returnObj.Msg = "找不到购物车数据";
                return Json(returnObj);
            }

            string column = "";
            foreach (QiyeGoodsCart item in goodsCarModel)
            {
                column = "State";
                if (type == -1)
                {
                    item.State = -1;
                }
                else if (type == 0)//根据传入参数更新购物车内容
                {
                    //价格因更改规格随之改变
                    QiyeGoods carGoods = QiyeGoodsBLL.SingleModel.GetModel(item.GoodsId);
                    if (carGoods == null)
                    {
                        item.State = 2;
                    }
                    else
                    {
                        column += ",Count";
                        if (!string.IsNullOrWhiteSpace(carGoods.SpecDetail))
                        {
                            float? price = carGoods.GASDetailList.Where(x => x.Id.Equals(item.SpecIds))?.FirstOrDefault()?.Price;
                            if (price != null)
                            {
                                column += ",Price";
                                item.Price = Convert.ToInt32(price * 100);
                            }
                        }
                    }
                }

                bool success = QiyeGoodsCartBLL.SingleModel.Update(item, column);
                if (!success)
                {
                    returnObj.Msg = "失败";
                    return Json(returnObj);
                }
            }
            returnObj.Msg = "操作成功";
            returnObj.isok = true;
            return Json(returnObj);
        }

        /// <summary>
        /// 获取运费信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetFreightFee(string appId = "", int userId = 0, int storeId = 0, string province = "", string city = "", string goodCartIds = "")
        {
            returnObj = new Return_Msg_APP();

            if (userId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            if (string.IsNullOrWhiteSpace(goodCartIds))
            {
                returnObj.Msg = "购物车参数出错";
                return Json(returnObj);
            }

            C_UserInfo usrInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (usrInfo == null)
            {
                returnObj.Msg = "无效用户";
                return Json(returnObj);
            }

            XcxAppAccountRelation model = _xcxAppAccountRelationBLL.GetModelByAppid(usrInfo.appId);
            if (model == null)
            {
                returnObj.Msg = "无效模板";
                return Json(returnObj);
            }

            QiyeStore store = QiyeStoreBLL.SingleModel.GetModelByAId(model.Id);
            if (store == null)
            {
                returnObj.Msg = "没有找到店铺";
                return Json(returnObj);
            }

            string errorMsg = "";
            DeliveryFeeResult deliueryResult = DeliveryTemplateBLL.SingleModel.GetQiyeFee(goodCartIds, store.Aid, province, city, ref errorMsg);
            if (errorMsg.Length > 0)
            {
                returnObj.Msg = errorMsg;
                return Json(returnObj);
            }

            returnObj.Msg = "获取成功";
            returnObj.isok = true;
            returnObj.dataObj = new { deliueryResult = deliueryResult, storeaddress = store.Location };
            return Json(returnObj);
        }

        /// <summary>
        /// 获取订单信息列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="state"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetOrderList(int userId = 0, int state = 0, int pageIndex = 1, int pageSize = 10)
        {
            returnObj = new Return_Msg_APP();
            if (userId <= 0)
            {
                returnObj.Msg = "用户ID不能为0";
                return Json(returnObj);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                returnObj.Msg = "找不到用户";
                return Json(returnObj);
            }

            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(userInfo.appId);
            if (xcxrelation == null)
            {
                returnObj.Msg = "无效模板";
                return Json(returnObj);
            }

            int count = 0;
            List<QiyeGoodsOrder> list = QiyeGoodsOrderBLL.SingleModel.GetList_Api(state, xcxrelation.Id, userId, pageSize, pageIndex, ref count);

            returnObj.isok = true;
            returnObj.dataObj = new { count = count, list = list };
            return Json(returnObj);
        }

        /// <summary>
        /// 获取订单信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetOrderInfo(int userId = 0, int id = 0)
        {
            returnObj = new Return_Msg_APP();
            if (userId <= 0)
            {
                returnObj.Msg = "用户ID不能为0";
                return Json(returnObj);
            }

            QiyeGoodsOrder model = QiyeGoodsOrderBLL.SingleModel.GetModel_Api(id);
            if (model != null)
            {
                QiyeStore store = QiyeStoreBLL.SingleModel.GetModel(model.StoreId);
                if (store != null)
                {
                    model.StorePhone = store.Phone;
                }
                model.DeliveryInfo = DeliveryFeedbackBLL.SingleModel.GetOrderFeed(orderId: model.Id, orderType: DeliveryOrderType.独立小程序订单商家发货);
            }

            returnObj.dataObj = model;
            returnObj.isok = true;
            return Json(returnObj);
        }

        /// <summary>
        /// 修改订单状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult UpdateOrderState(int id = 0, int userId = 0, int state = 0)
        {
            returnObj = new Return_Msg_APP();
            if (id <= 0)
            {
                returnObj.Msg = "订单ID不能为空";
                return Json(returnObj);
            }
            QiyeGoodsOrder order = QiyeGoodsOrderBLL.SingleModel.GetModel(id);
            if (order == null)
            {
                returnObj.Msg = "订单已失效";
                return Json(returnObj);
            }
            if (order.UserId != userId)
            {
                returnObj.Msg = "无效用户";
                return Json(returnObj);
            }

            string msg = "";
            switch (state)
            {
                case (int)QiyeOrderState.已取消:
                    if (order.State != (int)QiyeOrderState.待付款)
                    {
                        returnObj.Msg = "取消订单：无效订单状态";
                        return Json(returnObj);
                    }
                    QiyeGoodsOrderBLL.SingleModel.CancelOrder(order, ref msg);
                    if (msg.Length > 0)
                    {
                        returnObj.Msg = msg;
                        return Json(returnObj);
                    }
                    break;
                case (int)QiyeOrderState.已完成:
                    if (order.State != (int)QiyeOrderState.待收货)
                    {
                        returnObj.Msg = "确认收货：无效订单状态";
                        return Json(returnObj);
                    }
                    QiyeGoodsOrderBLL.SingleModel.ReceiptGoods(order, ref msg);
                    if (msg.Length > 0)
                    {
                        returnObj.Msg = msg;
                        return Json(returnObj);
                    }
                    break;
                default:
                    returnObj.Msg = "无效订单状态";
                    return Json(returnObj);
            }

            returnObj.Msg = "操作成功";
            returnObj.isok = true;
            return Json(returnObj);
        }
        #endregion
    }
}