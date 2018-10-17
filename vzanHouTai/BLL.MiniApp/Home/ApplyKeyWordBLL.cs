using DAL.Base;
using Entity.MiniApp.Home;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Home
{
    public class ApplyKeyWordBLL : BaseMySql<ApplyKeyWord>
    {
        #region 单例模式
        private static ApplyKeyWordBLL _singleModel;
        private static readonly object SynObject = new object();

        private ApplyKeyWordBLL()
        {

        }

        public static ApplyKeyWordBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new ApplyKeyWordBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<ApplyKeyWord> GetDataList(int state, int typeId, string phone, string keyWordName, int pageIndex, int pageSize, ref int count)
        {
            List<ApplyKeyWord> list = new List<ApplyKeyWord>();

            string sql = $"select {"{0}"}  from ApplyKeyWord k left join KeyWord t on k.KeyWordId=t.id";
            string sqlList = string.Format(sql, "k.*,t.name tname");
            string sqlCount = string.Format(sql, "count(*)");
            string sqlWhere = $" where 1=1";
            string sqlLimit = $" ORDER BY k.addtime desc LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";

            if (typeId != -999)
            {
                sqlWhere += $" and k.typeid={typeId} ";
            }
            if (state != -999)
            {
                sqlWhere += $" and k.state={state} ";
            }
            List<MySqlParameter> paras = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(phone))
            {
                sqlWhere += " and k.phone like @phone ";
                paras.Add(new MySqlParameter("@phone", $"%{phone}%"));
            }
            if (!string.IsNullOrEmpty(keyWordName))
            {
                sqlWhere += " and t.name like @tname ";
                paras.Add(new MySqlParameter("@tname", $"%{keyWordName}%"));
            }

            count = base.GetCountBySql(sqlCount + sqlWhere, paras.ToArray());
            if (count <= 0)
                return list;

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sqlList + sqlWhere + sqlLimit, paras.ToArray()))
            {
                while (dr.Read())
                {
                    ApplyKeyWord model = base.GetModel(dr);
                    if (dr["tname"] != DBNull.Value)
                    {
                        model.KeyWordName = dr["tname"].ToString();
                    }
                    list.Add(model);
                }
            }

            return list;
        }
    }
}
