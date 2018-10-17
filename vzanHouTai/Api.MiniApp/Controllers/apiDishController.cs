using Api.MiniApp.Filters;
using Api.MiniApp.Models.Dish;
using BLL.MiniApp;
using BLL.MiniApp.Dish;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Tools;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.CoreHelper;
using Entity.MiniApp.Dish;
using Entity.MiniApp.Tools;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    [AuthCheck]
    public class apiDishController : InheritController
    {
        private DishApiReturnMsg result;

        /// <summary>
        /// 手机号码提交 Redis_key
        /// </summary>
        private static readonly string DISH_PHONE_KEY = "DISH_PHONE_KEY_{0}";

        /// <summary>
        /// 不要把所有BLL在这里集中实例化了，要改正这个写法
        /// </summary>
        public apiDishController()
        {
            result = new DishApiReturnMsg();
        }

        [AllowAnonymous]
        public ActionResult getAid(string appid = "")
        {
            if (string.IsNullOrEmpty(appid))
            {
                result.info = "appid不能为空";
                return Json(result);
            }
            XcxAppAccountRelation model = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appid);
            if (model == null)
            {
                result.info = $"未找到和appid:{appid}相关联的小程序";
                return Json(result);
            }
            result.code = 1;
            result.info = model.Id;
            return Json(result);
        }

        /// <summary>
        /// 店铺配置
        /// </summary>
        /// <returns></returns>
        public JsonResult getDishConfig(int aid = 0)
        {

            DishSetting model = DishSettingBLL.SingleModel.GetModelByAid(aid);
            if (model == null)
            {
                model = new DishSetting();
                model.aid = aid;
                model.Id = Convert.ToInt32(DishSettingBLL.SingleModel.Add(model));
                if (model.Id <= 0)
                {
                    result.code = 5;
                    result.info = "数据错误";
                }
                else
                {
                    result.code = 1;
                    result.info = new
                    {
                        model.dish_rz_open,
                        dish_type = 2,//1=单门店，2=多门店，默认使用2
                        model.dish_xiaoliang_show_type,
                        model.dish_share,
                        model.dish_focus_wx
                    };
                }
            }
            else
            {
                result.code = 1;
                result.info = new
                {
                    model.dish_rz_open,
                    dish_type = 2,//1=单门店，2=多门店，默认使用2
                    model.dish_xiaoliang_show_type,
                    model.dish_share,
                    model.dish_focus_wx
                };
            }
            return Json(result);
        }

        /// <summary>
        /// 门店列表
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页数</param>
        /// <param name="ws_lat">纬度</param>
        /// <param name="ws_lng">经度</param>
        /// <param name="sort_type">排序方式：1=综合排序，2=销量最高，3=距离最近</param>
        /// <param name="keywords">搜索关键词</param>
        /// <param name="cate_id">门店分类ID</param>
        /// <returns></returns>
        public ActionResult getDishList(int aid, int pageIndex = 1, int pageSize = 10, double ws_lat = 0d, double ws_lng = 0d, int sort_type = 1, string keywords = "", int cate_id = 0, string utoken = "")
        {
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }

            List<DishStore> storeList = DishStoreBLL.SingleModel.GetListByCondition(aid, ws_lat, ws_lng, sort_type, keywords, cate_id, pageIndex, pageSize);
            List<DishCategory> dishCategoryList = DishCategoryBLL.SingleModel.GetList($" aid={aid} and is_show=1 and state=1 and storeId=0 ", 100, 1, "*", "is_order desc");
            List<DishPicture> pictureList = DishPictureBLL.SingleModel.GetList($"aid={aid} and is_show=1 and state=1 and type=0", 100, 1, "*", "is_order desc");

            if (storeList != null && storeList.Count > 0)
            {
                for (int i = 0; i < storeList.Count; i++)
                {
                    DishStore model = storeList[i];
                    //基本设置
                    try { model.baseConfig = JsonConvert.DeserializeObject<DishBaseConfig>(model.baseConfigJson) ?? new DishBaseConfig(); }
                    catch { model.baseConfig = new DishBaseConfig(); }
                    //高级设置
                    try { model.gaojiConfig = JsonConvert.DeserializeObject<DishGaojiConfig>(model.gaojiConfigJson) ?? new DishGaojiConfig(); }
                    catch { model.gaojiConfig = new DishGaojiConfig(); }
                    //店内设置
                    try { model.dianneiConfig = JsonConvert.DeserializeObject<DishDianneiConfig>(model.dianneiConfigJson) ?? new DishDianneiConfig(); }
                    catch { model.dianneiConfig = new DishDianneiConfig(); }
                    //外卖设置
                    try { model.takeoutConfig = JsonConvert.DeserializeObject<DishTakeoutConfig>(model.takeoutConfigJson) ?? new DishTakeoutConfig(); }
                    catch { model.takeoutConfig = new DishTakeoutConfig(); }
                    //跳转设置
                    try { model.tiaozhuanConfig = JsonConvert.DeserializeObject<DishTiaoZhuanConfig>(model.tiaozhuanConfigJson) ?? new DishTiaoZhuanConfig(); }
                    catch { model.tiaozhuanConfig = new DishTiaoZhuanConfig(); }

                    model.stars = DishCommentBLL.SingleModel.GetStars(model.id);

                    model.dish_yue_xiaoliang = DishOrderBLL.SingleModel.GetXiaoLiang(model.id);
                    DishStoreBLL.SingleModel.GetDishActivityInfo(ref model, userInfo.Id);
                }
            }
            result.code = 1;
            result.info = new
            {
                index_cate_list = dishCategoryList,
                index_dish_list = storeList,
                index_swiper_list = pictureList
            };
            return Json(result);
        }

        /// <summary>
        /// 获取单个店铺信息
        /// </summary>
        /// <returns></returns>
        public JsonResult getDishInfo(int dish_id = 0, string utoken = "")
        {
            if (dish_id <= 0)
            {
                result.code = 5;
                result.info = "非法请求,dish_id<=0";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }
            DishStore dishStoreModel = DishStoreBLL.SingleModel.GetModel(dish_id);
            if (dishStoreModel == null || dishStoreModel.state < 0)
            {
                result.code = 5;
                result.info = "店铺不存在或已关闭";
                return Json(result);
            }


            List<DishCategory> dishCategoryList = DishCategoryBLL.SingleModel.GetListBySql($"select * from dishcategory where storeid={dish_id} and is_show=1 and state=1  order by is_order desc");


            List<DishComment> commentList = DishCommentBLL.SingleModel.GetList($"storeid={dish_id} and state=1", 20, 1, "*", "id desc")??new List<DishComment>();
            commentList.ForEach(comment =>
            {
                C_UserInfo commentUserInfo = C_UserInfoBLL.SingleModel.GetModel(comment.uId);
                comment.headImg = commentUserInfo == null ? "" : commentUserInfo.HeadImgUrl;
            });
            int commentCount = DishCommentBLL.SingleModel.GetCount($"storeid={dish_id} and state=1");

            int totalScore = Utils.SafeGetDBObject(SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(), CommandType.Text, $"SELECT sum(star) from dishcomment where storeid={dish_id}  and state=1"), 0);
            double averageScore = totalScore;
            if (commentCount > 0)
            {
                averageScore = totalScore / (double)commentCount;
            }
            //基本设置
            try { dishStoreModel.baseConfig = JsonConvert.DeserializeObject<DishBaseConfig>(dishStoreModel.baseConfigJson) ?? new DishBaseConfig(); }
            catch { dishStoreModel.baseConfig = new DishBaseConfig(); }
            //高级设置
            try { dishStoreModel.gaojiConfig = JsonConvert.DeserializeObject<DishGaojiConfig>(dishStoreModel.gaojiConfigJson) ?? new DishGaojiConfig(); }
            catch { dishStoreModel.gaojiConfig = new DishGaojiConfig(); }
            //店内设置
            try { dishStoreModel.dianneiConfig = JsonConvert.DeserializeObject<DishDianneiConfig>(dishStoreModel.dianneiConfigJson) ?? new DishDianneiConfig(); }
            catch { dishStoreModel.dianneiConfig = new DishDianneiConfig(); }
            //外卖设置
            try { dishStoreModel.takeoutConfig = JsonConvert.DeserializeObject<DishTakeoutConfig>(dishStoreModel.takeoutConfigJson) ?? new DishTakeoutConfig(); }
            catch { dishStoreModel.takeoutConfig = new DishTakeoutConfig(); }
            //跳转设置
            try { dishStoreModel.tiaozhuanConfig = JsonConvert.DeserializeObject<DishTiaoZhuanConfig>(dishStoreModel.tiaozhuanConfigJson) ?? new DishTiaoZhuanConfig(); }
            catch { dishStoreModel.tiaozhuanConfig = new DishTiaoZhuanConfig(); }
            //支付设置
            try { dishStoreModel.paySetting = JsonConvert.DeserializeObject<DishPaySetting>(dishStoreModel.paySettingJson) ?? new DishPaySetting(); }
            catch { dishStoreModel.paySetting = new DishPaySetting(); }
            DishVipCardSetting cardSetting = DishVipCardSettingBLL.SingleModel.GetModelByStoreId(dishStoreModel.id);
            DishStoreBLL.SingleModel.GetDishActivityInfo(ref dishStoreModel, userInfo.Id);
            int card_open_status = cardSetting == null ? 0 : cardSetting.card_open_status;
            string card_info = cardSetting == null ? string.Empty : cardSetting.card_info;
            int order_type = Context.GetRequestInt("order_type", 1);


            StringBuilder filterSql = new StringBuilder();
            if (order_type == 2)
            {
                filterSql.Append($" and is_waimai = 1 ");
            }

            #region info

            int is_yingye_status = GetStoreYingyeStatus(order_type, dishStoreModel);
            result.code = 1;
            result.info = new
            {
                dish_cate = dishCategoryList,//菜品分类
                comment_list = commentList,//评价
                dish_comment_count = commentCount,//评价总数
                dish_comment_fenshu = averageScore.ToString("F1"),//平均分数
                dish_info = new
                {
                    dishGoodsCount = DishGoodBLL.SingleModel.GetCount($"storeid={dish_id} and state=1 {filterSql}"),
                    dishStoreModel.storeHome_qrcode,
                    card_open_status,
                    dishStoreModel.dish_con_mobile,
                    dishStoreModel.dish_con_phone,
                    dish_gps_lat = dishStoreModel.ws_lat,
                    dish_gps_lng = dishStoreModel.ws_lng,
                    card_info,
                    dish_id = dishStoreModel.id,
                    dish_is_zntuijian = 0,
                    dishStoreModel.ps_open_status,//是否开启配送

                    dishStoreModel.dish_logo,
                    dishStoreModel.dish_name,

                    dishStoreModel.gaojiConfig.dish_is_diannei,
                    dishStoreModel.gaojiConfig.dish_is_fapiao,
                    dishStoreModel.gaojiConfig.dish_waimai_text,
                    dishStoreModel.gaojiConfig.dish_webview_url,
                    dishStoreModel.gaojiConfig.dish_is_paidui,
                    dishStoreModel.gaojiConfig.dish_is_sms_check,
                    dishStoreModel.gaojiConfig.dish_is_waimai,
                    dishStoreModel.gaojiConfig.dish_is_webview_open,
                    dishStoreModel.gaojiConfig.dish_is_yuding,
                    dishStoreModel.gaojiConfig.dish_is_ziqu,
                    dishStoreModel.gaojiConfig.dish_paidui_text,
                    dishStoreModel.gaojiConfig.dish_pay_limit_time,
                    dishStoreModel.gaojiConfig.dish_yuding_text,
                    dishStoreModel.gaojiConfig.dish_ziqu_text,
                    dishStoreModel.gaojiConfig.dish_diannei_text,

                    dishStoreModel.dianneiConfig.dish_is_zhuohao_change,
                    dishStoreModel.dianneiConfig.dish_is_rcode_open,
                    dishStoreModel.dianneiConfig.dish_ziqu_day,
                    dishStoreModel.dianneiConfig.dish_diannei_tips_one,
                    dishStoreModel.dianneiConfig.dish_diannei_tips_show,
                    dishStoreModel.dianneiConfig.dish_diannei_tips_two,
                    dishStoreModel.dianneiConfig.dish_diannei_fangshi,

                    dishStoreModel.baseConfig.dish_address,
                    dishStoreModel.baseConfig.dish_fuwu,
                    dishStoreModel.baseConfig.dish_gonggao,
                    dishStoreModel.baseConfig.dish_pingjunxiaofei,
                    dishStoreModel.baseConfig.dish_jieshao,
                    dishStoreModel.baseConfig.dish_open_status,
                    dishStoreModel.baseConfig.dish_quyu,
                    dishStoreModel.baseConfig.dish_shijing,
                    dishStoreModel.baseConfig.dish_show_status,
                    dishStoreModel.baseConfig.dish_yuding_gonggao,
                    dishStoreModel.tiaozhuanConfig.dish_tomini_appid,
                    dishStoreModel.tiaozhuanConfig.dish_tomini_apptext,
                    dishStoreModel.tiaozhuanConfig.dish_tomini_appurl,
                    dishStoreModel.tiaozhuanConfig.dish_is_open_tomini,

                    //dish_yingye_btime = 0,
                    //dish_yingye_etime = 0,
                    //dish_yingye_time = "",
                    //dish_yingye_waimai_time = "",
                    //dish_ziqu_time = "",
                    dishStoreModel.baseConfig.open_time,
                    dishStoreModel.baseConfig.wm_time,
                    dishStoreModel.dianneiConfig.zq_time,
                    dishStoreModel.takeoutConfig.ps_rule,
                    dishStoreModel.baseConfig.dish_zizhi,
                    dishStoreModel.baseConfig.dish_share,
                    dish_zong_xiaoliang = 0,
                    dish_zuijin_xiaoliang = 0,
                    huodong_quan_jiner = DishActivityBLL.SingleModel.GetQuanJiner(dish_id, userInfo.Id),//商家代金券总金额
                    is_yingye_status,
                    dishStoreModel.takeoutConfig.dish_waimai_auto_post,
                    dishStoreModel.takeoutConfig.waimai_limit_jiner,
                    dishStoreModel.takeoutConfig.waimai_limit_juli,
                    dishStoreModel.takeoutConfig.waimai_peisong_base_juli,
                    dishStoreModel.takeoutConfig.waimai_peisong_base_step,
                    dishStoreModel.takeoutConfig.waimai_peisong_jiner,
                    dishStoreModel.paySetting.pay_is_useCoupon,
                    waimai_peisong_jiner_guize = "",
                    waimai_peisong_jiner_hou = 0,
                    waimai_peisong_time_jiedian = "",
                    dishStoreModel.huodong_list,
                    dishStoreModel.gaojiConfig.menu_style,
                    dish_beizhu_text = dishStoreModel.gaojiConfig.dish_beizhu_info?.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[] { },
                },
            };

            #endregion info

            return Json(result);
        }

        private int GetStoreYingyeStatus(int order_type, DishStore dishStoreModel)
        {
            int code = 0;//0:未营业 1:营业中
            if (dishStoreModel.baseConfig.dish_open_status == 0) return code;
            if (order_type == 1)//店内
            {
                if (dishStoreModel.baseConfig.open_time == null || dishStoreModel.baseConfig.open_time.Count <= 0 || dishStoreModel.gaojiConfig.dish_is_diannei == 0) return code;
                List<DishOpenTime> open_time = dishStoreModel.baseConfig.open_time;
                string date = DateTime.Now.ToShortDateString();
                foreach (var item in open_time)
                {
                    DateTime startTime = Convert.ToDateTime($"{date} {item.dish_open_btime}");
                    DateTime endTime = DateTime.Now;
                    if (item.dish_open_etime == "24:00")
                    {
                        date = DateTime.Now.AddDays(1).ToShortDateString();
                        endTime = Convert.ToDateTime($"{date} 00:00");
                    }
                    else
                    {
                        endTime = Convert.ToDateTime($"{date} {item.dish_open_etime}");
                    }
                    if (DateTime.Compare(DateTime.Now, startTime) > 0 && DateTime.Compare(DateTime.Now, endTime) < 0)
                    {
                        code = 1;
                        return code;
                    }
                }
            }
            else//外卖
            {
                if (dishStoreModel.baseConfig.wm_time == null || dishStoreModel.baseConfig.wm_time.Count <= 0 || dishStoreModel.gaojiConfig.dish_is_waimai == 0) return code;
                List<DishOpenWmTime> open_time = dishStoreModel.baseConfig.wm_time;
                string date = DateTime.Now.ToShortDateString();
                foreach (var item in open_time)
                {
                    DateTime startTime = Convert.ToDateTime($"{date} {item.dish_open_wm_btime}");
                    DateTime endTime = DateTime.Now;
                    if (item.dish_open_wm_etime == "24:00")
                    {
                        date = DateTime.Now.AddDays(1).ToShortDateString();
                        endTime = Convert.ToDateTime($"{date} 00:00");
                    }
                    else
                    {
                        endTime = Convert.ToDateTime($"{date} {item.dish_open_wm_etime}");
                    }
                    if (DateTime.Compare(DateTime.Now, startTime) > 0 && DateTime.Compare(DateTime.Now, endTime) < 0)
                    {
                        code = 1;
                        return code;
                    }
                }
            }

            return code;
        }

        public JsonResult getDishOneInfo(int dish_id = 0)
        {
            if (dish_id <= 0)
            {
                result.info = "非法请求，dish_id <= 0";
                return Json(result);
            }
            DishStore model = DishStoreBLL.SingleModel.GetModel(dish_id);
            if (model == null || model.state <= 0)
            {
                result.info = "店铺不存在或已关闭";
                return Json(result);
            }

            #region info

            result.info = new
            {
                dish_id = model.id,
                card_info = "",
                card_open_status = 0,
                model.baseConfig.dish_address,
                model.baseConfig.dish_con_mobile,
                model.baseConfig.dish_con_phone,
                model.baseConfig.dish_fuwu,
                model.baseConfig.dish_fuwu_arr,
                model.baseConfig.dish_gonggao,
                model.baseConfig.dish_jieshao,
                model.baseConfig.dish_open_status,
                model.baseConfig.dish_pingjunxiaofei,
                model.baseConfig.dish_quyu,
                model.baseConfig.dish_show_status,
                model.baseConfig.dish_yuding_gonggao,

                dish_gps_lat = "",
                dish_gps_lng = "",
                dish_is_zntuijian = "",
                dish_all_pingfen = 0,
                dish_all_xiaoliang = 0,
                dish_yingye_btime = "",//营业开始时间
                dish_yingye_etime = "",//关店时间
                dish_yingye_time_text = "",

                dish_kong_xing = 0,//空心⭐的数量
                dish_shi_xing = 5,//实心⭐的数量
                dish_zong_xiaoliang = 0,//总销量
                dish_zuijin_xiaoliang = 0,//最近销量
                huodong_list = "",//活动

                model.dish_logo,
                model.dish_name,
                model.gaojiConfig.dish_beizhu_info,
                model.gaojiConfig.dish_diannei_text,
                model.gaojiConfig.dish_is_diannei,
                model.gaojiConfig.dish_is_fapiao,
                model.gaojiConfig.dish_is_paidui,
                model.gaojiConfig.dish_is_sms_check,
                model.gaojiConfig.dish_is_waimai,
                model.gaojiConfig.dish_is_webview_open,
                model.gaojiConfig.dish_is_yuding,
                model.gaojiConfig.dish_is_ziqu,
                model.gaojiConfig.dish_paidui_text,
                model.gaojiConfig.dish_pay_limit_time,
                model.gaojiConfig.dish_waimai_text,
                model.gaojiConfig.dish_webview_text,
                model.gaojiConfig.dish_webview_url,
                model.gaojiConfig.dish_yuding_text,
                model.gaojiConfig.dish_ziqu_text,

                //dish_shijing_arr=[],//商家环境
                //dish_zizhi_arr=[],//资质

                model.dianneiConfig.dish_diannei_fangshi,
                model.dianneiConfig.dish_diannei_tips_one,
                model.dianneiConfig.dish_diannei_tips_show,
                model.dianneiConfig.dish_diannei_tips_two,
                model.dianneiConfig.dish_is_rcode_open,
                model.dianneiConfig.dish_is_zhuohao_change,
                model.dianneiConfig.dish_ziqu_day,

                model.tiaozhuanConfig.dish_is_open_tomini,
                model.tiaozhuanConfig.dish_tomini_appid,
                model.tiaozhuanConfig.dish_tomini_apptext,
                model.tiaozhuanConfig.dish_tomini_appurl,

                model.takeoutConfig.waimai_limit_jiner,
                model.takeoutConfig.waimai_limit_juli,
                model.takeoutConfig.waimai_peisong_base_juli,
                model.takeoutConfig.waimai_peisong_base_step,
                model.takeoutConfig.waimai_peisong_jiner,
                waimai_peisong_jiner_guize = "",
                waimai_peisong_jiner_hou = "",
                waimai_peisong_time_jiedian = "",
            };

            #endregion info

            return Json(result);
        }

        /// <summary>
        /// 在线预定
        /// </summary>
        /// <returns></returns>
        public JsonResult dingAdd(DishYuDing model)
        {
            if (!ModelState.IsValid)
            {
                result.code = 0;
                result.info = this.ErrorMsg();
            }
            else
            {
                int newid = Convert.ToInt32(DishYuDingBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    result.code = 1;
                    result.info = "预订成功，请等待客服与您联系";
                }
                else
                    result.info = "预订失败";
            }
            return Json(result);
        }

        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="dish_id">门店ID</param>
        /// <param name="cate_id">分类ID</param>
        /// <param name="order_type">下单方式，2=外卖</param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="search_key">根据产品名称模糊查询</param>
        /// <returns>{code:1,info:[]}</returns>
        public JsonResult getGoodsListByCateId(int dish_id, int cate_id = 0, int order_type = 1, int pageSize = 20, int pageIndex = 1, string search_key = "")
        {
            if (dish_id <= 0)
            {
                result.info = "参数错误：dish_id不能<=0";
                return Json(result);
            }

            List<MySqlParameter> parameters = new List<MySqlParameter>();
            StringBuilder filterSql = new StringBuilder();

            if (!string.IsNullOrEmpty(search_key))
            {
                filterSql.Append(" and g_name like @search_key ");
                parameters.Add(new MySqlParameter("@search_key", Utils.FuzzyQuery(search_key)));
            }
            if (cate_id > 0)
            {
                filterSql.Append($" and cate_id = {cate_id} ");
            }
            if (order_type == 2)
            {
                filterSql.Append($" and is_waimai = 1 ");
            }


            List<DishGood> list = DishGoodBLL.SingleModel.GetListByParam($"storeid={dish_id} and state=1 { filterSql}", parameters.ToArray(), pageSize, pageIndex, "*", $"is_order desc,id desc");

            DateTime curMonthStartTime = Convert.ToDateTime($"{DateTime.Now.Year}-{DateTime.Now.Month}-1 00:00:00");
            DateTime curMonthEndTime = curMonthStartTime.AddMonths(1).AddDays(-1);

            list?.ForEach(good =>
            {
                if (good.goods_type > 0)
                {
                    //查询属性
                    List<DishAttr> attrList = DishAttrBLL.SingleModel.GetListBySql($"SELECT * from dishattr where id in(SELECT DISTINCT attr_id from dishgoodattr where goods_id={good.id} and attr_type_id={good.goods_type}) and state=1 order by sort desc");
                    List<good_attr> resultGoodAttrList = new List<good_attr>();
                    //查询属性值
                    List<DishGoodAttr> goodAttrList = DishGoodAttrBLL.SingleModel.GetListBySql($"SELECT ga.*,(SELECT a.attr_name from dishattr a where a.id=ga.attr_id) as attr_name from dishgoodattr ga where ga.goods_id={good.id} and ga.attr_type_id={good.goods_type}");
                    attrList?.ForEach(p =>
                    {
                        good_attr item = new good_attr();
                        item.attr_type = p.cat_id;
                        item.name = p.attr_name;

                        List<good_attr_value> values = goodAttrList?.Where(ga => ga.attr_id == p.id)?.Select(n => new good_attr_value
                        {
                            id = n.id,
                            label = n.value,
                            price = n.price,
                            format_price = n.price,
                        })?.ToList();
                        item.values = values;
                        resultGoodAttrList.Add(item);
                    });

                    good.goods_specification = resultGoodAttrList;
                }
                good.img = Utils.ResizeImg(good.img);
                //如果没填月销量，统计当月销量
                good.yue_xiaoliang += DishGoodBLL.SingleModel.GetSalesCountByTimeSpan(good.storeId, good.id, curMonthStartTime, curMonthEndTime);//月销量

            });
            result.code = 1;
            result.info = list;
            return Json(result);
        }

        /// <summary>
        /// 产品详情
        /// </summary>
        /// <returns></returns>
        public JsonResult getGoodsInfo(int goods_id)
        {
            if (goods_id <= 0)
            {
                result.info = "请求参数错误：goods_id<=0";
                return Json(result);
            }

            DishGood good = DishGoodBLL.SingleModel.GetModel(goods_id);
            if (good == null || good.state == -1)
            {
                result.info = "产品不存在或已删除";
                return Json(result);
            }
            if (good.state == 0)
            {
                result.info = "产品已下架";
                return Json(result);
            }
            good.img = Utils.ResizeImg(good.img);
            result.code = 1;
            result.info = good;
            return Json(result);
        }

        /// <summary>
        /// 查询产品规格
        /// </summary>
        /// <param name="goods_id"></param>
        /// <returns></returns>
        public JsonResult getOneGoodsAttr(int goods_id = 0)
        {
            if (goods_id <= 0)
            {
                result.info = "非法请求，goods_id<=0";
                return Json(result);
            }
            DishGood good = DishGoodBLL.SingleModel.GetModel(goods_id);
            if (good == null || good.state <= 0)
            {
                result.info = "产品不存在或已下架";
                return Json(result);
            }



            //查询属性
            List<DishAttr> attrList = DishAttrBLL.SingleModel.GetListBySql($"SELECT * from dishattr where id in(SELECT DISTINCT attr_id from dishgoodattr where goods_id={goods_id} and attr_type_id={good.goods_type}) and state=1 order by sort desc");
            List<good_attr> resultGoodAttrList = new List<good_attr>();
            //查询属性值
            List<DishGoodAttr> goodAttrList = DishGoodAttrBLL.SingleModel.GetListBySql($"SELECT ga.*,(SELECT a.attr_name from dishattr a where a.id=ga.attr_id) as attr_name from dishgoodattr ga where ga.goods_id={goods_id} and ga.attr_type_id={good.goods_type}");
            attrList?.ForEach(p =>
            {
                good_attr item = new good_attr();
                item.attr_type = p.cat_id;
                item.name = p.attr_name;

                List<good_attr_value> values = goodAttrList?.Where(ga => ga.attr_id == p.id)?.Select(n => new good_attr_value
                {
                    id = n.id,
                    label = n.value,
                    price = n.price,
                    format_price = n.price,
                })?.ToList();
                item.values = values;
                resultGoodAttrList.Add(item);
            });
            result.code = 1;
            result.info = new
            {
                good_a_info = new
                {
                    good.id,
                    good.g_name,
                    good.shop_price
                },
                good_attr = resultGoodAttrList,
            };
            return Json(result);
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="dish_id"></param>
        /// <returns></returns>
        public JsonResult getUserInfo(string utoken = "")
        {
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }
            result.code = 1;
            result.info = new
            {
                user_nickname = userInfo.NickName,
                user_headimg = userInfo.HeadImgUrl,
                u_sex = userInfo.Sex,
                u_phone = userInfo.TelePhone,
                u_address = userInfo.Address
            };
            return Json(result);
        }

        /// <summary>
        /// 保存用户信息
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="dish_id"></param>
        /// <returns></returns>
        public JsonResult editUserInfo(string utoken, DishUserInfo userInfo)
        {
            C_UserInfo model = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (model == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }
            model.NickName = userInfo.u_name;
            model.Sex = userInfo.u_sex;
            model.TelePhone = userInfo.u_phone;
            if (C_UserInfoBLL.SingleModel.Update(model, "NickName,Sex,TelePhone"))
            {
                result.code = 1;
                result.info = "保存成功！";
            }
            else
            {
                result.code = 0;
                result.info = "保存失败！";
            }
            return Json(result);
        }

        /// <summary>
        /// 查询发票
        /// </summary>
        /// <param name="dish_id"></param>
        /// <param name="utoken"></param>
        /// <returns></returns>
        public JsonResult getFapiaoList(string utoken)
        {
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户不存在";
                return Json(result);
            }

            List<DishInvoice> dishInvoiceList = DishInvoiceBLL.SingleModel.GetList($"userid={userInfo.Id} and state=1", 100, 1, "*", "id desc");
            result.code = 1;
            result.info = dishInvoiceList;
            return Json(result);
        }

        /// <summary>
        /// 添加发票
        /// </summary>
        /// <returns></returns>
        public JsonResult addFapiao(string utoken, DishInvoice model, string action_type = "")
        {
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户不存在";
                return Json(result);
            }

            model.userid = userInfo.Id;
            if (action_type == "add")
            {
                int newid = Convert.ToInt32(DishInvoiceBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    result.code = 1;
                    result.info = newid;
                }
                else
                {
                    result.info = "添加失败！";
                }
            }
            else if (action_type == "edit")
            {
                if (DishInvoiceBLL.SingleModel.Update(model))
                {
                    result.code = 1;
                    result.info = "保存成功";
                }
                else
                {
                    result.info = "保存失败！";
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 删除发票
        /// </summary>
        /// <returns></returns>
        public JsonResult deleteFapiao(string utoken, int fapiao_id)
        {
            if (fapiao_id <= 0)
            {
                result.info = "发票不存在";
                return Json(result);
            }

            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户不存在";
                return Json(result);
            }

            DishInvoice model = DishInvoiceBLL.SingleModel.GetModel(fapiao_id);
            if (model == null)
            {
                result.info = "发票不存在";
                return Json(result);
            }
            if (userInfo.Id != model.userid)
            {
                result.info = "非法操作";
                return Json(result);
            }
            model.state = -1;
            if (DishInvoiceBLL.SingleModel.Update(model))
            {
                result.code = 1;
                result.info = "删除成功";
            }
            else
            {
                result.info = "删除失败！";
            }
            return Json(result);
        }

        /// <summary>
        /// 发票详情
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="fapiao_id"></param>
        /// <returns></returns>
        public JsonResult getFapiaoInfo(string utoken, int fapiao_id)
        {

            DishInvoice model = DishInvoiceBLL.SingleModel.GetModel(fapiao_id);
            if (model == null || (model != null && model.state != 1))
            {
                result.info = "发票不存在或已删除";
                return Json(result);
            }
            result.code = 1;
            result.info = model;
            return Json(result);
        }

        /// <summary>
        /// 领券
        /// </summary>
        /// <returns></returns>
        public JsonResult quanLingQu(string utoken = "", int dish_id = 0)
        {

            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);

            //查询出店铺里所有可用的券
            List<DishActivity> activityList = DishActivityBLL.SingleModel.GetAvailableQuan(dish_id);
            //领券
            activityList?.ForEach(p =>
            {
                DishActivityBLL.SingleModel.DistributeActivity(userInfo.Id, p);
            });
            //查询用户的券
            var userActivityList = DishActivityUserBLL.SingleModel.GetAvailableQuan(dish_id, userInfo.Id);
            result.code = 1;
            result.info = new
            {
                all_quan_count = userActivityList?.Item2,
                quan_list = userActivityList?.Item1,
            };
            return Json(result);
        }

        /// <summary>
        /// 获取可用优惠券，下单时选择代金券
        /// </summary>
        /// <returns></returns>
        public JsonResult getUserQuanList(string utoken = "", int dish_id = 0,string goodsId="",string goodsInfo="")
        {

            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户不存在";
                return Json(result);
            }
            DishStore dishStore = DishStoreBLL.SingleModel.GetModel(dish_id);
            if (dishStore == null)
            {
                result.info = "门店错误";
                return Json(result);
            }
            //查询用户的券

            List<object> list = new List<object>();
            var userActivityList = DishActivityUserBLL.SingleModel.GetAvailableQuan(dish_id, userInfo.Id);
            if(userActivityList.Item1!=null && userActivityList.Item1.Count > 0)
            {
                foreach(var coupon in userActivityList.Item1)
                {
                    var obj = new {
                        coupon.aId,
                        coupon.dish_id,
                        coupon.quan_end_time_fmt,
                        coupon.quan_id,
                        coupon.quan_jiner,
                        coupon.quan_limit_jiner,
                        coupon.quan_name,
                        coupon.quan_status,
                        coupon.quan_type,
                        coupon.user_id,
                        isReductionCard=false,
                    };
                    list.Add(obj);
                }
            }
            List<CouponLog> reductionCardList = CouponLogBLL.SingleModel.GetListByApi(0, userInfo.Id.ToString(), dish_id, dishStore.aid.ToString(), 100, 1, "p.storeid desc,l.addtime desc", goodsId, goodsInfo);
            if (reductionCardList != null && reductionCardList.Count > 0)
            {
                foreach (var coupon in reductionCardList)
                {
                    var obj = new
                    {
                        aId= coupon.AId,
                        dish_id= coupon.StoreId,
                        quan_end_time_fmt= coupon.EndUseTimeStr,
                        quan_id= coupon.Id,
                        quan_jiner= coupon.Money_fmt,
                        quan_limit_jiner= coupon.LimitMoneyStr,
                        quan_name= coupon.CouponName,
                        quan_status= coupon.State,
                        quan_type=3,
                        user_id= coupon.UserId,
                        isReductionCard = true,
                    };
                    list.Add(obj);
                }
            }
           
            result.code = 1;
            result.info = list;
            return Json(result);
        }

        /// <summary>
        /// 餐桌名称
        /// </summary>
        /// <returns></returns>
        public JsonResult getDishTableInfo(int dish_id = 0, int table_id = 0)
        {
            if (dish_id <= 0 || table_id == 0)
            {
                result.info = new { table_id = 0, table_name = "" };
                return Json(result);
            }
            DishTable table = DishTableBLL.SingleModel.GetModel(table_id);
            result.code = 1;
            result.info = new
            {
                table_id = table?.id,
                table?.table_name
            };
            return Json(result);
        }

        /// <summary>
        /// 订单评论
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="oid"></param>
        /// <param name="fval"></param>
        /// <param name="fcon"></param>
        /// <param name="imgs"></param>
        /// <returns></returns>
        public JsonResult postComment(string utoken, int aId = 0, int storeId = 0, int oid = 0, int fval = 5, string fcon = "", string imgs = "")
        {
            if (aId <= 0 || storeId <= 0 || oid <= 0)
            {
                result.info = "参数错误";
                return Json(result);
            }
            if (!ModelState.IsValid)
            {
                result.info = this.ErrorMsg();
                return Json(result);
            }

            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户不存在";
                return Json(result);
            }

            DishComment comment = new DishComment()
            {
                aId = aId,
                imgs = imgs,
                content = fcon,
                nickName = userInfo.NickName,
                oId = oid,
                star = fval,
                uId = userInfo.Id,
                storeId = storeId,
            };
            int newid = Convert.ToInt32(DishCommentBLL.SingleModel.Add(comment));
            if (newid > 0)
            {
                DishStore store = DishStoreBLL.SingleModel.GetModel(storeId);
                if (store != null)
                {
                    store.commentCount++;
                    DishStoreBLL.SingleModel.Update(store, "commentcount");
                }
                result.code = 1;
                result.info = newid;
                DishOrder order = DishOrderBLL.SingleModel.GetModel(oid);
                if (order != null)
                {
                    order.order_status = (int)DishEnums.OrderState.已完成;
                    order.is_comment = 1;
                    DishOrderBLL.SingleModel.Update(order, "is_comment,order_status");
                }
            }
            else
            {
                result.info = "评论失败";
            }
            return Json(result);
        }

        /// <summary>
        /// 获取用户收货地址详情
        /// </summary>
        /// <returns></returns>
        public JsonResult getAddresssInfo(string utoken, int id = 0)
        {
            if (id <= 0)
            {
                result.info = "请求参数错误：要求id>0";
            }
            DishUserAddress model = DishUserAddressBLL.SingleModel.GetModel(id);
            if (model == null || model.state != 1)
            {
                result.info = "收货地址不存在或已删除";
            }
            else
            {
                result.code = 1;
                result.info = model;
            }
            return Json(result);
        }

        /// <summary>
        /// 获取用户所有收货地址
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="dish_id"></param>
        /// <returns></returns>
        public JsonResult getAddressList(string utoken)
        {


            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            result.code = 1;
            result.info = DishUserAddressBLL.SingleModel.GetList($"userid={userInfo.Id} and state=1");
            return Json(result);
        }

        /// <summary>
        /// 添加/修改用户收货地址
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public JsonResult addAddress(string utoken, DishUserAddress model)
        {
            if (!ModelState.IsValid)
            {
                result.code = 0;
                result.info = this.ErrorMsg();
            }
            else
            {
                if (model == null)
                {
                    result.code = 0;
                    result.info = "参数错误";
                    return Json(result);
                }
                if (model.u_lng <= 0 && model.u_lat <= 0)
                {
                    AddressApi addressdata = AddressHelper.GetLngAndLatByAddress(model.address);
                    if (addressdata == null || addressdata.result == null || addressdata.result.location == null)
                    {
                        result.code = 0;
                        result.info = "获取腾讯经纬度失败";
                        return Json(result);
                    }
                    model.u_lat = addressdata.result.location.lat;
                    model.u_lng = addressdata.result.location.lng;
                }


                C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
                if (model.id == 0)
                {
                    model.userid = userInfo.Id;
                    int newid = Convert.ToInt32(DishUserAddressBLL.SingleModel.Add(model));
                    if (newid > 0)
                    {
                        result.code = 1;
                        result.info = "添加成功";
                    }
                    else
                        result.info = "添加失败";
                }
                else
                {
                    model.update_time = DateTime.Now;
                    bool updateResult = DishUserAddressBLL.SingleModel.Update(model, "consignee,u_sex,mobile,address,buchong,u_lat,u_lng");
                    result.code = updateResult ? 1 : 0;
                    result.info = updateResult ? "修改成功" : "修改失败";
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 删除用户收货地址
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult deleteAddress(string utoken, int id)
        {
            if (id <= 0)
            {
                result.info = "请求参数错误：要求id>0";
            }

            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);

            DishUserAddress model = DishUserAddressBLL.SingleModel.GetModel(id);
            if (model == null)
            {
                result.info = "收货地址不存";
            }
            else if (model.state != 1)
            {
                result.info = "收货地址已删除";
            }
            else if (model.userid != userInfo.Id)
            {
                result.info = "非法操作";
            }
            else
            {
                model.state = -1;
                bool updateResult = DishUserAddressBLL.SingleModel.Update(model, "state");
                result.code = updateResult ? 1 : 0;
                result.info = updateResult ? "删除成功" : "删除失败";
            }
            return Json(result);
        }

        /// <summary>
        /// 验证是否领取会员卡
        /// </summary>
        /// <param name="shop_id"></param>
        /// <param name="utoken"></param>
        /// <returns></returns>
        public JsonResult UserIsCard(int shop_id = 0, string utoken = "")
        {

            if (shop_id <= 0 || string.IsNullOrEmpty(utoken))
            {
                result.info = "参数错误";
                return Json(result);
            }
            DishStore store = DishStoreBLL.SingleModel.GetModel(shop_id);
            if (store == null)
            {
                result.info = "店铺不存在";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }
            DishVipCard vipCard = DishVipCardBLL.SingleModel.GetVipCardByStoreId_UId(store.id, userInfo.Id);
            if (vipCard == null)
            {
                result.code = 0;
                // result.info = userInfo;
            }
            else
            {
                result.code = 1;
                result.info = "1";
            }
            return Json(result);
        }

        /// <summary>
        /// 获取会员卡信息
        /// </summary>
        /// <param name="shop_id"></param>
        /// <param name="utoken"></param>
        /// <returns></returns>
        public JsonResult GetCardInfo(int shop_id = 0, string utoken = "")
        {

            if (shop_id <= 0 || string.IsNullOrEmpty(utoken))
            {
                result.info = "参数错误";
                return Json(result);
            }
            DishStore store = DishStoreBLL.SingleModel.GetModel(shop_id);
            if (store == null)
            {
                result.info = "店铺不存在";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }
            DishVipCard vipCard = DishVipCardBLL.SingleModel.GetVipCardByStoreId_UId(store.id, userInfo.Id);
            if (vipCard == null)
            {
                int recordCount = DishVipCardBLL.SingleModel.GetCountByStoreId(store.id);
                vipCard = new DishVipCard();
                vipCard.is_new = 1;
                vipCard.aid = store.aid;
                vipCard.shop_id = store.id;
                vipCard.uid = userInfo.Id;
                vipCard.number += recordCount;
                vipCard.add_time = DateTime.Now;
                vipCard.Id = Convert.ToInt32(DishVipCardBLL.SingleModel.Add(vipCard));
                if (vipCard.Id <= 0)
                {
                    result.info = "领取会员卡失败";
                    return Json(result);
                }
            }
            result.code = 1;
            result.info = vipCard;
            return Json(result);
        }

        /// <summary>
        /// 充值记录
        /// </summary>
        /// <param name="shop_id"></param>
        /// <param name="utoken"></param>
        /// <param name="account_type"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public JsonResult GetLogList(int shop_id = 0, string utoken = "", int account_type = 0, int pageSize = 20, int pageIndex = 1)
        {


            if (shop_id <= 0 || string.IsNullOrEmpty(utoken))
            {
                result.info = "参数错误";
                return Json(result);
            }
            DishStore store = DishStoreBLL.SingleModel.GetModel(shop_id);
            if (store == null)
            {
                result.info = "店铺不存在";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }
            DishVipCard vipCard = DishVipCardBLL.SingleModel.GetVipCardByStoreId_UId(store.id, userInfo.Id);
            int recordCount = 0;
            List<DishCardAccountLog> list = DishCardAccountLogBLL.SingleModel.GetRecordLogList(store.id, userInfo.Id, pageIndex, pageSize, out recordCount, account_type);
            double account_all_pay = DishCardAccountLogBLL.SingleModel.GetAccountAll(1, store.id, userInfo.Id);
            double account_all_xiaofei = DishCardAccountLogBLL.SingleModel.GetAccountAll(2, store.id, userInfo.Id);
            result.code = 1;
            result.info = new
            {
                account_all_balance = vipCard.account_balance,
                account_all_pay,
                account_all_xiaofei,
                list,
            };
            return Json(result);
        }

        #region 下单流程 - 相关接口

        /// <summary>
        /// 1.添加购物车
        /// </summary>
        /// <param name="cdata"></param>
        /// <param name="utoken"></param>
        /// <param name="dish_id"></param>
        /// <returns></returns>
        public JsonResult AddDishCart(string cdata = "", string utoken = "", int dish_id = 0)
        {
            if (string.IsNullOrWhiteSpace(cdata) || string.IsNullOrEmpty(utoken) || dish_id <= 0)
            {
                result.info = "参数错误";
                return Json(result);
            }
            Dictionary<string, cdataModel> cdataModels = new Dictionary<string, cdataModel>();
            try
            {
                cdataModels = JsonConvert.DeserializeObject<Dictionary<string, cdataModel>>(cdata);
            }
            catch (Exception)
            {
                result.info = "参数有误";
                return Json(result);
            }

            DishStore store = DishStoreBLL.SingleModel.GetModel(dish_id);
            if (store == null)
            {
                result.info = "店铺不存在";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }

            //批量添加购物车
            List<DishShoppingCart> carts = new List<DishShoppingCart>();
            DishShoppingCart curCart = null;
            DishGood curGood = null;
            string goodsIds = string.Join(",", cdataModels.Select(s=>s.Value.goods_id).Distinct());
            List<DishGood> dishGoodList = DishGoodBLL.SingleModel.GetListByIds(goodsIds);

            foreach (KeyValuePair<string, cdataModel> item in cdataModels)
            {
                curCart = new DishShoppingCart();
                curGood = dishGoodList?.FirstOrDefault(f=>f.id.ToString() == item.Value.goods_id);

                curCart.aId = store.aid;
                curCart.storeId = store.id;
                curCart.user_id = userInfo.Id;
                curCart.goods_id = Convert.ToInt32(item.Value.goods_id);
                curCart.goods_name = item.Value.goods_name;
                curCart.goods_number = item.Value.cart_goods_number;
                curCart.goods_price = item.Value.goods_price;
                curCart.goods_attr = item.Value.goods_attr;
                curCart.goods_attr_id = item.Value.goods_attr_id;
                curCart.goods_img = curGood?.img;

                carts.Add(curCart);
            }

            //删除无效购物车记录
            DishShoppingCartBLL.SingleModel.DeleteShoppingCart(store.aid, store.id, userInfo.Id);

            //检查库存
            if (!DishOrderBLL.SingleModel.CheckStock(carts))
            {
                result.info = "购物车内存在库存不足的菜品,请到重新下单";
                return Json(result);
            }

            //添加购物车
            bool isSuccess = DishShoppingCartBLL.SingleModel.BatchAddGoodCart(carts);

            result.code = 1;
            result.info = "添加购物车成功";
            return Json(result);
        }

        /// <summary>
        /// 2.获取购物车
        /// </summary>
        /// <param name="cdata"></param>
        /// <param name="utoken"></param>
        /// <param name="dish_id"></param>
        /// <returns></returns>
        public JsonResult GetCartList(string utoken = "", int dish_id = 0, int orderId = 0)
        {
            if (string.IsNullOrEmpty(utoken) || dish_id <= 0)
            {
                result.info = "参数错误";
                return Json(result);
            }
            DishStore store = DishStoreBLL.SingleModel.GetModel(dish_id);
            if (store == null)
            {
                result.info = "店铺不存在";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }

            try { store.gaojiConfig = JsonConvert.DeserializeObject<DishGaojiConfig>(store.gaojiConfigJson) ?? new DishGaojiConfig(); } catch (Exception) { store.gaojiConfig = new DishGaojiConfig(); } //读取store配置
            List<DishShoppingCart> carts = DishShoppingCartBLL.SingleModel.GetShoppingCart(store.aid, store.id, userInfo.Id, orderId) ?? new List<DishShoppingCart>();
            List<DishGood> goods = DishGoodBLL.SingleModel.GetGoodsByIds(carts.Select(c => c.goods_id)?.ToArray());//购物车内商品相关资料

            //商品基本费用
            double all_g_dabao_price = carts.Sum(c => goods.FirstOrDefault(g => g.id == c.goods_id).dabao_price * c.goods_number); //打包费总计
            double all_g_number = carts.Sum(c => c.goods_number); //订单商品总数量
            double all_g_price = carts.Sum(c => c.goods_price * c.goods_number); //商品总价格
            double all_g_yunfei = 0; //前端无用,暂传0

            //满减活动
            string is_huodong_mianjian_info = string.Empty; //满减活动文案
            double is_huodong_mianjian_jiner = 0.00; //活动减免金额
            int is_huodong_mianjian_status = 0; //满减活动开关
            int is_huodong_mianjian_id = 0;
            //根据消费的<商品金额>查询
            DishActivity activity = DishActivityBLL.SingleModel.GetMoneyMeetActivity_manjian(store.aid, store.id, carts.Sum(c => c.goods_price * c.goods_number));
            if (activity != null)
            {
                is_huodong_mianjian_info = activity.q_shuoming;
                is_huodong_mianjian_jiner = activity.q_diyong_jiner;
                is_huodong_mianjian_status = 1;
                is_huodong_mianjian_id = activity.id;
            }

            //首单立减
            string is_huodong_shou_info = $"首单立减{store.gaojiConfig.huodong_shou_jiner}元"; //首单立减文案
            double is_huodong_shou_jiner = DishOrderBLL.SingleModel.IsFristOrder(userInfo.Id, store.id, 0) ? store.gaojiConfig.huodong_shou_jiner : 0.00d; //首单立减金额
            int is_huodong_shou_status = store.gaojiConfig.huodong_shou_isopen; //首单立减开关
            int is_huodong_shou_id = 0; //首单立减Id -不存在的<为了配合前端保留>

            //购物车商品资料拼接
            Dictionary<int, object> dicCart = new Dictionary<int, object>();
            carts.Where(c => c.order_id == 0)?.ToList().ForEach(c =>
            {
                dicCart.Add(c.id, new
                {
                    attr_name = string.Empty,
                    dish_id = c.storeId,
                    goods_attr = c.goods_attr,
                    goods_attr_id = c.goods_attr_id,
                    goods_id = c.goods_id,
                    goods_img = c.goods_img,
                    goods_name = c.goods_name,
                    goods_number = c.goods_number,
                    goods_price = c.goods_price,
                    id = c.id,
                    is_checked = false,
                    table_id = 0,
                    user_id = c.user_id,
                });
            });
            string glist = JsonConvert.SerializeObject(dicCart);

            result.code = 1;
            result.info = new
            {
                all_g_dabao_price = Math.Round(all_g_dabao_price, 2),
                all_g_number = all_g_number,
                all_g_price = all_g_price,
                all_g_yunfei = all_g_yunfei,

                is_huodong_mianjian_info = is_huodong_mianjian_info,
                is_huodong_mianjian_jiner = is_huodong_mianjian_jiner,
                is_huodong_mianjian_status = is_huodong_mianjian_status,
                is_huodong_mianjian_id = is_huodong_mianjian_id,

                is_huodong_shou_info = is_huodong_shou_info,
                is_huodong_shou_jiner = is_huodong_shou_jiner,
                is_huodong_shou_status = is_huodong_shou_status,
                is_huodong_shou_id = is_huodong_shou_id,

                glist = glist
            };
            return Json(result);
        }

        /// <summary>
        /// 3.下单订单请求页参数
        /// </summary>
        /// <param name="dish_id"></param>
        /// <param name="order_type"></param>
        /// <param name="is_ziqu"></param>
        /// <param name="utoken"></param>
        /// <returns></returns>
        public JsonResult GetOrderDishInfo(int dish_id = 0, int order_type = 0, int is_ziqu = 0, string utoken = "")
        {
            if (dish_id <= 0 || utoken == "" || order_type <= 0)
            {
                result.info = "订单内容异常";
                return Json(result);
            }
            DishStore store = DishStoreBLL.SingleModel.GetModel(dish_id);
            if (store == null)
            {
                result.info = "店铺不存在";
                return Json(result);
            }

            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }

            //支付方式集合
            Dictionary<int, string> pay_typelist = new Dictionary<int, string>();
            if (store.paySetting.pay_weixin_isopen == 1) pay_typelist.Add(1, DishEnums.PayMode.微信支付.ToString());
            if (order_type == (int)DishEnums.OrderType.店内 && is_ziqu == 0)
            {
                if (store.paySetting.pay_xianjin_isopen == 1) pay_typelist.Add(2, DishEnums.PayMode.线下支付.ToString());
                if (store.paySetting.pay_daofu_isopen == 1) pay_typelist.Add(3, DishEnums.PayMode.货到支付.ToString());
            }
            if (store.paySetting.pay_yuer_isopen == 1) pay_typelist.Add(4, DishEnums.PayMode.余额支付.ToString());
            string pay_typelist_str = JsonConvert.SerializeObject(pay_typelist);

            //会员卡资料
            DishVipCard vipCard = DishVipCardBLL.SingleModel.GetVipCardByStoreId_UId(store.id, userInfo.Id);
            //桌台集合
            List<DishTable> dish_table_list = DishTableBLL.SingleModel.GetTableByParams(store.aid, store.id, true);

            //格式化日期选择列表
            Func<int, object> day_data = (day) =>
            {
                List<string> day_strs = new List<string>();
                for (int i = 0; i <= day; i++) day_strs.Add(DateTime.Now.AddDays(i).ToString("MM-dd"));

                return day_strs.Select(d => new
                {
                    day_title = d,
                    is_select = false,
                });
            };

            result.code = 1;
            result.info = new
            {
                dish_info = new
                {
                    dish_address = store.baseConfig.dish_address,
                    dish_all_pingfen = "5",
                    dish_all_xiaoliang = "0",
                    dish_beizhu_info = "",
                    dish_beizhu_text = "",
                    dish_con_mobile = store.baseConfig.dish_con_phone, //联系商家电话
                    dish_con_phone = "",
                    dish_diannei_fangshi = store.dianneiConfig.dish_diannei_fangshi,
                    dish_diannei_text = "店内",
                    dish_diannei_tips_one = "下单付款后，订单才能下送后厨",
                    dish_diannei_tips_show = store.dianneiConfig.dish_diannei_tips_show, //订单流程提醒显隐，默认1
                    dish_diannei_tips_two = store.dianneiConfig.dish_diannei_fangshi == 1 ? store.dianneiConfig.dish_diannei_tips_one : store.dianneiConfig.dish_diannei_tips_two, //订单流程文案
                    dish_fuwu = "WIFI,唱k",
                    dish_gonggao = "各类新鲜水果，不要钱！不要钱！！",
                    dish_gps_lat = store.ws_lat, //餐饮单店地址经度
                    dish_gps_lng = store.ws_lng,//餐饮单店地址纬度
                    dish_id = store.id, //餐饮单店店铺id
                    dish_is_diannei = "1",
                    dish_is_fapiao = store.gaojiConfig.dish_is_fapiao,
                    dish_is_open_tomini = "0",
                    dish_is_paidui = "1",
                    dish_is_rcode_open = "0",
                    dish_is_sms_check = "1",
                    dish_is_waimai = "1",
                    dish_is_webview_open = "0",
                    dish_is_yuding = "1",
                    dish_is_zhuohao_change = store.dianneiConfig.dish_is_zhuohao_change,
                    dish_is_ziqu = "1",
                    dish_is_zntuijian = "0",
                    dish_jieshao = "不用钱的店",
                    dish_logo = store.dish_logo, //餐饮单店店铺logo
                    dish_name = store.dish_name, //餐饮单店店铺名称
                    dish_open_status = "1",
                    dish_paidui_text = "排队",
                    dish_pay_limit_time = "10",
                    dish_pingjunxiaofei = "0.00",
                    dish_quyu = "广州东站对面建国酒店街边",
                    dish_shijing = "",
                    dish_show_status = "1",
                    dish_tomini_appid = "",
                    dish_tomini_apptext = "",
                    dish_tomini_appurl = "",
                    dish_waimai_text = "外卖",
                    dish_webview_text = "介绍",
                    dish_webview_url = "",
                    dish_yingye_btime = "0",
                    dish_yingye_etime = "24",
                    dish_yingye_time = "",
                    dish_yingye_waimai_time = "",
                    dish_yuding_gonggao = "",
                    dish_yuding_text = "预订",
                    dish_ziqu_day = "2",
                    dish_ziqu_text = "自取",
                    dish_zizhi = "",
                    dish_zong_xiaoliang = "0",
                    dish_zuijin_xiaoliang = "0",
                    id = 0, //订单id（我前端看到的是用它来请求订单详情和请求支付）
                    is_delete = "1",
                    is_yingye_status = 1,
                    update_time = "1523969347",
                    user_card_account = vipCard?.account_balance ?? 0.00,//余额
                    waimai_limit_jiner = "0",
                    waimai_limit_juli = "0",
                    waimai_peisong_base_juli = "0",
                    waimai_peisong_base_step = "0",
                    waimai_peisong_jiner = "0",
                    waimai_peisong_jiner_guize = "",
                    waimai_peisong_jiner_hou = "0",
                    waimai_peisong_time_jiedian = "",
                    dish_ziqu_day_data = day_data(store.dianneiConfig.dish_ziqu_day),
                    pay_typelist = pay_typelist_str, //支付方式集合

                    dish_ziqu_time = store.dianneiConfig.zq_time, //自取方式集合
                },
                dish_table_list = dish_table_list, //餐桌号集合（前端无使用）
            };
            return Json(result);
        }

        /// <summary>
        /// 4.下单
        /// </summary>
        /// <param name="cdata"></param>
        /// <param name="utoken"></param>
        /// <param name="dish_id"></param>
        /// <returns></returns>
        public JsonResult PostOrder(int dish_id = 0, string utoken = "", string oinfo = null)
        {
            #region 1.传入数据完整性的基本验证

            string errorMsg = string.Empty;//错误信息
            if (string.IsNullOrWhiteSpace(oinfo) || dish_id <= 0 || utoken == "")
            {
                result.info = "订单内容异常";
                return Json(result);
            }

            PostOrderInfo orderModel = null;
            try { orderModel = JsonConvert.DeserializeObject<PostOrderInfo>(oinfo); }
            catch (Exception) { result.info = "订单内容错误"; return Json(result); }

            //店铺验证
            DishStore store = DishStoreBLL.SingleModel.GetModel(dish_id);
            if (store == null)
            {
                result.info = "店铺不存在";
                return Json(result);
            }
            if (store.baseConfig.dish_open_status != 1) //门店是否可以下单
            {
                result.info = "门店未开启";
                return Json(result);
            }

            //关联小程序验证
            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModel(store.aid);
            if (xcxrelation == null || string.IsNullOrEmpty(xcxrelation.AppId))
            {
                result.info = "小程序已过期";
                return Json(result);
            }

            //用户验证
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }
            DishActivityUser quan = null;
            CouponLog reductionCard = null;
            if (orderModel.quan_id > 0) {
                //判断是否立减金，立减金查couponlog表
                if (orderModel.isReductionCard)
                {
                    reductionCard = CouponLogBLL.SingleModel.GetModel(orderModel.quan_id);
                    if (reductionCard == null|| reductionCard.State!=0)
                    {
                        result.info = "优惠券无效";
                        return Json(result);
                    }
                }
                else
                {
                    //优惠券验证
                    quan = DishActivityUserBLL.SingleModel.GetModel(orderModel.quan_id);
                    if (quan == null)
                    {
                        result.info = "优惠券无效";
                        return Json(result);
                    }
                }
            }
            DishOrder order = DishOrderBLL.SingleModel.GetModel(orderModel.order_id) ?? new DishOrder();//订单
            if (order?.id > 0 && order?.pay_status != (int)DishEnums.PayState.未付款)
            {
                result.info = "订单已结单,无法生成订单";
                return Json(result);
            }
            List<DishShoppingCart> carts = DishShoppingCartBLL.SingleModel.GetShoppingCart(store.aid, store.id, userInfo.Id, 0) ?? new List<DishShoppingCart>();//购物车
            List<DishShoppingCart> curNewCarts = carts.ToList();//此次订单新加的购物车

            List<DishGood> goods = DishGoodBLL.SingleModel.GetGoodsByIds(carts.Select(c => c.goods_id)?.ToArray()); //购物车内商品详细资料
            if (goods == null || carts.Any(c => !goods.Any(g => c.goods_id == g.id)))
            {
                result.info = "菜品信息异常";
                return Json(result);
            }

            #endregion 1.传入数据完整性的基本验证

            #region 2.建立lock元素并锁定当前代码块执行填充订单及操作数据库的操作

            //添加唯一锁
            string lockkey = order.aId + "_" + (int)TmpType.智慧餐厅;
            if (!_lockObjectDictOrder.ContainsKey(lockkey))
            {
                if (!_lockObjectDictOrder.TryAdd(lockkey, new object()))
                {
                    result.info = "系统繁忙,请稍候再试！";
                    return Json(returnObj);
                }
            }
            lock (_lockObjectDictOrder[lockkey])
            {
                #region a.针对不同订单类型的验证及填充订单操作

                //不同订单类型,走不同填充流程
                switch (orderModel.order_type)
                {
                    case (int)DishEnums.OrderType.店内:
                        if (orderModel.is_ziqu == 0) //店内
                        {
                            DishOrderBLL.SingleModel.PerfectionOrder_DianNei(orderModel, order, carts, store, result, userInfo);
                        }
                        else  //店内自取
                        {
                            DishOrderBLL.SingleModel.PerfectionOrder_DianNeiZiQu(orderModel, order, carts, goods, store, result, userInfo);
                        }
                        break;

                    case (int)DishEnums.OrderType.外卖:
                        DishOrderBLL.SingleModel.PerfectionOrder_WaiMai(orderModel, order, carts, goods, store, result, userInfo, xcxrelation);
                        break;

                    default:
                        result.info = "不支持此种订单类型";
                        return Json(result);
                }
                if (result.info != null) return Json(result); //校验填充订单过程是否有问题,若已提示数据异常则 return

                #endregion a.针对不同订单类型的验证及填充订单操作

                #region b.检测库存并插入数据库

                //检查库存
                if (!DishOrderBLL.SingleModel.CheckStock(carts))
                {
                    result.info = "购物车内存在库存不足的菜品,请到重新下单";
                    return Json(result);
                }
                //生成订单
                if (!DishOrderBLL.SingleModel.CreateOrder(order, carts, quan, reductionCard))
                {
                    result.info = "下单火爆,请稍候再试";
                    return Json(result);
                }

                #endregion b.检测库存并插入数据库

                //拿到这次生成的订单ID
                order.id = DishShoppingCartBLL.SingleModel.GetModel(carts[0].id).order_id;
            }

            #endregion 2.建立lock元素并锁定当前代码块执行填充订单及操作数据库的操作

            //下单时打印的打印机打印订单
            PrinterHelper.DishPrintOrderByPrintType(order, store, curNewCarts, 1);

            result.code = 1;
            result.info = order.id;
            return Json(result);
        }

        /// <summary>
        /// 5.订单详情
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="utoken"></param>
        /// <returns></returns>
        public JsonResult OrderInfo(int oid = 0, string utoken = "")
        {
            if (oid <= 0 || string.IsNullOrWhiteSpace(utoken))
            {
                result.info = "参数错误";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }
            DishOrder order = DishOrderBLL.SingleModel.GetModel(oid);
            if (order == null)
            {
                result.info = "订单资料错误";
                return Json(result);
            }
            DishStore store = DishStoreBLL.SingleModel.GetModel(order.storeId);
            if (store == null)
            {
                result.info = "店铺资料错误";
                return Json(result);
            }
            order.carts = DishShoppingCartBLL.SingleModel.GetCartsByOrderId(order.id) ?? new List<DishShoppingCart>();

            result.code = 1;
            result.info = new
            {
                info = new
                {
                    add_time = order.add_time,
                    address = order.address,
                    area = order.area,
                    city = order.city,
                    confirm_time = order.confirm_time,
                    consignee = order.consignee,
                    country = order.country,
                    coupon_type = "",
                    ctime = order.ctime,
                    dabao_fee = order.dabao_fee,//餐盒费用
                    dish_id = order.storeId,
                    dish_logo = store.dish_logo,
                    dish_name = store.dish_name,
                    dish_quyu = store.baseConfig.dish_address,
                    email = "",
                    express_code = "",
                    express_name = "",
                    fapiao_text = order.is_fapiao == 1 ? $"{order.fapiao_text}({order.fapiao_leixing_txt})" : order.fapiao_text,
                    goods_amount = order.goods_amount,
                    huodong_manjin_jiner = order.huodong_manjin_jiner,
                    huodong_quan_id = order.huodong_quan_id,
                    huodong_quan_jiner = order.huodong_quan_jiner,
                    huodong_shou_jiner = order.huodong_shou_jiner,
                    id = order.id,
                    is_auto_cash = order.is_auto_cash,
                    is_comment = order.is_comment,
                    is_delete = order.is_delete,
                    is_fapiao = order.is_fapiao,
                    is_tongzhi = order.is_tongzhi,
                    is_ziqu = order.is_ziqu,
                    mobile = order.mobile,
                    order_amount = order.order_amount,
                    order_amount_old = order.order_amount_old,
                    order_haoma = order.order_haoma,
                    order_jiucan_type = order.order_jiucan_type,
                    order_sn = order.order_sn,
                    order_status = order.order_status,
                    order_status_txt = order.pay_status_txt,
                    order_table_id = order.order_table_id,
                    order_table_id_zhen = order.order_table_id_zhen,
                    order_type = order.order_type,
                    pay_end_time = order.pay_end_time,
                    pay_end_time_text = order.pay_end_time_text,
                    pay_id = order.pay_id,
                    pay_name = order.pay_name,
                    pay_status = order.pay_status,
                    pay_time = order.pay_time,
                    peisong_amount = order.peisong_amount,
                    peisong_name = order.peisong_name,
                    peisong_open = order.peisong_open,
                    peisong_status = order.peisong_status,
                    peisong_status_text = order.peisong_status_text,
                    peisong_type = order.peisong_type,
                    order.PeiSongName,
                    peisong_user_name = order.peisong_user_name,
                    peisong_user_phone = order.peisong_user_phone,
                    post_info = order.post_info,
                    province = order.province,
                    settlement_total_fee = order.settlement_total_fee,
                    shipping_fee = order.shipping_fee,
                    shipping_id = order.shipping_id,
                    shipping_name = order.shipping_name,
                    shipping_status = order.shipping_status,
                    shipping_time = order.shipping_time,
                    u_lat = order.u_lat,
                    u_lng = order.u_lng,
                    user_id = order.user_id,
                    yongcan_renshu = order.yongcan_renshu,
                    zipcode = order.zipcode,
                    ziqu_time = order.ziqu_time,
                    ziqu_username = order.ziqu_username,
                    ziqu_userphone = order.ziqu_userphone,

                    order_action_list = new
                    {
                        cancel = order.order_status != (int)DishEnums.OrderState.已完成 && order.order_status != (int)DishEnums.OrderState.已取消 && order.pay_status == (int)DishEnums.PayState.未付款,//用于前端判断订单列表的‘取消’按钮显隐，true显示 false隐藏
                        pay = order.order_status != (int)DishEnums.OrderState.已取消 && order.pay_status == (int)DishEnums.PayState.未付款, //用于前端判断订单列表的‘支付’按钮显隐，true显示 false隐藏
                        comment = (order.pay_status == (int)DishEnums.PayState.已付款 && order.is_comment == 0), //用于前端判断订单列表的‘去评价’按钮显隐，true显示 false隐藏
                        yes_comment = order.is_comment == 1,//用于前端判断订单列表的‘已评价’按钮显隐，true显示 false隐藏
                    },

                    glist = order.carts,//购物车详情
                }
            };
            return Json(result);
        }

        /// <summary>
        /// 6.订单支付
        /// </summary>
        /// <param name="pay_type"></param>
        /// <param name="order_id"></param>
        /// <param name="utoken"></param>
        /// <returns></returns>
        public ActionResult OrderGoPay(int pay_type = 0, int order_id = 0, string utoken = "")
        {
            result.code = 5; //前端要求此接口失败code返回5
            string msg = null; //error string

            if (pay_type <= 0 || order_id <= 0 || string.IsNullOrWhiteSpace(utoken))
            {
                result.info = "参数错误";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }
            DishOrder order = DishOrderBLL.SingleModel.GetModel(order_id);
            if (order == null)
            {
                result.info = "订单资料错误";
                return Json(result);
            }
            if (order.order_status == (int)DishEnums.OrderState.已取消 || order.order_status == (int)DishEnums.OrderState.已完成)
            {
                result.info = "对不起，该订单已完成或已取消";
                return Json(result);
            }
            if (order.order_jiucan_type != 2 && order.pay_end_time <= DateTime.Now)
            {
                result.info = "订单已超过规定的付款时间,不可付款";
                return Json(result);
            }

            order.pay_id = pay_type;
            //【获取立减金，没有的话就传null】
            Coupons reduction = CouponsBLL.SingleModel.GetVailtModel(order.aId,order.storeId);
            switch (order.pay_id)
            {
                case (int)DishEnums.PayMode.微信支付:
                    if (order.settlement_total_fee > 0) //若金额为0,则为免费订单
                    {
                        if (order.cityMordersId <= 0)//接口会可能重复调用,若未生成过微信订单才去生成
                        {
                            order.cityMordersId = DishOrderBLL.SingleModel.CreateCityOrder(order);
                            if (order.cityMordersId <= 0)
                            {
                                result.info = "生成微信订单失败";
                                return Json(result);
                            }
                            order.cityMordersId = order.cityMordersId;
                            if (!DishOrderBLL.SingleModel.Update(order, "pay_id,cityMordersId"))
                            {
                                result.info = "关联订单失败";
                                return Json(result);
                            }
                        }
                        result.code = 11;
                        result.info = result.info = new
                        {
                            reduction,
                            order_id = order.id,
                            cityMorderId = order.cityMordersId,
                        };
                        return Json(result);
                    }
                    else //免费订单
                    {
                        //执行改变订单状态
                        if (!DishOrderBLL.SingleModel.PayOrderDisposeDB(order, ref msg))
                        {
                            result.info = msg ?? "支付失败";
                            return Json(result);
                        }

                        DishOrderBLL.SingleModel.AfterPayOrderOperation(order);
                        result.code = 9;
                        result.info = new
                        {
                            order_id = order.id = order.id,
                        };
                        return Json(result);
                    }

                case (int)DishEnums.PayMode.余额支付:
                case (int)DishEnums.PayMode.线下支付:
                case (int)DishEnums.PayMode.货到支付:
                    //执行改变订单状态
                    if (!DishOrderBLL.SingleModel.PayOrderDisposeDB(order, ref msg))
                    {
                        result.info = msg ?? "支付失败";
                        return Json(result);
                    }

                    DishOrderBLL.SingleModel.AfterPayOrderOperation(order);
                    result.code = 9;
                    result.info = new
                    {
                        reduction,
                        order_id = order.id = order.id,
                    };
                    return Json(result);

                default:
                    result.info = "暂不支持该支付方式";
                    return Json(result);
            }
        }

        #endregion 下单流程 - 相关接口

        /// <summary>
        /// 门店直接买单
        /// </summary>
        /// <param name="pay_type"></param>
        /// <param name="order_id"></param>
        /// <param name="utoken"></param>
        /// <returns></returns>
        public ActionResult PayOrder(double money = 0.00d, int pay_name = 0, string beizhu = "", int dish_id = 0, string utoken = "", int aid = 0, int activityId = 0)
        {
            result.code = 5;//失败默认5

            if (money <= 0.00d || pay_name <= 0 || dish_id <= 0 || string.IsNullOrWhiteSpace(utoken) || aid <= 0)
            {
                result.info = "参数错误";
                return Json(result);
            }

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcx == null)
            {
                result.info = "授权资料错误";
                return Json(result);
            }

            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }
            DishStore store = DishStoreBLL.SingleModel.GetModel(dish_id);
            if (store == null)
            {
                result.info = "店铺资料错误";
                return Json(result);
            }

            //优惠券验证
            DishActivityUser quan = DishActivityUserBLL.SingleModel.GetModel(activityId);
            if (quan == null && activityId > 0)
            {
                result.info = "优惠券无效";
                return Json(result);
            }
            if (quan != null&&quan.quan_limit_jiner > money)
            {
                result.info = "优惠券不可用";
                return Json(result);
            }
            double cash = quan == null ? money : money - quan.quan_jiner;//实付金额
            double discount = quan == null ? 0 : quan.quan_jiner;//优惠金额
            switch (pay_name)
            {
                case (int)DishEnums.PayMode.微信支付:
                    var obj = new
                    {
                        price = money,//原价
                        money = cash,//实付金额
                        discount,//优惠金额
                        activityId = quan == null ? 0 : quan.id,
                    };
                    string no = WxPayApi.GenerateOutTradeNo();
                    CityMorders citymorderModel = new CityMorders
                    {
                        OrderType = (int)ArticleTypeEnum.DishStorePayTheBill,
                        ActionType = (int)ArticleTypeEnum.DishStorePayTheBill,
                        Addtime = DateTime.Now,
                        payment_free = Convert.ToInt32(cash * 100),
                        trade_no = no,
                        Percent = 99,//不收取服务费
                        userip = WebHelper.GetIP(),
                        FuserId = userInfo.Id,
                        Fusername = userInfo.NickName,
                        orderno = no,
                        payment_status = 0,
                        Status = 0,
                        Articleid = 0,
                        CommentId = store.id,//订单stroreId
                        MinisnsId = aid,// 订单aId
                        TuserId = userInfo.Id,
                        ShowNote = $@" {xcx.Title}门店买单支付{cash}元",
                        CitySubId = 0,//无分销,默认为0
                        PayRate = 1,
                        buy_num = 0, //无
                        appid = xcx.AppId,
                        remark = beizhu,
                        AttachPar = JsonConvert.SerializeObject(obj)
                    };
                    int cityMorderId = Convert.ToInt32(new CityMordersBLL().Add(citymorderModel));

                    if (cityMorderId <= 0)
                    {
                        result.info = "生成微信订单失败";
                        return Json(result);
                    }

                    result.code = 11;
                    result.info = new
                    {
                        cityMorderId = cityMorderId
                    };
                    return Json(result);

                case (int)DishEnums.PayMode.余额支付:
                    string msg = string.Empty;
                    if (!DishVipCardBLL.SingleModel.PayOrderByBalance(aid, store.id, userInfo.Id, cash, $"门店买单,支付￥{cash}元,备注:{beizhu}", ref msg))
                    {
                        result.info = msg;
                        return Json(result);
                    }
                    TemplateMsg_Gzh.SendOrderSuccessTemplateMessage(userInfo.NickName,cash, store.id, DateTime.Now, "余额支付",money,discount);
                    if (quan != null)
                    {
                        quan.quan_status = 1;
                        DishActivityUserBLL.SingleModel.Update(quan, "quan_status");
                    }
                    result.code = 9;
                    result.info = "支付成功";
                    return Json(result);

                default:
                    result.info = "暂不支持该支付方式";
                    return Json(result);
            }
        }

        /// <summary>
        /// 获取用户订单记录
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="pagesize"></param>
        /// <param name="pagenum"></param>
        /// <param name="order_status"></param>
        /// <returns></returns>
        public JsonResult GetUserOrderList(string utoken = "", int pagesize = 1, int pagenum = 1, int order_status = -1, int dish_id = 0)
        {
            if (string.IsNullOrWhiteSpace(utoken))
            {
                result.info = "参数错误";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }

            List<DishOrder> orders = DishOrderBLL.SingleModel.GetUserOrders(storeId: dish_id, userId: userInfo.Id, pageSize: pagesize, pageIndex: pagenum, order_status: order_status);
            if (orders == null || orders.Count == 0)
            {
                result.code = 1;
                result.info = new { info = new { } };
                return Json(result);
            }
            orders.ForEach(o =>
            {
                o.carts = DishShoppingCartBLL.SingleModel.GetCartsByOrderId(o.id) ?? new List<DishShoppingCart>();
            });

            List<DishStore> store = DishStoreBLL.SingleModel.GetStoreByIds(orders.Select(o => o.storeId)?.ToArray());

            result.code = 1;
            result.info = new
            {
                info = orders.Select(order => new
                {
                    store_openstaus = store?.FirstOrDefault(s => s.id == order.storeId)?.state == 1, //门店是否开启状态

                    add_time = order.add_time,
                    address = order.address,
                    area = order.area,
                    city = order.city,
                    confirm_time = order.confirm_time,
                    consignee = order.consignee,
                    country = order.country,
                    coupon_type = "",
                    ctime = order.ctime,
                    dabao_fee = order.dabao_fee,//餐盒费用
                    dish_id = order.storeId,
                    dish_logo = store?.FirstOrDefault(s => s.id == order.storeId)?.dish_logo,
                    dish_name = store?.FirstOrDefault(s => s.id == order.storeId)?.dish_name,
                    dish_quyu = store?.FirstOrDefault(s => s.id == order.storeId)?.baseConfig.dish_address,
                    email = "",
                    express_code = "",
                    express_name = "",
                    fapiao_text = order.is_fapiao == 1 ? $"{order.fapiao_text}({order.fapiao_leixing_txt})" : order.fapiao_text,
                    goods_amount = order.goods_amount,
                    huodong_manjin_jiner = order.huodong_manjin_jiner,
                    huodong_quan_id = order.huodong_quan_id,
                    huodong_quan_jiner = order.huodong_quan_jiner,
                    huodong_shou_jiner = order.huodong_shou_jiner,
                    id = order.id,
                    is_auto_cash = order.is_auto_cash,
                    is_comment = order.is_comment,
                    is_delete = order.is_delete,
                    is_fapiao = order.is_fapiao,
                    is_tongzhi = order.is_tongzhi,
                    is_ziqu = order.is_ziqu,
                    mobile = order.mobile,
                    order_amount = order.order_amount,
                    order_amount_old = order.order_amount_old,
                    order_haoma = order.order_haoma,
                    order_jiucan_type = order.order_jiucan_type,
                    order_sn = order.order_sn,
                    order_status = order.order_status,
                    order_status_txt = order.pay_status_txt,
                    order_table_id = order.order_table_id,
                    order_table_id_zhen = order.order_table_id_zhen,
                    order_type = order.order_type,
                    pay_end_time = order.pay_end_time,
                    pay_end_time_text = order.pay_end_time_text,
                    pay_id = order.pay_id,
                    pay_name = order.pay_name,
                    pay_status = order.pay_status,
                    pay_time = order.pay_time,
                    peisong_amount = order.peisong_amount,
                    peisong_name = order.peisong_name,
                    peisong_open = order.peisong_open,
                    peisong_status = order.peisong_status,
                    peisong_status_text = order.peisong_status_text,
                    peisong_type = order.peisong_type,
                    peisong_user_name = order.peisong_user_name,
                    peisong_user_phone = order.peisong_user_phone,
                    post_info = order.post_info,
                    province = order.province,
                    settlement_total_fee = order.settlement_total_fee,
                    shipping_fee = order.shipping_fee,
                    shipping_id = order.shipping_id,
                    shipping_name = order.shipping_name,
                    shipping_status = order.shipping_status,
                    shipping_time = order.shipping_time,
                    u_lat = order.u_lat,
                    u_lng = order.u_lng,
                    user_id = order.user_id,
                    yongcan_renshu = order.yongcan_renshu,
                    zipcode = order.zipcode,
                    ziqu_time = order.ziqu_time,
                    ziqu_username = order.ziqu_username,
                    ziqu_userphone = order.ziqu_userphone,

                    order_action_list = new
                    {
                        cancel = order.order_status != (int)DishEnums.OrderState.已完成 && order.order_status != (int)DishEnums.OrderState.已取消 && order.pay_status == (int)DishEnums.PayState.未付款,//用于前端判断订单列表的‘取消’按钮显隐，true显示 false隐藏
                        pay = order.order_status != (int)DishEnums.OrderState.已取消 && order.pay_status == (int)DishEnums.PayState.未付款, //用于前端判断订单列表的‘支付’按钮显隐，true显示 false隐藏
                        comment = (order.pay_status == (int)DishEnums.PayState.已付款 && order.is_comment == 0), //用于前端判断订单列表的‘去评价’按钮显隐，true显示 false隐藏
                        yes_comment = order.is_comment == 1,//用于前端判断订单列表的‘已评价’按钮显隐，true显示 false隐藏
                    },

                    glist = order.carts,//购物车详情
                })
            };

            return Json(result);
        }

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="pagesize"></param>
        /// <param name="pagenum"></param>
        /// <param name="order_status"></param>
        /// <returns></returns>
        public JsonResult DeleteUserOrder(string utoken = "", int oid = 0)
        {
            if (string.IsNullOrWhiteSpace(utoken))
            {
                result.info = "参数错误";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }

            DishOrder order = DishOrderBLL.SingleModel.GetModel(oid);
            if (order == null)
            {
                result.info = "订单资料错误";
                return Json(result);
            }

            order.order_status = (int)DishEnums.OrderState.已取消;
            bool isSuccess = DishOrderBLL.SingleModel.Update(order, "order_status");

            if (!isSuccess)
            {
                result.info = "删除订单失败";
                return Json(result);
            }
            else
            {
                result.code = 1;
                result.info = "删除订单成功";
                return Json(result);
            }
        }

        /// <summary>
        /// 判定门店是否在配送范围内
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="oid"></param>
        /// <returns></returns>
        public JsonResult checkPeisongAddressLimitByGps(double u_lat = 0, double u_lng = 0, int dish_id = 0, string utoken = "")
        {
            if (string.IsNullOrWhiteSpace(utoken) || dish_id <= 0)
            {
                result.code = 5;
                result.info = "参数错误";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.code = 5;
                result.info = "用户信息错误";
                return Json(result);
            }

            DishStore store = DishStoreBLL.SingleModel.GetModel(dish_id);
            if (store == null)
            {
                result.code = 5;
                result.info = "门店资料错误";
                return Json(result);
            }

            string address = Context.GetRequest("address", "");
            string acceptername = Context.GetRequest("acceptername", "");
            string accepterphone = Context.GetRequest("accepterphone", "");
            string appid = Context.GetRequest("appid", "");
            string openid = Context.GetRequest("openid", "");
            string cityname = "";
            //if (store.ps_type == (int)miniAppOrderGetWay.达达配送 || store.ps_type == (int)miniAppOrderGetWay.快跑者配送)
            //{
            //    AddressApi addressinfo = AddressHelper.GetAddressByApi(u_lng.ToString(), u_lat.ToString());
            //    if (addressinfo == null || addressinfo.result == null || addressinfo.result.address_component == null)
            //    {
            //        result.code = 5;
            //        result.info = addressinfo;// "获取地址繁忙";
            //        return Json(result);
            //    }
            //    cityname = addressinfo.result.address_component.city.Replace("市", "");
            //    string msg = "";
            //    int peisonprice = 0;
            //    peisonprice = _distributionApiConfigBLL.Getpeisongfei(cityname, appid, openid, u_lat.ToString(), u_lng.ToString(), acceptername, accepterphone, address, ref msg, store.ps_type, store.id, store.aid);
            //    result.info = peisonprice * 0.01;
            //    if (!string.IsNullOrEmpty(msg))
            //    {
            //        result.code = 5;
            //        result.info = msg;
            //        return Json(result);
            //    }
            //}
            //else
            //{
            //    string errMsg = string.Empty;
            //    double peisong_money = DishOrderBLL.SingleModel.GetOrderShipping_fee(store, 999999999.99, u_lat, u_lng, ref errMsg,appid,openid,acceptername,accepterphone,address,cityname);
            //    if (!string.IsNullOrWhiteSpace(errMsg))
            //    {
            //        result.code = 5;
            //        result.info = errMsg;
            //        return Json(result);
            //    }
            //    result.info = peisong_money;
            //}

            if (store.ps_type == (int)miniAppOrderGetWay.达达配送 || store.ps_type == (int)miniAppOrderGetWay.快跑者配送)
            {
                AddressApi addressinfo = AddressHelper.GetAddressByApi(u_lng.ToString(), u_lat.ToString());
                if (addressinfo == null || addressinfo.result == null || addressinfo.result.address_component == null)
                {
                    result.code = 5;
                    result.info = addressinfo;// "获取地址繁忙";
                    return Json(result);
                }
                cityname = addressinfo.result.address_component.city.Replace("市", "");
            }
            string errMsg = string.Empty;
            double peisong_money = DishOrderBLL.SingleModel.GetOrderShipping_fee(store, 999999999.99, u_lat, u_lng, ref errMsg, appid, openid, acceptername, accepterphone, address, cityname);
            if (!string.IsNullOrWhiteSpace(errMsg))
            {
                result.code = 5;
                result.info = errMsg;
                return Json(result);
            }
            result.info = peisong_money;
            result.code = 1;
            return Json(result);
        }

        /// <summary>
        /// 获取当前排队数据
        /// </summary>
        /// <param name="dish_id"></param>
        /// <param name="utoken"></param>
        /// <returns></returns>
        public JsonResult getQueueList(int dish_id = 0, string utoken = "")
        {
            if (string.IsNullOrWhiteSpace(utoken) || dish_id <= 0)
            {
                result.code = 5;
                result.info = "参数错误";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.code = 5;
                result.info = "用户信息错误";
                return Json(result);
            }

            DishStore store = DishStoreBLL.SingleModel.GetModel(dish_id);
            if (store == null)
            {
                result.code = 5;
                result.info = "门店资料错误";
                return Json(result);
            }

            List<DishQueue> queues = DishQueueBLL.SingleModel.GetList($"aid ={store.aid} and storeId ={store.id} and state=1", 100, 1, "*", "q_order desc");
            queues.ForEach(q =>
            {
                q.pd_count = DishQueueUpBLL.SingleModel.GetCountByParams(store.aid, store.id, q.id);
                q.pd_this_num = DishQueueUpBLL.SingleModel.GetCurQueueUp(store.aid, store.id, q.id)?.q_haoma ?? "无";
            });

            DishQueueUp queueUp = DishQueueUpBLL.SingleModel.GetUserQueueUp(store.aid, store.id, userInfo.Id);

            result.code = 1;
            result.info = new
            {
                qinfo = queueUp,
                qlist = queues.Select(q => new
                {
                    dish_id = q.storeId,
                    q.id,
                    q.pd_count,
                    q.pd_this_num,
                    q.q_name,
                    q.q_order,
                    q.q_qianzhui,
                    q.q_renshu,
                    q_status = q.state,
                    q.q_tongzhi_renshu,
                    update_time = "",
                }),
            };
            return Json(result);
        }

        /// <summary>
        /// 取号
        /// </summary>
        /// <param name="dish_id"></param>
        /// <param name="utoken"></param>
        /// <returns></returns>
        public JsonResult actionQueue(int dish_id = 0, int pd_renshu = 0, string pd_phone = "", string utoken = "")
        {
            if (string.IsNullOrWhiteSpace(utoken) || dish_id <= 0)
            {
                result.code = 5;
                result.info = "参数错误";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.code = 5;
                result.info = "用户信息错误";
                return Json(result);
            }

            DishStore store = DishStoreBLL.SingleModel.GetModel(dish_id);
            if (store == null)
            {
                result.code = 5;
                result.info = "门店资料错误";
                return Json(result);
            }
            if (pd_renshu <= 0)
            {
                result.code = 5;
                result.info = "人数填写错误";
                return Json(result);
            }
            if (string.IsNullOrWhiteSpace(pd_phone))
            {
                result.code = 5;
                result.info = "手机号码填写错误";
                return Json(result);
            }

            string lockkey = store.aid + "_" + (int)TmpType.智慧餐厅 + "_paidui";
            if (!_lockObjectDictOrder.ContainsKey(lockkey))
            {
                if (!_lockObjectDictOrder.TryAdd(lockkey, new object()))
                {
                    result.code = 5;
                    result.info = "系统繁忙,请稍候再试！";
                    return Json(returnObj);
                }
            }
            lock (lockkey)
            {
                //去到哪个队列
                DishQueue joinQueue = null;
                List<DishQueue> queues = DishQueueBLL.SingleModel.GetList($"aid ={store.aid} and storeId ={store.id} and state=1", 100, 1, "*", "q_order desc");
                joinQueue = queues?.OrderBy(q => q.q_renshu).FirstOrDefault(q => q.q_renshu >= pd_renshu); //人数满足用餐人数且最接近于用餐人数的队列
                if (joinQueue == null)
                {
                    result.code = 5;
                    result.info = "门店队列无适用于填写人数的桌台";
                    return Json(result);
                }

                DishQueueUp queueUp = new DishQueueUp();
                queueUp.aId = store.aid;
                queueUp.storeId = store.id;
                queueUp.user_Id = userInfo.Id;

                queueUp.q_catid = joinQueue.id;
                queueUp.cate_name = joinQueue.q_name;
                queueUp.q_z_haoma = joinQueue.q_curnumber + 1;
                queueUp.q_haoma = joinQueue.q_qianzhui + queueUp.q_z_haoma;
                queueUp.q_renshu = pd_renshu;
                queueUp.q_phone = pd_phone;
                queueUp.q_addtime = DateTime.Now;

                if (!DishQueueUpBLL.SingleModel.AddQueueUp(queueUp))
                {
                    result.code = 5;
                    result.info = "排队场面火爆,稍候再试！";
                    return Json(returnObj);
                }
            }
            result.code = 1;
            result.info = "排队成功,请耐心等待";
            return Json(result);
        }

        /// <summary>
        /// 取消排队
        /// </summary>
        /// <param name="dish_id"></param>
        /// <param name="utoken"></param>
        /// <returns></returns>
        public JsonResult qxQueueInfo(int q_id = 0, string utoken = "")
        {
            if (string.IsNullOrWhiteSpace(utoken) || q_id <= 0)
            {
                result.info = "参数错误";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }

            DishQueueUp queueUp = DishQueueUpBLL.SingleModel.GetModel(q_id);
            if (queueUp == null)
            {
                result.info = "排队资料错误";
                return Json(result);
            }
            queueUp.state = (int)DishEnums.QueueUpEnums.已取消;
            if (DishQueueUpBLL.SingleModel.Update(queueUp, "state"))
            {
                result.code = 1;
                result.info = "取消排队成功";
                return Json(result);
            }
            else
            {
                result.code = 1;
                result.info = "取消排队失败";
                return Json(result);
            }
        }

        public JsonResult getRechargeConfig(int dish_id = 0, string utoken = "")
        {
            if (string.IsNullOrWhiteSpace(utoken) || dish_id <= 0)
            {
                result.info = "参数错误";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }
            DishVipCardSetting cardSetting = DishVipCardSettingBLL.SingleModel.GetModelByStoreId(dish_id);
            if (cardSetting == null) cardSetting = new DishVipCardSetting();
            cardSetting.rechargeConfig = JsonConvert.DeserializeObject<List<RechargeConfig>>(cardSetting.rechargeConfigJson);
            result.code = 1;
            result.info = cardSetting.rechargeConfig;
            return Json(result);
        }

        public JsonResult cardRecharge(int aid = 0, int shop_id = 0, string utoken = "", double rz_account = 0.00d)
        {
            if (string.IsNullOrWhiteSpace(utoken) || shop_id <= 0 || aid <= 0)
            {
                result.info = $"参数错误";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }
            DishVipCardSetting cardSetting = DishVipCardSettingBLL.SingleModel.GetModelByStoreId(shop_id);
            if (cardSetting == null) cardSetting = new DishVipCardSetting();
            cardSetting.rechargeConfig = JsonConvert.DeserializeObject<List<RechargeConfig>>(cardSetting.rechargeConfigJson);
            RechargeConfig recharge = cardSetting.rechargeConfig.Where(item => item.rc_man <= rz_account).OrderByDescending(item => item.rc_man).FirstOrDefault();

            DishCardAccountLog accountLog = new DishCardAccountLog()
            {
                shop_id = shop_id,
                account_info = $"充值",
                account_money = rz_account,
                user_id = userInfo.Id,
                aId = aid
            };
            if (cardSetting.rechargeConfig != null && cardSetting.rechargeConfig.Count > 0)
            {
                if (recharge != null)
                {
                    DishCardAccountLog songLog = new DishCardAccountLog()
                    {
                        shop_id = shop_id,
                        account_info = $"充值满{recharge.rc_man} 送 {recharge.rc_song}",
                        account_money = recharge.rc_song,
                        user_id = userInfo.Id,
                        aId = aid
                    };
                    songLog.id = Convert.ToInt32(DishCardAccountLogBLL.SingleModel.Add(songLog));
                    if (songLog.id > 0)
                    {
                        accountLog.account_info += "|" + songLog.id;
                    }
                    else
                    {
                        result.code = 0;
                        result.info = "系统异常";
                        return Json(result);
                    }
                }
            }
            accountLog.id = Convert.ToInt32(DishCardAccountLogBLL.SingleModel.Add(accountLog));
            if (accountLog.id <= 0)
            {
                result.code = 0;
                result.info = "系统异常!";
                return Json(result);
            }
            string msg = string.Empty;
            int cityMorderId = DishCardAccountLogBLL.SingleModel.CreateOrder(accountLog);
            result.code = 11;

            result.info = new { order_id = accountLog.id, cityMorderId, msg };
            return Json(result);
        }

        /// <summary>
        /// 提交手机号码
        /// </summary>
        /// <param name="code"></param>
        /// <param name="phone"></param>
        /// <param name="utoken"></param>
        /// <returns></returns>
        public ActionResult postChangePhone(string code, string phone, string utoken)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(utoken))
            {
                result.code = 0;
                result.info = "参数错误";
                return Json(result);
            }
            //验证码是否匹配
            string serverAuthCode = RedisUtil.Get<string>(string.Format(DISH_PHONE_KEY, phone));
            if (serverAuthCode != code)
            {
                result.code = 0;
                result.info = "手机号码错误或验证码错误";
                return Json(result);
            }
            C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.info = "用户信息错误";
                return Json(result);
            }
            userInfo.TelePhone = phone;
            bool isSuccess = C_UserInfoBLL.SingleModel.Update(userInfo, "telephone");
            if (isSuccess)
            {
                result.code = 1;
                result.info = "提交成功";
            }
            else
            {
                result.info = "提交失败";
            }
            return Json(result);
        }

        /// <summary>
        /// 发送短信消息验证
        /// </summary>
        /// <param name="tel"></param>
        /// <param name="sendType"></param>
        /// <param name="isNeedCheckBind">是否检查手机已被绑定</param>
        /// <returns></returns>
        public ActionResult sendPhoneCode(SendTypeEnum sendType = SendTypeEnum.个人中心)
        {
            string telePhoneNumber = Context.GetRequest("phone", string.Empty);

            Return_Msg msg = new Return_Msg();
            msg.isok = false;
            try
            {
                if (string.IsNullOrWhiteSpace(telePhoneNumber) || !Regex.IsMatch(telePhoneNumber, @"^[1]+[3-9]+\d{9}$"))
                {
                    msg.Msg = "手机格式不正确！";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }

                //通过短信发送验证码
                SendMsgHelper sendMsgHelper = new SendMsgHelper();
                string authCode = RedisUtil.Get<string>(string.Format(DISH_PHONE_KEY, telePhoneNumber));
                if (string.IsNullOrEmpty(authCode))
                    authCode = Utility.EncodeHelper.CreateRandomCode(4);
                bool result = sendMsgHelper.AliSend(telePhoneNumber, "{\"code\":\"" + authCode + "\",\"product\":\" " + Enum.GetName(typeof(SendTypeEnum), sendType) + "\"}", "小未科技", 401);
                if (result)
                {
                    RedisUtil.Set<string>(string.Format(DISH_PHONE_KEY, telePhoneNumber), authCode, TimeSpan.FromMinutes(5));
                    msg.isok = true;
                    msg.Msg = "验证码发送成功！";
                }
                else
                {
                    msg.Msg = "验证码发送失败,请稍后再试！";
                }
                msg.dataObj = authCode;
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msg.Msg = "系统异常！" + ex.Message;
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
        }
    }
}