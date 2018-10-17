using BLL.MiniApp;
using BLL.MiniApp.Conf;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Web.Mvc;
using System.Web.UI;
using User.MiniApp.Comment;
using User.MiniApp.Model;
using Utility;
using User.MiniApp.Filters;

namespace User.MiniApp.Controllers
{
    public class MiappOfficialWebController: officialwebController
    {

    }

    public class officialwebController : baseController
    {

        

        /// <summary>
        /// 实例化对象
        /// </summary>
        public officialwebController()
        {
            
        }

        [LoginFilter]
        public ActionResult Config(int Id)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            if (Id <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            ViewBag.rappId = Id;

            //小程序广告图
            ViewBag.shouquan = 0;
            ViewBag.ImgUrlList = new List<object>();
            ViewBag.ImgUrl = "";
            ViewBag.XcxName = "";
            ViewBag.shouquan = 0;

            List<OpenAuthorizerConfig> umodel = OpenAuthorizerConfigBLL.SingleModel.GetListByaccoundidAndRid(dzaccount.Id.ToString(), Id);
            if (umodel != null && umodel.Count > 0)
            {
                ViewBag.shouquan = 1;
                List<ConfParam> paramslist = ConfParamBLL.SingleModel.GetListByRId(Id);
                //var paramslist = _miniappparamBll.GetModelByappids("'" + umodel[0].appid + "'");


                if (paramslist != null && paramslist.Count > 0)
                {
                    List<object> imgurl = new List<object>();
                    ConfParam imginfo = paramslist.Where(w => w.Param == "img").FirstOrDefault();
                    if (imginfo != null)
                    {
                        ViewBag.ImgUrl = imginfo.Value;
                        imgurl.Add(new { id = imginfo.Id, url = imginfo.Value });
                    }

                    ConfParam cinfo = paramslist.Where(w => w.Param == "nparam").FirstOrDefault();
                    if (cinfo != null)
                    {
                        ViewBag.XcxName = cinfo.Value;
                    }

                    ViewBag.ImgUrlList = imgurl;
                }
            }
            
            return View();
        }
        [HttpPost]
        public ActionResult SaveConfig(int id, string datajson)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModel(id);
            if (umodel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }

