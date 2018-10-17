using DAL.Base;
using Entity.MiniApp.Dish;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Dish
{
    public class DishAdminUserBLL : BaseMySql<DishAdminUser>
    {
        #region 单例模式
        private static DishAdminUserBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishAdminUserBLL()
        {

        }

        public static DishAdminUserBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishAdminUserBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<DishAdminUser> GetAdminListByParam(int aid,int storeId = 0)
        {
            return base.GetList($" aid = {aid} and storeId = {storeId} ");
        }

        public DishAdminUser GetAdminByParam(int aid, int storeId = 0)
        {
            return base.GetModel($" aid = {aid} and storeId = {storeId} ");
        }


        /// <summary>
        /// 检测是否存在同名管理员账号
        /// </summary>
        /// <param name="id">当前记录ID</param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="login_username">登录名</param>
        /// <returns></returns>
        public bool CheckExistLoginName(int id, int aId, int storeId, string login_username)
        {
            string whereSql = $" aId = @aId and storeId = @storeId and login_username = @login_username and id != @id ";
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@id", id));
            mysqlParams.Add(new MySqlParameter("@aId", aId));
            mysqlParams.Add(new MySqlParameter("@storeId", storeId));
            mysqlParams.Add(new MySqlParameter("@login_username", login_username));

            return base.Exists(whereSql, mysqlParams.ToArray());
        }
        /// <summary>
        /// 默认更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool UpdateAdminData(DishAdminUser model)
        {
            //默认要更新的字段
            List<string> defaultUpdateCols = new List<string>();
            defaultUpdateCols.Add("login_username");
            defaultUpdateCols.Add("updateTime");
            if (!string.IsNullOrWhiteSpace(model.login_userpass))
            {
                defaultUpdateCols.Add("login_userpass");
            }

            model.updateTime = DateTime.Now;
            return base.Update(model,string.Join(",", defaultUpdateCols));
        }


        /// <summary>
        /// 返回登录人信息
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="login_username"></param>
        /// <param name="login_userpass"></param>
        /// <returns></returns>
        public DishAdminUser GetAdminByLoginParams(int aid, string login_username, string login_userpass)
        {
            try
            {
                MySqlParameter[] paras = new MySqlParameter[]{
                                 new MySqlParameter("@aid",aid),
                                 new MySqlParameter("@login_username",login_username),
                                 new MySqlParameter("@login_userpass",DESEncryptTools.GetMd5Base32(login_userpass)),
                };
                DishAdminUser admin = base.GetModel(" aid = @aid and login_username = @login_username and login_userpass = @login_userpass  ", paras);
                return admin;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #region 重写底层,加入添加更新model加密密码
        public override object Add(DishAdminUser model)
        {
            //加密密码
            model.login_userpass = DESEncryptTools.GetMd5Base32(model.login_userpass);
            model.addTime = DateTime.Now;
            return base.Add(model);
        }


        public override bool Update(DishAdminUser model)
        {
            //加密密码
            if(!model.login_userpass.IsNullOrWhiteSpace())
            {
                model.login_userpass = DESEncryptTools.GetMd5Base32(model.login_userpass);
            }

            return base.Update(model);
        }

        public override bool Update(DishAdminUser model,string columns)
        {
            if (columns.IsNullOrWhiteSpace())
            {
                return false;
            }

            //加密密码
            if (columns.Split(',').Contains("login_userpass") && !model.login_userpass.IsNullOrWhiteSpace())
            {
                model.login_userpass = DESEncryptTools.GetMd5Base32(model.login_userpass);
            }
            return base.Update(model, columns);
        }
        #endregion
    }
}
