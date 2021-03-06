﻿using DAL.Base;
using Entity.MiniApp.Pin;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Pin
{
    public class PinAttrBLL : BaseMySql<PinAttr>
    {
        #region 单例模式
        private static PinAttrBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinAttrBLL()
        {

        }

        public static PinAttrBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinAttrBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public bool UpdateSortBatch(string sortData)
        {
            if (string.IsNullOrEmpty(sortData))
                return false;

            string[] sortDataArray = sortData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (sortDataArray.Length <= 0)
                return false;
            List<string> sql = new List<string>();
            List<MySqlParameter[]> sqlParameters = new List<MySqlParameter[]>();
            for (int i = 0; i < sortDataArray.Length; i++)
            {
                string[] idSortArray = sortDataArray[i].Split('_');
                sql.Add($"update PinAttr set sort=@sort where id=@id");
                sqlParameters.Add(new MySqlParameter[] {
                    new MySqlParameter("@sort",idSortArray[1]),
                    new MySqlParameter("@id",idSortArray[0])
                });
            }
            return ExecuteTransaction(sql.ToArray(), sqlParameters.ToArray());
        }
    }
}
