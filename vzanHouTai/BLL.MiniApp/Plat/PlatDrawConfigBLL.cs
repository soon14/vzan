using BLL.MiniApp.Conf;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Plat;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Plat
{
    public class PlatDrawConfigBLL : BaseMySql<PlatDrawConfig>
    {
        #region 单例模式
        private static PlatDrawConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatDrawConfigBLL()
        {

        }

        public static PlatDrawConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatDrawConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public PlatDrawConfig GetModelByAId(int aid)
        {
            return base.GetModel($"aid={aid} and state>=0");
        }
    }
}
