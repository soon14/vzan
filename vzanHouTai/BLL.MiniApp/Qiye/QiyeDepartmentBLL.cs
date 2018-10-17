using DAL.Base;
using Entity.MiniApp.Qiye;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Qiye
{
   public class QiyeDepartmentBLL: BaseMySql<QiyeDepartment>
    {
        #region 单例模式
        private static QiyeDepartmentBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyeDepartmentBLL()
        {

        }

        public static QiyeDepartmentBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyeDepartmentBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<QiyeDepartment> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<QiyeDepartment>();

            return base.GetList($"id in ({ids})");
        }

        public List<QiyeDepartment> GetQiyeDepartmentList(int aid , out int totalCount, int pageSize = 10, int pageIndex = 1)
        {
            string sqlWhere = $"Aid={aid} and State<>-1";
            List<QiyeDepartment> list = base.GetList(sqlWhere,pageSize, pageIndex);
            totalCount = base.GetCount(sqlWhere);
            return list;
        }

        /// <summary>
        /// 根据名称获取部门
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public QiyeDepartment GetDepartmentByName(int appId, string name)
        {
            return base.GetModel($"Name=@name and Aid={appId}  and state<>-1", new MySqlParameter[] { new MySqlParameter("@name", name) });
        }

        /// <summary>
        /// 获取该部门下员工数量
        /// </summary>
        /// <param name="departMentId"></param>
        /// <returns></returns>
        public int GetEmployeeCount(int departMentId)
        {
            return QiyeEmployeeBLL.SingleModel.GetCount($"DepartmentId={departMentId} and State<>-1");
        }



    }
}
