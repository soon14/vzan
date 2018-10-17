using DAL.Base;
using Entity.MiniApp.Fds;

namespace BLL.MiniApp.Fds
{
    public class FoodAddressBLL : BaseMySql<FoodAddress>
    {
        #region 单例模式
        private static FoodAddressBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodAddressBLL()
        {

        }

        public static FoodAddressBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodAddressBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public override object Add(FoodAddress model)
        {
            var addSql = BuildAddSql(model);
            addSql += "select last_insert_id(); ";
            return SqlMySql.ExecuteScalar(connName, System.Data.CommandType.Text, addSql, null);
        }

        public override bool Update(FoodAddress model, string columnFields)
        {
            var updateSql = BuildUpdateSql(model,columnFields) + ";";
            
            return SqlMySql.ExecuteNonQuery(connName, System.Data.CommandType.Text, updateSql, null) > 0;
        }

        public override bool Update(FoodAddress model)
        {
            var updateSql = BuildUpdateSql(model) + ";";
            return SqlMySql.ExecuteNonQuery(connName, System.Data.CommandType.Text, updateSql, null) > 0;
        }

        public bool SetDefault(int id, int UserId)
        {
            
            var address = base.GetModel(id);
            if (address == null || address.UserId != UserId)
                return false;
            base.ExecuteNonQuery($"UPDATE foodaddress set IsDefault = 0 where UserId = '{UserId}';");
            address.IsDefault = 1;
            return base.Update(address, "IsDefault");
        }
    }
}
