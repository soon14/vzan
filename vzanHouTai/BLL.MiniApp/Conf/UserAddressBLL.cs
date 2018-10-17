using DAL.Base;
using MySql.Data.MySqlClient;
using Entity.MiniApp.Conf;
namespace BLL.MiniApp.Conf
{
    public class UserAddressBLL : BaseMySql<UserAddress>
    {
        #region 单例模式
        private static UserAddressBLL _singleModel;
        private static readonly object SynObject = new object();

        private UserAddressBLL()
        {

        }

        public static UserAddressBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new UserAddressBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 将一个收货地址设为默认收货地址
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool changeUserAddressState(int id,int userid, int isdefault)
        {
            TransactionModel tran = new TransactionModel();
            if (isdefault == 1)
            {
                tran.Add($"update UserAddress set isdefault=0 where userid={userid}");
                tran.Add($"update UserAddress set isdefault=1 where id={id}");
            }
            else
            {
                tran.Add($"update UserAddress set isdefault=0 where id={id}");
            }
            return ExecuteTransactionDataCorect(tran.sqlArray);
        }
        
        public bool Exists(UserAddress model)
        {
            string sql = $"userid=@userid and contact=@contact and phone=@phone and province=@province and city=@city and district=@district and street=@street and id!=@id";
            return this.Exists(sql, new MySqlParameter[] {
                new MySqlParameter("userid",model.userid),
                new MySqlParameter("contact",model.contact),
                new MySqlParameter("phone",model.phone),
                new MySqlParameter("province",model.province),
                new MySqlParameter("city",model.city),
                new MySqlParameter("district",model.district),
                new MySqlParameter("street",model.street),
                new MySqlParameter("id",model.id),
            });

        }
    }
}
