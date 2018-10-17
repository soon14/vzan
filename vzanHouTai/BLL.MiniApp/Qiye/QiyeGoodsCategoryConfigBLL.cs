using DAL.Base;
using Entity.MiniApp.Qiye;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Qiye
{

   public class QiyeGoodsCategoryConfigBLL : BaseMySql<QiyeGoodsCategoryConfig>
    {
        #region 单例模式
        private static QiyeGoodsCategoryConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyeGoodsCategoryConfigBLL()
        {

        }

        public static QiyeGoodsCategoryConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyeGoodsCategoryConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public QiyeGoodsCategoryConfig GetModelByAid(int aid)
        {
            return base.GetModel($"Aid={aid}");
        }
    }
}
