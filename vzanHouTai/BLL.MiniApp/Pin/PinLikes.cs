using DAL.Base;
using Entity.MiniApp.Pin;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Pin
{
    public class PinLikesBLL : BaseMySql<PinLikes>
    {
        #region 单例模式
        private static PinLikesBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinLikesBLL()
        {

        }

        public static PinLikesBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinLikesBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
