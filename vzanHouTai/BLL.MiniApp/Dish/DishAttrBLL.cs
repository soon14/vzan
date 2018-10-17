using DAL.Base;
using System.Data;
using Entity.MiniApp.Dish;
using System.Collections.Generic;
using Utility;
using Core.MiniApp;

namespace BLL.MiniApp.Dish
{
    public class DishAttrBLL : BaseMySql<DishAttr>
    {
        #region 单例模式
        private static DishAttrBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishAttrBLL()
        {

        }

        public static DishAttrBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishAttrBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<DishAttr> GetListFromTable(int pageIndex, int pageSize,int fid)
        {
            DataTable dt = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, $"SELECT a.*,(SELECT cat_name from dishattrtype where dishattrtype.id=a.cat_id and dishattrtype.state=1) as cat_name from dishattr a where a.state=1 and cat_id={fid}  order by id desc limit {pageSize * pageIndex},{pageSize}").Tables[0];
            return DataHelper.ConvertDataTableToList<DishAttr>(dt);
        }

        public List<DishAttr> GetListById(string Id)
        {
            return GetList($"Id IN ({Id})");
        }

        public int GetCountByType(int typeId)
        {
            return GetCount($"state = 1 and cat_id = {typeId}");
        }

        public bool DeleteAttr(DishAttr attrbute)
        {
            attrbute.state = -1;
            return base.Update(attrbute, "state");
        }
    }
}
