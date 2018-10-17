using Entity.MiniApp.Dish;
using Entity.MiniApp.Ent;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Helper
{
    /// <summary>
    /// 用于处理需要导出的Excel数据
    /// </summary>
    public static class ExportExcelBLL
    {
        #region 多门店导出订单数据处理
        /// <summary>
        /// 获取到店自取导出数据表格
        /// </summary>
        /// <param name="orderList"></param>
        /// <returns></returns>
        public static DataTable GetDaoDianZiquData(List<EntGoodsOrder> orderList)
        {
            DataTable exportTable = new DataTable();
            exportTable.Columns.AddRange(new DataColumn[] {
                        new DataColumn("订单号"),
                        new DataColumn("商品"),
                        new DataColumn("会员"),
                        new DataColumn("会员级别"),
                        new DataColumn("订单金额(元)"),
                        new DataColumn("优惠金额(元)"),
                        new DataColumn("实收金额(元)"),
                        new DataColumn("支付方式"),
                        new DataColumn("手机号"),
                        new DataColumn("自取门店"),
                        new DataColumn("订单备注"),
                        new DataColumn("下单时间"),
                        new DataColumn("订单状态")
                    });
            foreach (var item in orderList)
            {
                var dr = exportTable.NewRow();
                dr[0] = item.OrderNum;
                dr[1] = item.goodsNames;
                dr[2] = item.nickName;
                dr[3] = item.vipLevelName;
                dr[4] = item.GoodsMoney;
                dr[5] = item.ReducedPriceStr;
                dr[6] = item.BuyPriceStr;
                dr[7] = item.BuyModeStr;
                dr[8] = item.AccepterTelePhone;
                dr[9] = item.storeName;
                dr[10] = item.Message;
                dr[11] = item.CreateDateStr;
                dr[12] = item.StateStr;
                exportTable.Rows.Add(dr);
            }
            return exportTable;
        }
        /// <summary>
        /// 获取门店配送导出数据表格
        /// </summary>
        /// <param name="orderList"></param>
        /// <returns></returns>
        public static DataTable GetTongChengPeisongData(List<EntGoodsOrder> orderList)
        {
            DataTable exportTable = new DataTable();
            exportTable.Columns.AddRange(new DataColumn[] {
                        new DataColumn("订单号"), 
                        new DataColumn("商品"),
                        new DataColumn("会员"),
                        new DataColumn("会员级别"),
                        new DataColumn("订单金额(元)"),
                        new DataColumn("优惠金额(元)"),
                        new DataColumn("实收金额(元)"),
                        new DataColumn("支付方式"),
                        new DataColumn("配送地址"),
                        new DataColumn("收货人"),
                        new DataColumn("收货电话"),
                        new DataColumn("配送门店"),
                        new DataColumn("下单时间"),
                        new DataColumn("订单备注"),
                        new DataColumn("订单状态")
                    });
            foreach (var item in orderList)
            {
                var dr = exportTable.NewRow();
                dr[0] = item.OrderNum;
                dr[1] = item.goodsNames;
                dr[2] = item.nickName;
                dr[3] = item.vipLevelName;
                dr[4] = item.GoodsMoney;
                dr[5] = item.ReducedPriceStr;
                dr[6] = item.BuyPriceStr;
                dr[7] = item.BuyModeStr;
                dr[8] = item.Address;
                dr[9] = item.AccepterName;
                dr[10] = item.AccepterTelePhone;
                dr[11] = item.storeName;
                dr[12] = item.CreateDateStr;
                dr[13] = item.Message;
                dr[14] = item.StateStr;
                exportTable.Rows.Add(dr);
            }
            return exportTable;
        }

        /// <summary>
        /// 获取快递配送导出数据表格
        /// </summary>
        /// <param name="orderList"></param>
        /// <returns></returns>
        public static DataTable GetKuaiDiPeisongData(List<EntGoodsOrder> orderList)
        {
            DataTable exportTable = new DataTable();
            exportTable.Columns.AddRange(new DataColumn[] {
                        new DataColumn("订单号"),
                        new DataColumn("商品"),
                        new DataColumn("会员"),
                        new DataColumn("会员级别"),
                        new DataColumn("订单金额(元)"),
                        new DataColumn("优惠金额(元)"),
                        new DataColumn("实收金额(元)"),
                        new DataColumn("支付方式"),
                        new DataColumn("配送地址"),
                        new DataColumn("收货人"),
                        new DataColumn("收货电话"),
                        new DataColumn("下单时间"),
                        new DataColumn("订单备注"),
                        new DataColumn("订单状态")
                    });
            foreach (var item in orderList)
            {
                var dr = exportTable.NewRow();
                dr[0] = item.OrderNum;
                dr[1] = item.goodsNames;
                dr[2] = item.nickName;
                dr[3] = item.vipLevelName;
                dr[4] = item.GoodsMoney;
                dr[5] = item.ReducedPriceStr;
                dr[6] = item.BuyPriceStr;
                dr[7] = item.BuyModeStr;
                dr[8] = item.Address;
                dr[9] = item.AccepterName;
                dr[10] = item.AccepterTelePhone;
                dr[11] = item.CreateDateStr;
                dr[12] = item.Message;
                dr[13] = item.StateStr;
                exportTable.Rows.Add(dr);
            }
            return exportTable;
        }



        #endregion

        #region 餐饮多门店导出数据处理
        public static DataTable GetDishVipData(List<DishVipCard> dataList)
        {
            DataTable exportTable = new DataTable();
            exportTable.Columns.AddRange(new DataColumn[] {
                        new DataColumn("会员名称"),
                        new DataColumn("会员姓名"),
                        new DataColumn("手机号码"),
                        new DataColumn("会员卡号"),
                        new DataColumn("开卡日期"),
                        new DataColumn("会员卡余额"),
                    });
            foreach (var item in dataList)
            {
                var dr = exportTable.NewRow();
                dr[0] = item.nickname;
                dr[1] = item.u_name;
                dr[2] = item.u_phone;
                dr[3] = item.number;
                dr[4] = item.add_time.ToString("yyyy-MM-dd HH:mm:ss");
                dr[5] = item.account_balance.ToString("F");
                exportTable.Rows.Add(dr);
            }
            return exportTable;
        }
        #endregion
    }
}
