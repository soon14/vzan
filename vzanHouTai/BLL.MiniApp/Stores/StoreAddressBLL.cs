using DAL.Base;
using Entity.MiniApp.Stores;

namespace BLL.MiniApp.Stores
{
    public class StoreAddressBLL : BaseMySql<StoreAddress>
    {
        #region 单例模式
        private static StoreAddressBLL _singleModel;
        private static readonly object SynObject = new object();

        private StoreAddressBLL()
        {

        }

        public static StoreAddressBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new StoreAddressBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public bool SetDefault(int id, int UserId)
        {
            var addressBll = new  StoreAddressBLL();
            var address = addressBll.GetModel(id);
            if (address == null || address.UserId != UserId)
                return false;
            addressBll.ExecuteNonQuery($"UPDATE storeaddress set IsDefault = 0 where UserId = '{UserId}';");
            address.IsDefault = 1;
            return addressBll.Update(address, "IsDefault");
        }
    }
}
