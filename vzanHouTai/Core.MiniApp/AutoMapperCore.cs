using AutoMapper;
using Core.MiniApp.DTO;
using Entity.MiniApp.Dish;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.MiniApp
{
    /// <summary>
    /// AutoMapper核心业务封装
    /// </summary>
    public class AutoMapperCore
    {
        /// <summary>
        /// 映射配置
        /// </summary>
        private IMapperConfigurationExpression _config { get; set; }

        /// <summary>
        /// 解析映射配置
        /// </summary>
        public static void Configure()
        {
            //初始化映射设置
            Mapper.Initialize(newConfig => new AutoMapperCore()._configure(ref newConfig));
            //校验映射设置（无效配置，抛出异常）
            Mapper.AssertConfigurationIsValid();
        }

        private void _configure(ref IMapperConfigurationExpression newConfig)
        {
            _config = newConfig;
            /**开始新增映射**/

            //新增映射类方法：调用addToManifest<DAO,DTO>（可选设置：DTO字段=DAO字段）
            //示例代码：addToManifest<EntGoods, FlashItemForSelect>("SourceId=id,Title=name,OrigPrice=price,DealPrice=price");

            //--新增映射类统一，写在此注释下面--
            AddFlashDealMap();
            AddDishGoodMap();
            /**结束新增映射**/
        }

        /// <summary>
        /// 增加映射类到解析清单
        /// </summary>
        /// <typeparam name="T">DAO（数据库实体类）</typeparam>
        /// <typeparam name="T2">DTO（传输实体类）</typeparam>
        /// <param name="members"></param>
        /// <returns></returns>
        private IMappingExpression<T, T2> AddMapper<T, T2>(string members = null)
        {
            IMappingExpression<T,T2> profile = _config.CreateMap<T, T2>(MemberList.None);
            if (string.IsNullOrWhiteSpace(members))
            {
                return profile;
            }
            string[] memberSet = members.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string member in memberSet)
            {
                string[] memberExp = member.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                string memberDTO = memberExp.First();
                string memberDAO = memberExp.Last();
                profile.ForMember(memberDTO, _ => _.MapFrom(memberDAO));
            };
            return profile;
        }

        /// <summary>
        /// 新增秒杀业务映射
        /// </summary>
        private void AddFlashDealMap()
        {
            IMappingExpression<EntGoods, FlashItemForSelect> mapper = AddMapper<EntGoods, FlashItemForSelect>("SourceId=id,Title=name,OrigPrice=price,DealPrice=price");
        }

        private void AddDishGoodMap()
        {
            IMappingExpression<DishGood, EditProduct> productMap = AddMapper<DishGood, EditProduct>("Img=img,Name=g_name,Description=g_description,Unit=g_danwei,Hit=g_renqi,PrintTagId=g_print_tag,Price=shop_price,OriginalPrice=market_price,PackingFee=dabao_price,DailySupply=day_kucun,Sort=is_order,CategoryId=cate_id,Sale=yue_xiaoliang");
            productMap.ForMember(member => member.IsTakeOut, _ => _.MapFrom(item => item.is_waimai == 1));
            productMap.ForMember(member => member.Display, _ => _.MapFrom(item => item.state == 1));
            IMappingExpression<DishGoodAttr, EditProductAttr> productAttrMap = AddMapper<DishGoodAttr, EditProductAttr>("AttrId=attr_id,Option=value,Price=price,AttrTypeId=attr_type_id");
        }
    }
}
