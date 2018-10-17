using BLL.MiniApp.cityminiapp;
using BLL.MiniApp.Dish;
using Entity.MiniApp;
using Entity.MiniApp.cityminiapp;
using Entity.MiniApp.Dish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Dish.Filters;
using User.MiniApp.Areas.Dish.Models;
using User.MiniApp.Areas.DishVipCardDish.Models;
using Utility.IO;

namespace User.MiniApp.Areas.Dish.Controllers
{
    [LoginFilter]
    public class SwiperController : User.MiniApp.Controllers.baseController
    {
        
        protected readonly Return_Msg _result;

        public SwiperController()
        {
            _result = new Return_Msg();
            _result.Msg = "执行失败";

            
        }

        // GET: Dish/swiper
        public ActionResult Index()
        {
            int aid = Context.GetRequestInt("aId", 0);
            if (aid <= 0)
            {
                _result.code = "500";
                _result.Msg = "参数错误";
                return View("PageError", _result);
            }
            return View();
        }

        public ActionResult GetPictures()
        {
            int aid = Context.GetRequestInt("aId", 0);
            if (aid <= 0)
            {
                _result.Msg = "参数错误";
                return Json(_result);
            }
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int recordCount = 0;
            List<DishPicture> list = DishPictureBLL.SingleModel.GetLunboImgs(pageSize, pageIndex, aid, out recordCount);
            _result.dataObj = new
            {
                recordCount,
                list
            };
            _result.isok = true;
            return Json(_result);
        }
        public ActionResult Edit(int aid = 0, int id = 0)
        {
            if (aid <= 0)
            {
                _result.code = "500";
                _result.Msg = "参数错误";
                return View("PageError", _result);
            }
            LunboEditViewModel model = new LunboEditViewModel();
            List<DishStore> storeList = DishStoreBLL.SingleModel.GetAvailableStore(aid);
            if (id > 0)
                model.picture = DishPictureBLL.SingleModel.GetModelById_Aid(id, aid);

            model.storeList = storeList;

            return View(model);
        }

        public ActionResult SavePicture(DishPicture picture)
        {
            int aid = Context.GetRequestInt("aId", 0);
            if (aid <= 0)
            {
                _result.Msg = "参数错误";
                return Json(_result);
            }
            if (picture == null)
            {
                _result.Msg = "参数错误!";
                return Json(_result);
            }
            if (picture.Id > 0)
            {
                DishPicture model = DishPictureBLL.SingleModel.GetModelById_Aid(picture.Id, aid);
                if (model == null)
                {
                    _result.Msg = "数据错误!";
                    return Json(_result);
                }
                model.title = picture.title;
                model.is_order = picture.is_order;
                model.is_show = picture.is_show;
                model.img = picture.img;
                model.url = picture.url;
                _result.isok = DishPictureBLL.SingleModel.Update(model, "title,is_order,is_show,img,url");
                _result.Msg = _result.isok ? "保存成功" : "保存失败";
                return Json(_result);
            }
            else
            {
                picture.addTime = DateTime.Now;
                picture.aid = aid;
                picture.Id = Convert.ToInt32(DishPictureBLL.SingleModel.Add(picture));
                _result.isok = picture.Id > 0;
                _result.Msg = _result.isok ? "添加成功" : "添加失败";
                return Json(_result);
            }
        }

        public ActionResult DelPicture()
        {
            int aid = Context.GetRequestInt("aid", 0);
            if (aid <= 0)
            {
                _result.Msg = "参数错误";
                return Json(_result);
            }
            int pid = Context.GetRequestInt("pid", 0);
            if (pid <= 0)
            {
                _result.Msg = "参数错误!";
                return Json(_result);
            }
            DishPicture model = DishPictureBLL.SingleModel.GetModelById_Aid(pid, aid);
            if (model == null)
            {
                _result.Msg = "数据错误";
                return Json(_result);
            }
            model.state = -1;
            _result.isok = DishPictureBLL.SingleModel.Update(model, "state");
            if (_result.isok)
            {
                _result.Msg = "操作成功";
               
            }
            else
            {
                _result.Msg = "操作失败";
            }
            return Json(_result);

        }
        public ActionResult ShowPicture()
        {
            int aid = Context.GetRequestInt("aid", 0);
            if (aid <= 0)
            {
                _result.Msg = "参数错误";
                return Json(_result);
            }
            int pid = Context.GetRequestInt("id", 0);
            if (pid <= 0)
            {
                _result.Msg = "参数错误!";
                return Json(_result);
            }
            DishPicture model = DishPictureBLL.SingleModel.GetModelById_Aid(pid, aid);
            if (model == null)
            {
                _result.Msg = "数据错误";
                return Json(_result);
            }
            int is_show = Context.GetRequestInt("is_show", 0);
            model.is_show = is_show;
            _result.isok = DishPictureBLL.SingleModel.Update(model, "is_show");
            _result.Msg = _result.isok ? "操作成功" : "操作失败";
            return Json(_result);
        }
    }
}