using DAL.Base;
using Entity.MiniApp.cityminiapp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.cityminiapp
{
    public class CityStoreMsgTypeBLL : BaseMySql<CityStoreMsgType>
    {
        #region 单例模式
        private static CityStoreMsgTypeBLL _singleModel;
        private static readonly object SynObject = new object();

        private CityStoreMsgTypeBLL()
        {

        }

        public static CityStoreMsgTypeBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CityStoreMsgTypeBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 根据aid获取列表以及总数
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="msgTypeName"></param>
        /// <param name="orderWhere"></param>
        /// <returns></returns>
        public List<CityStoreMsgType> getListByaid(int aid, out int totalCount, int pageSize = 10, int pageIndex = 1, string msgTypeName = "", string orderWhere = "sortNumber desc,addTime desc")
        {
            string strWhere = $"aid={aid} and state<>-1";
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(msgTypeName))
            {
                strWhere += $" and name like @msgTypeName";
                mysqlParams.Add(new MySqlParameter("@msgTypeName", "%" + msgTypeName + "%"));
            }
            totalCount = base.GetCount(strWhere, mysqlParams.ToArray());
            return base.GetListByParam(strWhere, mysqlParams.ToArray(), pageSize, pageIndex, "*", orderWhere);
        }

        public List<CityStoreMsgType> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<CityStoreMsgType>();

            string strWhere = $"Id in({ids})";
            return base.GetList(strWhere);
        }

        /// <summary>
        /// 根据id集合获取City_StoreMsgType列表数据
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<CityStoreMsgType> GetListByIds(int aid, string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<CityStoreMsgType>();

            string strWhere = $"aid={aid} and state<>-1 and Id in({ids})";
            return base.GetList(strWhere);
        }

        /// <summary>
        /// 根据类别名称获取id集合
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="msgTypeName"></param>
        /// <returns></returns>
        public string GetListByName(int aid, string msgTypeName)
        {
            int totalCount = 0;
            string ids = string.Empty;
            List<int> listIds = new List<int>();
            List<CityStoreMsgType> list = getListByaid(aid, out totalCount, 1000, 1, msgTypeName);
            if (list != null && list.Count > 0)
            {
                listIds.AddRange(list.Select(x => x.Id));
                ids = string.Join(",", listIds);
            }
            return ids;
        }

        /// <summary>
        /// 判断类别名称是否存在
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="msgTypeName"></param>
        /// <returns></returns>

        public CityStoreMsgType msgTypeNameIsExist(int aid, string msgTypeName)
        {

            string strWhere = $"aid={aid} and state<>-1 and name=@msgTypeName";
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@msgTypeName", "" + msgTypeName + ""));
            return base.GetModel(strWhere, mysqlParams.ToArray());
        }


      



    }
}
