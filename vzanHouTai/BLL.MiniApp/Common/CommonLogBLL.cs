using DAL.Base;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp
{
    public class CommonLogBLL : BaseMySql<CommonLog>
    {

        #region 单例模式
        private static CommonLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private CommonLogBLL()
        {

        }

        public static CommonLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CommonLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
