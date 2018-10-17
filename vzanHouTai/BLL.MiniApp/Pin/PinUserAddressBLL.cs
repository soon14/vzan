using DAL.Base;
using Entity.MiniApp.Pin;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Pin
{
    public class PinUserAddressBLL : BaseMySql<PinUserAddress>
    {
        #region 单例模式
        private static PinUserAddressBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinUserAddressBLL()
        {

        }

        public static PinUserAddressBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinUserAddressBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool setDefaultAddress(PinUserAddress model)
        {
            TransactionModel tran = new TransactionModel();
            if (model.isDefault == 1)
            {
                tran.Add($"update {TableName} set isdefault=0 where userid={model.userId}");
                tran.Add($"update {TableName} set isdefault=1 where id={model.id}");
            }
            else
            {
                tran.Add($"update UserAddress set isdefault=0 where id={model.id}");
            }
            return ExecuteTransactionDataCorect(tran.sqlArray);
        }
    }
}
