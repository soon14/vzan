using DAL.Base;
using System.Data;
using Entity.MiniApp.Dish;
using System.Collections.Generic;
using Utility;
using Core.MiniApp;

namespace BLL.MiniApp.Dish
{
    public class DishAttrTypeBLL : BaseMySql<DishAttrType>
    {
        #region 单例模式
        private static DishAttrTypeBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishAttrTypeBLL()
        {

        }

        public static DishAttrTypeBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishAttrTypeBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<DishAttrType> GetListFromTable(int pageIndex, int pageSize,int aId, int storeId)
        {
            DataTable dt = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, $"SELECT t.*,(SELECT count(0) from dishattr where dishattr.cat_id=t.id and dishattr.state=1) as attrcount from dishattrtype t where t.state<>-1 and t.storeId={storeId} and t.aid={aId} order by id desc limit {pageSize * pageIndex},{pageSize}").Tables[0];
            return DataHelper.ConvertDataTableToList<DishAttrType>(dt);
        }
        
        public int GetCountByStore(int storeId, int aId)
        {
            return GetCount($"state<>-1 and aid={aId} and storeid={storeId}");
        }

        public bool DeleteAttrType(DishAttrType attrType)
        {
            attrType.state = -1;
            return base.Update(attrType, "state");
        }
    }
}
