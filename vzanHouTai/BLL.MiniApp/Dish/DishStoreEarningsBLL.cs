using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Dish;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Dish
{
    public class DishStoreEarningsBLL : BaseMySql<DishStoreEarnings>
    {
        #region 单例模式
        private static DishStoreEarningsBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishStoreEarningsBLL()
        {

        }

        public static DishStoreEarningsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishStoreEarningsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 增加门店收益
        /// </summary>
        /// <param name="money">金钱</param>
        /// <param name="type">收益类型</param>
        /// <param name="info">收益备注</param>
        /// <returns></returns>
        public bool AddStoreEarning(int aId,int storeId, double money, DishEnums.EarningsType type, string info,ref string errMeg,TransactionModel tran = null)
        {
            bool isSuccess = false;

            DishStoreEarnings earnings = GetModel($" aId = {aId} and storeId = {storeId} ") ?? new DishStoreEarnings();
            if(earnings.id <= 0)
            {
                earnings.aId = aId;
                earnings.storeId = storeId;

                earnings.id = Convert.ToInt32(Add(earnings));
                if (earnings.id <= 0)
                {
                    errMeg = "创建店铺收益账号失败";
                    return isSuccess;
                }
            }

            DishStoreEarningsDetails earningsDtl = new DishStoreEarningsDetails();
            earningsDtl.type = (int)type;
            earningsDtl.seId = earnings.id;
            earningsDtl.remark = info;
            switch (type)
            {
                case DishEnums.EarningsType.支付:
                    earningsDtl.changeMoney = money;
                    earnings.money = earningsDtl.surplusMoney = earnings.money + money;
                    break;
                case DishEnums.EarningsType.退款:
                    earningsDtl.changeMoney = -money;
                    earnings.money = earningsDtl.surplusMoney = earnings.money - money;

                    break;
                case DishEnums.EarningsType.提现:
                    earningsDtl.changeMoney = -money;
                    earnings.money = earningsDtl.surplusMoney = earnings.money - money;
                    break;
            }
            earnings.updateTime = earningsDtl.addTime = DateTime.Now;

            if (tran != null)
            {
                tran.Add(BuildUpdateSql(earnings, "money,updateTime"));
                tran.Add(DishStoreEarningsDetailsBLL.SingleModel.BuildAddSql(earningsDtl));

                isSuccess = true;
                return isSuccess;
            }
            else
            {
                tran = new TransactionModel();
                tran.Add(BuildUpdateSql(earnings, "money,updateTime"));
                tran.Add(DishStoreEarningsDetailsBLL.SingleModel.BuildAddSql(earningsDtl));
                isSuccess = ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
                if (!isSuccess) errMeg = "改动收益失败";

                return isSuccess;
            }
        }

        public DishStoreEarnings GetModelByStoreId(int storeId)
        {
            return GetModel($"storeId={storeId}");
        }
    }
}