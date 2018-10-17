using DAL.Base;
using Entity.MiniApp.Fds;
using System.Collections.Generic;

namespace BLL.MiniApp.Fds
{
    public class FoodTableBLL : BaseMySql<FoodTable>
    {
        #region 单例模式
        private static FoodTableBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodTableBLL()
        {

        }

        public static FoodTableBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodTableBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public FoodTable GetModelByScene(int foodid,string scene,bool isNewTableNo=false)
        {
            if (isNewTableNo)
            {
                return GetModel($"FoodId={foodid} and id={scene} and State>=0 ");
            }
            return GetModel($"FoodId={foodid} and Scene='{scene}' and State>=0 ");
        }

        public List<FoodTable> GetTablesByStore(int foodId)
        {
            return GetList($" FoodId = {foodId} and State >= 0 ");
        }

        public FoodTable GetModelById(int id)
        {
            return GetModel($"id={id} and state>=0");
        }
    }
}
