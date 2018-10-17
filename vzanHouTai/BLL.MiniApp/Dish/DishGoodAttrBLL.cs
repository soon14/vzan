using DAL.Base;
using System.Data;
using Entity.MiniApp.Dish;
using System.Collections.Generic;
using Utility;
using Core.MiniApp;

namespace BLL.MiniApp.Dish
{
    public class DishGoodAttrBLL : BaseMySql<DishGoodAttr>
    {
        #region 单例模式
        private static DishGoodAttrBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishGoodAttrBLL()
        {

        }

        public static DishGoodAttrBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishGoodAttrBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<DishGoodAttr> GetListByProduct(int productId)
        {
            return GetList($"goods_id = {productId}");
        }

        public bool UpdateAttr(List<DishGoodAttr> newAttr = null, List<DishGoodAttr> updateAttr = null, List<DishGoodAttr> deleteAttr = null)
        {
            TransactionModel tran = new TransactionModel();
            updateAttr?.ForEach(attr => tran.Add(BuildUpdateSql(attr, "attr_name,attr_id,attr_type_id,price,value")));
            deleteAttr?.ForEach(attr => tran.Add($"DELETE FROM DishGoodAttr WHERE ID = {attr.id}"));
            newAttr?.ForEach(attr => tran.Add(BuildAddSql(attr)));

            if(tran.sqlArray.Length == 0)
            {
                return true;
            }
            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }
    }
}
