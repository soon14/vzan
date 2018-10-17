using DAL.Base;
using Entity.MiniApp.User;
using System.Collections.Generic;
/// <summary>
/// Member转移过来的
/// </summary>
namespace BLL.MiniApp
{
    public class CustomModelUserRelationBLL : BaseMySql<CustomModelUserRelation>
    {
        #region 单例模式
        private static CustomModelUserRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private CustomModelUserRelationBLL()
        {

        }

        public static CustomModelUserRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CustomModelUserRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public CustomModelUserRelation GetModelByAId(int aid)
        {
            return base.GetModel($"aid = {aid}");
        }
    }
}