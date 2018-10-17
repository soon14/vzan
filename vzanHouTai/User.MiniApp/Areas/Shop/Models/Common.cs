using AutoMapper;
using BLL.MiniApp.Dish;
using Core.MiniApp.DTO;
using Entity.MiniApp;
using Entity.MiniApp.Dish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace User.MiniApp.Areas.Shop.Models
{
    public class Common
    {
        #region 单例
        private static Common _singleModel;
        private static readonly object SynObject = new object();

        private Common()
        {

        }

        public static Common SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new Common();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public JsonResult ApiModel(bool isok = false, string message = null, string code = null, object data = null)
        {
            return new JsonResult()
            {
                Data = new Return_Msg
                {
                    Msg = message,
                    code = code,
                    dataObj = data,
                    isok = isok
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        public DishGood ConvertToDAO(EditProduct DTO)
        {
            return new DishGood
            {
                img = DTO.Img,
                g_name = DTO.Name,
                g_description = DTO.Description,
                g_danwei = DTO.Unit,
                g_renqi = DTO.Hit,
                g_print_tag = DTO.PrintTagId,
                shop_price = DTO.Price,
                market_price = DTO.OriginalPrice,
                dabao_price = DTO.PackingFee,
                day_kucun = DTO.DailySupply,
                is_order = DTO.Sort,
                is_waimai = DTO.IsTakeOut.Value ? 1 : 0,
                state = DTO.Display.Value ? 1 : 0,
                cate_id = DTO.CategoryId,
                yue_xiaoliang = DTO.Sale,
                add_time = DateTime.Now,
                update_time = DateTime.Now,
            };
        }

        public EditProduct ConvertToDTO(DishGood DAO)
        {
            return new EditProduct
            {
                Img = DAO.img,
                Name = DAO.g_name,
                Description = DAO.g_description,
                Unit = DAO.g_danwei,
                Hit = DAO.g_renqi,
                PrintTagId = DAO.g_print_tag,
                Price = DAO.shop_price,
                OriginalPrice = DAO.market_price,
                PackingFee = DAO.dabao_price,
                DailySupply = DAO.day_kucun,
                Sort = DAO.is_order,
                IsTakeOut = DAO.is_waimai == 1,
                Display = DAO.state == 1,
                CategoryId = DAO.cate_id,
                Sale = DAO.yue_xiaoliang,
            };
        }

        public List<EditProductAttr> ConvertToDTO(List<DishGoodAttr> DAO)
        {
            return Mapper.Map<List<DishGoodAttr>, List<EditProductAttr>>(DAO);
        }

        public List<EditProductAttr> ConvertToDAO(List<DishGoodAttr> DTO)
        {
            return Mapper.Map<List<DishGoodAttr>, List<EditProductAttr>>(DTO);
        }

        public List<DishGoodAttr> ConvertToDAO(List<EditProductAttr> DTO)
        {
            if(DTO == null || DTO.Count == 0)
            {
                return new List<DishGoodAttr>();
            }
            string attrId = string.Join(",", DTO.GroupBy(item => item.AttrId).Select(group => group.First()).Select(item => item.AttrId));
            List<DishAttr> attrConfig = DishAttrBLL.SingleModel.GetListById(attrId);
            return DTO.Where(item => attrConfig.Exists(config => config.id == item.AttrId)).Select(item => new DishGoodAttr
            {
                id = item.Id,
                attr_id = item.AttrId,
                attr_type_id = attrConfig.Find(config => config.id == item.AttrId).cat_id,
                value = item.Option ?? "",
                price = item.Price,
            }).ToList();
        }
    }
}