            List<ConfParam> paramslist = ConfParamBLL.SingleModel.GetListByRId(id);
            try
            {
                if (paramslist != null && paramslist.Count > 0)
                {
                    List<ConfParam> data = JsonConvert.DeserializeObject<List<ConfParam>>(datajson);
                    if (data != null && data.Count > 0)
                    {
                        foreach (ConfParam item in data)
                        {
                            ConfParam list = paramslist.FirstOrDefault(f => f.Param == item.Param);
                            if (list != null)
                            {
                                if (ConfParamBLL.SingleModel.UpdateList(id, item.Value, item.Param, umodel.AppId) <= 0)
                                {
                                    return Json(new { isok = -1, msg = "修改失败_" + item.Param }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                ConfParam model = new ConfParam();
                                model.AppId = umodel.AppId;
                                model.Param = item.Param;
                                model.Value = item.Value;
                                model.State = 0;
                                model.UpdateTime = DateTime.Now;
                                model.AddTime = DateTime.Now;
                                model.RId = id;
                                if (Convert.ToInt32(ConfParamBLL.SingleModel.Add(model)) <= 0)
                                {
                                    return Json(new { isok = -1, msg = "添加失败" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }
                }
                else
                {
                    List<ConfParam> data = JsonConvert.DeserializeObject<List<ConfParam>>(datajson);
                    if (data != null && data.Count > 0)
                    {
                        foreach (ConfParam item in data)
                        {
                            ConfParam model = new ConfParam();
                            model.AppId = umodel.AppId;
                            model.Param = item.Param;
                            model.Value = item.Value;
                            model.State = 0;
                            model.UpdateTime = DateTime.Now;
                            model.AddTime = DateTime.Now;
                            model.RId = id;
                            if (Convert.ToInt32(ConfParamBLL.SingleModel.Add(model)) <= 0)
                            {
                                return Json(new { isok = -1, msg = "添加失败" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { isok = -1, msg = "系统繁忙" + ex.Message }, JsonRequestBehavior.AllowGet);
            }


            return Json(new { isok = 1, msg = "保存成功" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获得根据Id模块信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult GetMiappModelData(int Id=0, int pageIndex=1, int pageSize=10)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            try
            {
                if (Id != 0)
                {
                    List<Miniapp> model = MiniappBLL.SingleModel.GetList("Id = " + Id + " and State = 1 and OpenId='" + dzaccount.OpenId + "'", pageSize, pageIndex);
                    return Json(new { data = model, code = 1, msg = "操作成功" });
                }
                else
                {
                    List<Miniapp> model = MiniappBLL.SingleModel.GetList("State = 1 and OpenId='" + dzaccount.OpenId + "'", pageSize, pageIndex);
                    int count = MiniappBLL.SingleModel.GetCount("State = 1 and OpenId='" + dzaccount.OpenId + "'");
                    return Json(new { data = model, code = 1, count = count, msg = "操作成功" });
                }
                //var model = _miniappBll.GetModel("Id = " + Id + " and State = 1");

            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return Json(new { data = new Miniapp(), code = 0, msg = "操作异常" });
            }
        }

        /// <summary>
        /// 编辑模块
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        public ActionResult AddOrEditMiniapp(Miniapp infon)
        {
            try
            {
                if (dzaccount == null)
                {
                    return Redirect("/dzhome/login");
                }
                //添加
                if (infon.Id == 0)
                {
                    infon.OpenId = dzaccount.OpenId;
                    infon.CreateDate = DateTime.Now;
                    object id = MiniappBLL.SingleModel.Add(infon);
                    if (int.Parse(id.ToString()) <= 0)
                    {
                        return Json(new { code = 0, msg = "添加失败" });
                    }
                    return Json(new { code = 1, msg = "添加成功" });
                }
                else
                {
                    //删除
                    if (infon.State == 0)
                    {
                        Miniapp model = MiniappBLL.SingleModel.GetModel(infon.Id);
                        model.State = infon.State;
                        if (!MiniappBLL.SingleModel.Update(model))
                        {
                            return Json(new { code = 0, msg = "删除失败" });
                        }
                        return Json(new { code = 1, msg = "删除成功" });
                    }
                    else
                    {
                        //修改
                        string cloumns = "StoreName,Linkurl,ImgUrl,Description,MenuLink,ModelId";
                        if (!MiniappBLL.SingleModel.Update(infon, cloumns))
                        {
                            return Json(new { code = 0, msg = "修改失败" });
                        }
                        return Json(new { code = 1, msg = "修改成功" });
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return Json(new { code = 0, msg = "操作异常" });
            }
        }

        [LoginFilter]
        public ActionResult Index(int Id, string storename, int Level)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            if (Id <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }

            Miniapp listtemp = MiniappBLL.SingleModel.GetModelByRelationId(Id);

            if (listtemp != null)
            {
                //获得隐藏模板Level
                ViewBag.hidden = listtemp.hiddenModel;
            }
            else
            {
                XcxAppAccountRelation xrelationmodel = XcxAppAccountRelationBLL.SingleModel.GetModel(Id);
                if (xrelationmodel == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
                }
                listtemp = new Miniapp();
                listtemp.CreateDate = DateTime.Now;
                listtemp.Description = "官网小程序";
                listtemp.OpenId = dzaccount.OpenId;
                listtemp.xcxRelationId = Id;
                listtemp.State = 1;
                listtemp.ModelId = xrelationmodel.AppId;
                listtemp.Id = Convert.ToInt32(MiniappBLL.SingleModel.Add(listtemp));
                if (listtemp.Id <= 0)
                {
                    return View("PageError", new Return_Msg() { Msg = "添加出错!", code = "500" });
                }
                ViewBag.hidden = "";
            }

            ViewBag.Title = storename;
            ViewBag.appId = listtemp.Id;
            ViewBag.rappId = Id;
            ViewBag.Level = Level;
            ViewBag.NewmodelId = 0;
            ViewBag.ImgUrlList = new List<object>();

            string sql = "appId = " + listtemp.Id + " and Level = " + Level + " and State = 1";
            List<Moduls> model = ModulsBLL.SingleModel.GetList(sql);


            ViewBag.ImgUrlList = new List<object>();
            if (model != null && model.Count > 0)
            {
                List<C_Attachment> imgs = C_AttachmentBLL.SingleModel.GetListByCache(model[0].Id, (int)AttachmentItemType.小程序官网首页轮播图, true);
                if (imgs != null && imgs.Count > 0)
                {
                    List<object> imgurl = new List<object>();
                    foreach (C_Attachment item in imgs)
                    {
                        imgurl.Add(new { id = item.id, url = item.filepath });
                    }
                    ViewBag.ImgUrlList = imgurl;
                }

                return View(model[0]);
            }

            return View(new Moduls() { appId = listtemp.Id, Level = Level });
        }

        public ActionResult ProductShow(int Id, string storename, int Level)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            if (Id <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数出错!", code = "500" });
            }

            ViewBag.Title = storename;
            ViewBag.appId = Id;
            ViewBag.Level = Level;
            ViewBag.NewmodelId = 0;
            ViewBag.ImgUrlList = new List<object>();

            return View(new Moduls() { Level = Level });
        }

        public ActionResult Development(int Id, string storename, int Level)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            if (Id <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数出错!", code = "500" });
            }

            ViewBag.Title = storename;
            ViewBag.appId = Id;
            ViewBag.Level = Level;
            ViewBag.NewmodelId = 0;
            ViewBag.ImgUrlList = new List<object>();

            return View(new Moduls() { Level = Level });
        }

        /// <summary>
        /// 初始化模块
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="storename"></param>
        /// <param name="Level"></param>
        /// <returns></returns>
        /// 
        public ActionResult ModelData(int Id=0, string storename="", int Level=0, int pageindex = 1, int pagesize = 10)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            if (Id <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数出错!", code = "500" });
            }

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(Id, dzaccount.Id.ToString());
            if (role == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }

            Miniapp listtemp = MiniappBLL.SingleModel.GetModelByRelationId(Id);

            if (listtemp != null)
            {
                //获得隐藏模板Level
                ViewBag.hidden = listtemp.hiddenModel;
            }
            else
            {
                XcxAppAccountRelation xrelationmodel = XcxAppAccountRelationBLL.SingleModel.GetModel(Id);
                if (xrelationmodel == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
                }
                listtemp = new Miniapp();
                listtemp.CreateDate = DateTime.Now;
                listtemp.Description = "官网小程序";
                listtemp.OpenId = dzaccount.OpenId;
                listtemp.xcxRelationId = Id;
                listtemp.State = 1;
                listtemp.ModelId = xrelationmodel.AppId;
                listtemp.Id = Convert.ToInt32(MiniappBLL.SingleModel.Add(listtemp));
                if (listtemp.Id <= 0)
                {
                    return View("PageError", new Return_Msg() { Msg = "添加繁忙!", code = "500" });
                }
                ViewBag.hidden = "";
            }

            ViewBag.Title = storename;
            ViewBag.appId = listtemp.Id;
            ViewBag.rappId = Id;
            ViewBag.Level = Level;
            ViewBag.NewmodelId = 0;

            string sql = "appId = " + listtemp.Id + " and Level = " + Level + " and State = 1";
            List<Moduls> model = ModulsBLL.SingleModel.GetList(sql);
            string viewname = "";

            switch (Level)
            {
                case (int)Miapp_Miniappmoduls_Level.ModelData: viewname = "Index";
                    break;
                case (int)Miapp_Miniappmoduls_Level.FirstModel: viewname = "FirstModel";
                    break;
                case (int)Miapp_Miniappmoduls_Level.TwoModel: viewname = "TwoModel";
                    break;
                case (int)Miapp_Miniappmoduls_Level.ThreeModel: viewname = "ThreeModel"; break;
                case (int)Miapp_Miniappmoduls_Level.FourModel:
                    viewname = "ProductShow";

                    List<Moduls> productlist = ModulsBLL.SingleModel.GetListByAppidandLevel(listtemp.Id, Level, pageindex, pagesize);
                    ViewBag.pageSize = 10;
                    ViewBag.TotalCount = ModulsBLL.SingleModel.GetListByAppidandLevelCount(listtemp.Id, Level);
                    return View(viewname, productlist);
                     
                case (int)Miapp_Miniappmoduls_Level.FiveModel:
                    Moduls miniappmodel = new Moduls();
                    if (model == null || model.Count<=0)
                    {
                        //添加动态新闻背景色
                        miniappmodel = new Moduls();
                        miniappmodel.appId = listtemp.Id;
                        miniappmodel.Level = (int)Miapp_Miniappmoduls_Level.FiveModel;
                        miniappmodel.State = 1;
                        miniappmodel.Createdate = DateTime.Now;
                        miniappmodel.Lastdate = DateTime.Now;
                        miniappmodel.Title = "发展历程";
                        miniappmodel.Id=Convert.ToInt32(ModulsBLL.SingleModel.Add(miniappmodel));
                    }
                    else
                    {
                        miniappmodel = model[0];
                    }
                    viewname = "Development";
                    List<Development> datass = DevelopmentBLL.SingleModel.GetListByAppid(listtemp.Id, pageindex, pagesize);
                    ViewBag.pageSize = 10;
                    ViewBag.dTitle = miniappmodel.Title;
                    ViewBag.did = miniappmodel.Id;
                    ViewBag.TotalCount = DevelopmentBLL.SingleModel.GetListByAppidCount(listtemp.Id);
                    return View(viewname, datass);
                    
                case (int)Miapp_Miniappmoduls_Level.SixModel: viewname = "CallWe";
                    break;
                case (int)Miapp_Miniappmoduls_Level.EightModel:
                    viewname = "CompanyNews";
                    List<Moduls> companynewslist = ModulsBLL.SingleModel.GetListByAppidandLevel(listtemp.Id, Level, pageindex, pagesize);
                    ViewBag.pageSize = 10;
                    ViewBag.TotalCount = ModulsBLL.SingleModel.GetListByAppidandLevelCount(listtemp.Id, Level);
                    return View(viewname, companynewslist);
                   
                default:
                    viewname = "ModelData";
                    break;
            }

            if (model != null && model.Count > 0)
            {
                return View(viewname, model[0]);
            }

            return View(viewname, new Moduls() { appId = listtemp.Id, Level = Level });
        }

        public ActionResult AddOrEditDevelopment(int appid, int id, int Level)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            Miniapp listtemp = MiniappBLL.SingleModel.GetModelByRelationId(appid);

            if (listtemp == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有找到模板!", code = "500" });
            }
            ViewBag.appId = listtemp.Id;
            ViewBag.rappId = appid;
            ViewBag.Level = Level;
            ViewBag.NewmodelId = 0;

            if (id <= 0)
            {
                return View(new Development());
            }
            else
            {
                Development model = DevelopmentBLL.SingleModel.GetModel(id);
                return View(model);
            }
        }

        public ActionResult AddOrEditCompanyNews(int appid, int id, int Level)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            Miniapp listtemp = MiniappBLL.SingleModel.GetModelByRelationId(appid);

            if (listtemp == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有找到模板!", code = "500" });
            }
            if (Level == 8)
            {
                ViewBag.Title = "编辑企业动态";
            }
            else
            {
                ViewBag.Title = "编辑产品展示";
            }
            ViewBag.appId = listtemp.Id;
            ViewBag.rappId = appid;
            ViewBag.Level = Level;
            ViewBag.NewmodelId = 0;

            if (id <= 0)
            {
                return View(new Moduls());
            }
            else
            {
                Moduls model = ModulsBLL.SingleModel.GetModel(id);
                ViewBag.xcxImgUrlList = new List<object>();
                if (model != null)
                {
                    ViewBag.xcxImgUrlList = new List<object>() { new { id = model.Id, url = model.ImgUrl } };
                }
                return View(model);
            }
        }
        
        /// <summary>
        /// 添加或编辑模块数据
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddOrEdit(Moduls infos, bool isdecode = false)
        {
            ServiceResult result = new ServiceResult();

            try
            {
                if (dzaccount == null)
                {
                    return Redirect("/dzhome/login");
                }
                if (infos == null)
                {
                    return Json(result.IsFailed("系统繁忙auth_limited !"));
                }

                #region Base64解密
                if (isdecode)
                {
                    string decode;
                    try
                    {
                        string strDescription = infos.Content.Replace(" ", "+");
                        byte[] bytes = Convert.FromBase64String(strDescription);
                        decode = Encoding.UTF8.GetString(bytes);
                    }
                    catch
                    {
                        decode = infos.Content;
                    }

                    infos.Content = decode;
                    string content = StringHelper.NoHtml(infos.Content.Replace("&nbsp;", ""));
                }

                #endregion
                if (infos.Id == 0)
                {
                    infos.State = 1;
                    infos.Createdate = DateTime.Now;
                    infos.Lastdate = DateTime.Now;
                    object id = ModulsBLL.SingleModel.Add(infos);
                    infos.Id = int.Parse(id.ToString());
                    result.IsSucceed("添加成功 !");

                    result.Data = new Dictionary<string, object> { { "datas", infos } };
                }
                else
                {
                    //判断是否为动态新闻模块
                    if ((int)Miapp_Miniappmoduls_Level.EightModel == infos.Level)
                    {
                        //获取新闻动态模板背景色
                        Moduls model2 = ModulsBLL.SingleModel.GetModel("appId=" + infos.appId + " and State = 1 and Level=" + (int)Miapp_Miniappmoduls_Level.NightModel);
                        if (model2 != null)
                        {
                            //修改动态新闻背景色
                            model2.Color = infos.Color;
                            ModulsBLL.SingleModel.Update(model2, "Color");
                        }
                        else
                        {
                            //添加动态新闻背景色
                            model2 = new Moduls();
                            model2.appId = infos.appId;
                            model2.Level = (int)Miapp_Miniappmoduls_Level.NightModel;
                            model2.State = 1;
                            model2.Createdate = DateTime.Now;
                            model2.Lastdate = DateTime.Now;
                            model2.Title = "动态新闻背景色";
                            ModulsBLL.SingleModel.Add(model2);
                        }
                    }

                    string updatecolumn = "Lastdate,Hidden,Content,ImgUrl,Content2,Name,Level,LitleImgUrl,Title,Address,AddressPoint,mobile,Color,State";
                    if (infos.State == 0)
                    {
                        updatecolumn = "Lastdate,State";
                        result.IsSucceed("删除成功 !");
                    }
                    else
                    {
                        result.IsSucceed("修改成功 !");
                    }
                    infos.Lastdate = DateTime.Now;

                    ModulsBLL.SingleModel.Update(infos, updatecolumn);
                    result.Data = new Dictionary<string, object> { { "datas", infos } };
                }

                //首页轮播图
                if (!string.IsNullOrEmpty(infos.ImgUrl) && infos.Level == 1)
                {
                    string[] imgArray = infos.ImgUrl.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (imgArray.Length > 0)
                    {
                        foreach (string item in imgArray.Where(item => !string.IsNullOrEmpty(item) && item.IndexOf("http://", StringComparison.Ordinal) == 0))
                        {
                            C_AttachmentBLL.SingleModel.Add(new C_Attachment
                            {
                                itemId = infos.Id,
                                createDate = DateTime.Now,
                                filepath = item,
                                itemType = (int)AttachmentItemType.小程序官网首页轮播图,
                                thumbnail = item,
                                status = 0
                            });
                        }
                    }
                    infos.ImgUrl = "";
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(result.IsFailed("服务器出错 , 请重试 !" + ex.Message));
            }
        }

        public ActionResult DeleteImgIndex(int id)
        {
            try
            {
                C_AttachmentBLL.SingleModel.DeleteImg(id);
                return Json(new { Success = true, Msg = "删除成功" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Msg = "删除失败" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取动态新闻数据
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult GetNewsData(int id)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            ServiceResult result = new ServiceResult();
            try
            {
                if (id == 0)
                {
                    return Json(result.IsFailed("系统繁忙auth_limited !"));
                }
                List<Moduls> infos = ModulsBLL.SingleModel.GetList("Id = " + id);
                result.IsSucceed("获取成功 !");
                result.Data = new Dictionary<string, object> { { "datas", infos } };

                return Json(result);
            }
            catch (Exception)
            {
                return Json(result.IsFailed("服务器出错 , 请重试 !"));
            }
        }

        /// <summary>
        /// 局部加载上传图片控件
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newid"></param>
        /// <returns></returns>
        [HttpGet]
        [ValidateInput(false)]
        public ActionResult ReflashImg(int id, string newid)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            //Miniapp listtemp = _miniappBll.GetModelByRelationId(id);
            //if(listtemp==null)
            //{
            //    return View("error");
            //}

            if (!string.IsNullOrWhiteSpace(newid))
            {
                Moduls infos = ModulsBLL.SingleModel.GetModel("Id = " + newid);
                if (infos != null)
                {
                    return PartialView("MiappImgUpload", infos);
                }
            }

            return PartialView("MiappImgUpload", new Moduls());
        }
        /// <summary>
        /// 局部加载上传图片控件
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newid"></param>
        /// <returns></returns>
        [HttpGet]
        [ValidateInput(false)]
        public ActionResult ReflashModelImg(int id)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            Miniapp infos = MiniappBLL.SingleModel.GetModelByRelationId(id);
            //var infos = _miniappBll.GetModel("Id = " + id);
            if (infos != null)
            {
                return PartialView("MiappImgUpload", new Moduls() { ImgUrl = infos.ImgUrl });
            }

            return PartialView("MiappImgUpload", new Moduls());
        }

        /// <summary>
        /// 获取动态新闻列表
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult GetNewsList(int appid, int Level)
        {
            ServiceResult result = new ServiceResult();
            try
            {
                if (dzaccount == null)
                {
                    return Json(result.IsFailed("系统繁忙auth_null !"));
                }
                if (appid == 0)
                {
                    return Json(result.IsFailed("系统繁忙appid_0 !"));
                }
                if (Level == 0)
                {
                    return Json(result.IsFailed("系统繁忙Level_0 !"));
                }
                //Miniapp listtemp = _miniappBll.GetModelByRelationId(appid);
                //if(listtemp==null)
                //{
                //    return Json(result.IsFailed("系统繁忙model_null !"));
                //}

                string sql = "appId = " + appid + " and Level = " + Level + " and State = 1";
                List<Moduls> model = ModulsBLL.SingleModel.GetList(sql);
                if (model != null && model.Count > 0)
                {
                    result.Data = new Dictionary<string, object> { { "datas", model } };
                }
                else
                {
                    result.Data = new Dictionary<string, object> { { "datas", "" } };
                }
                result.IsSucceed("获取成功 !");

                return Json(result);
            }
            catch (Exception)
            {
                return Json(result.IsFailed("服务器出错 , 请重试 !"));
            }

        }

        /// <summary>
        /// 获得发展历程数据
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public ActionResult GetDevelopmentData(int appid, int pageSize = 10, int pageIndexInt = 1)
        {
            ServiceResult result = new ServiceResult();
            try
            {
                if (dzaccount == null)
                {
                    return Json(result.IsFailed("系统繁忙auth_null !"));
                }

                if (appid == 0)
                {
                    result.Data = new Dictionary<string, object> { { "datas", "" } };
                }
                else
                {
                    //Miniapp listtemp = _miniappBll.GetModelByRelationId(appid);
                    string wheresql = "appId = " + appid + " and State = 1";

                    List<Development> datass = DevelopmentBLL.SingleModel.GetList(wheresql, pageSize, pageIndexInt, "", "Year desc,Month desc");
                    result.Data = new Dictionary<string, object> { { "datas", datass } };
                }

                return Json(result);
            }
            catch (Exception)
            {
                return Json(result.IsFailed("服务器出错 , 请重试 !"));
            }
        }

        /// <summary>
        /// 添加或编辑发展历程数据
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public ActionResult AddOrEditDevelopmentData(Development infos)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            ServiceResult result = new ServiceResult();
            try
            {
                if (dzaccount == null)
                {
                    return Json(result.IsFailed("系统繁忙auth_null !"));
                }

                if (infos == null)
                {
                    return Json(result.IsFailed("系统繁忙auth_null !"));
                }
                if (infos.Id == 0)
                {
                    infos.Lastdate = DateTime.Now;
                    infos.Createdate = DateTime.Now;
                    infos.State = 1;
                    //添加数据
                    object datatemp = DevelopmentBLL.SingleModel.Add(infos);
                    result.Data = new Dictionary<string, object> { { "id", datatemp } };
                    result.IsSucceed("添加成功 !");
                }
                else
                {
                    string updatecolumn = "Content,Year,Month,Lastdate,State";
                    if (infos.State == 0)
                    {
                        updatecolumn = "Lastdate,State";
                    }

                    infos.Lastdate = DateTime.Now;
                    //修改数据
                    DevelopmentBLL.SingleModel.Update(infos, updatecolumn);
                    result.Data = new Dictionary<string, object> { { "id", infos.Id } };
                    result.IsSucceed("修改成功 !");
                }

                return Json(result);
            }
            catch (Exception)
            {
                return Json(result.IsFailed("服务器出错 , 请重试 !"));
            }
        }

        #region 百度地图弹框

        [HttpPost]
        public ActionResult _PartailMapPoint(string lat, string lng, string address)
        {
            // 根据坐标查位置
            if (!lat.IsNullOrWhiteSpace() && !lng.IsNullOrWhiteSpace() && lat != "0" && lng != "0")
            {
                ViewBag.Condition = new Dictionary<string, string> { { "Type", "1" }, { "Data", lng + ',' + lat } };
            }
            else if (!address.IsNullOrWhiteSpace())  // 地址
            {
                ViewBag.Condition = new Dictionary<string, string> { { "Type", "2" }, { "Data", address } };
            }
            //else if (!area.IsNullOrWhiteSpace())  //区域
            //{
            //    ViewBag.Condition = new Dictionary<string, string> { { "Type", "3" }, { "Data", area } };
            //}
            else  // IP
            {
                ViewBag.Condition = new Dictionary<string, string> { { "Type", "4" }, { "Data", "广州市" } };
            }
            return PartialView("MapPoint");
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 纯文本中把空格标识用p标签样式text-indent:2rem;代替
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string ReturnStr3(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return content;
            }

            if (content.IndexOf("\n") <= 0)
            {
                if (content.IndexOf(" ") == 0)
                {
                    content = "<p style='text-indent:2em;'>" + content + "</p>";
                }
                else
                {
                    content = "<p>" + content + "</p>";
                }
                return content;
            }

            string temp = content.Substring(0, content.IndexOf("\n"));
            if (temp.IndexOf(" ") == 0)
            {
                return "<p style='text-indent:2em;'>" + temp + "</p>\n" + ReturnStr3(content.Replace(temp + "\n", ""));
            }
            else
            {
                return "<p>" + temp + "</p>\n" + ReturnStr3(content.Replace(temp + "\n", ""));
            }
        }
        #region 删除图片

        public ActionResult DeleteImg(int id, string imgurl)
        {
            try
            {
                Moduls model = ModulsBLL.SingleModel.GetModel(id);
                if (model != null && model.ImgUrl != null)
                {
                    if (model.ImgUrl.IndexOf(imgurl + ",") > -1)
                    {
                        model.ImgUrl = model.ImgUrl.Replace(imgurl + ",", "");
                    }
                    else
                    {
                        model.ImgUrl = model.ImgUrl.Replace(imgurl, "");
                    }

                    if (ModulsBLL.SingleModel.Update(model, "ImgUrl"))
                    {
                        return Json(new { Success = true, Msg = "删除成功" }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { Success = false, Msg = "删除失败" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Msg = "删除失败" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #endregion
        
        public ActionResult UserList()
        {
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "用户不存在!", code = "500" });
            }
            return Content(JsonConvert.SerializeObject(agentinfo));
            //return View(agentinfo);
        }

    }
}