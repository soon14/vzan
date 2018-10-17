using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp.Dish;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Dish
{
    public class DishShoppingCartBLL : BaseMySql<DishShoppingCart>
    {

        #region 单例模式
        private static DishShoppingCartBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishShoppingCartBLL()
        {

        }

        public static DishShoppingCartBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishShoppingCartBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 根据订单Id找到对应的购物车记录
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="filterRefundGoods">筛选掉已经退款的菜品</param>
        /// <returns></returns>
        public List<DishShoppingCart> GetCartsByOrderId(int orderId, bool filterRefundGoods = false)
        {
            return base.GetList($" order_id = {orderId} {(filterRefundGoods ? " and is_tuikuan = 0 " : "")}");
        }


        /// <summary>
        /// 根据订单Id找到对应的购物车记录
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="filterRefundGoods">筛选掉退款菜品</param>
        /// <returns></returns>
        public List<DishShoppingCart> GetCartsByIds(int[] ids)
        {
            if (ids == null || ids.Length <= 0)
            {
                return null;
            }
            return base.GetList($" id in ({string.Join(",", ids)}) ");
        }


        /// <summary>
        /// 批量添加购物车
        /// </summary>
        /// <param name="carts"></param>
        /// <returns></returns>
        public bool BatchAddGoodCart(List<DishShoppingCart> carts)
        {
            TransactionModel tran = new TransactionModel();
            foreach (DishShoppingCart item in carts)
            {
                tran.Add(BuildAddSql(item));
            }
            return ExecuteTransactionDataCorect(tran.sqlArray,tran.ParameterArray);
        }
        
        /// <summary>
        /// 查询当前用户购物车
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public List<DishShoppingCart> GetShoppingCart(int aId, int storeId, int user_id,int orderId)
        {
            return GetList($" aId = {aId} and storeId = {storeId} and user_id = {user_id} and (order_Id = 0 or order_Id = {orderId}) ");
        }

        /// <summary>
        /// 删除旧的无效购物车记录
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public bool DeleteShoppingCart(int aId, int storeId, int user_id)
        {
            return Delete($" aId = {aId} and storeId = {storeId} and user_id = {user_id} and order_Id = 0 ") > 0; 
        }

    }
}