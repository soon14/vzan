using DAL.Base;
using Entity.MiniApp.Ent;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Utility;

namespace BLL.MiniApp.Ent
{
    public class EntIndutypesBLL : BaseMySql<EntIndutypes>
    {
        #region 单例模式
        private static EntIndutypesBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntIndutypesBLL()
        {

        }

        public static EntIndutypesBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntIndutypesBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public string GetEntIndutypesName(int aid,string entIndutypesId)
        {
            string sql = $"SELECT GROUP_CONCAT(`TypeName`) from EntIndutypes where FIND_IN_SET(TypeId,@entIndutypesId) and aid={aid} and state=1";
            return DAL.Base.SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                CommandType.Text, sql,
                new MySqlParameter[] { new MySqlParameter("@entIndutypesId", entIndutypesId) }).ToString();
        }
        
        /// <summary>
        /// 通过参数名称获取参数Id
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public EntIndutypes GetExType(int aid, string typeName,int parentId)
        {
            string strWhere = $"aid={aid} and TypeName=@typeName and state=1";
            if (parentId > 0)
            {
                strWhere += $" and parentid={parentId}";
            }
            else if (parentId == 0)
            {
                strWhere += $" and parentid=0";
            }
            
            return base.GetModel(strWhere, new MySqlParameter[] { new MySqlParameter("@typeName", typeName) });

        }

        public List<EntIndutypes> GetListByAId(int aid)
        {
            string sqlwhere = $"aid={aid} and state=1";
            return base.GetList(sqlwhere);
        }

        /// <summary>
        /// 判断该行业有没有参数数据，没有复制系统默认参数到该行业
        /// </summary>
        /// <param name="entid">行业id</param>
        /// <param name="industr">行业类型</param>
        /// <returns></returns>
        public bool CopySystemTypes(int aid, string industr, ref string msg)
        {
            string wheresql = $"state>=0";
            if (string.IsNullOrEmpty(industr))
            {
                wheresql += $" and (Industr ='{industr}' or Industr is null)";
            }
            else
            {
                wheresql += $" and Industr ='{industr}'";
            }
            var count = GetCount(wheresql + $" and AId={aid}");
            if (count <= 0)
            {
                var list = GetList(wheresql + " and AId=0");
                if (list == null && list.Count <= 0)
                {
                    msg = "找不到默认参数数据";
                    log4net.LogHelper.WriteInfo(this.GetType(), msg);
                }
                TransactionModel tranModel = new TransactionModel();
                foreach (var item in list)
                {
                    item.AId = aid;
                    item.AddTime = DateTime.Now;
                    item.UpdateTime = DateTime.Now;

                    tranModel.Add(BuildAddSql(item));
                }

                var result = base.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
                if (result)
                {
                    msg = "复制系统默认参数成功";
                }
                else
                {
                    msg = "复制默认参数数据出错";
                }
                return result;
            }
            msg = "已存在参数";
            return true;
        }

        /// <summary>
        /// 获取该行业参数
        /// </summary>
        /// <param name="entid">行业id</param>
        /// <param name="industr">行业类型</param>
        /// <returns></returns>
        public List<EntIndutypes> GetListByAIdandLevel(int aid, string industr, int level, ref string msg, int pageindex = 1, int pagesize = 10)
        {
            string wheresql = $"AId={aid}  and state>=0  and level={level}";
            if (string.IsNullOrEmpty(industr))
            {
                wheresql += $" and (Industr ='' or Industr is null)";
            }
            else
            {
                wheresql += $" and Industr ='{industr}'";
            }

            var list = GetList(wheresql, pagesize, pageindex, "", "Sort Desc,id asc");

            return list;
        }

        public int GetCountByIndustr(int aid, string industr, int level)
        {
            string wheresql = $"AId={aid}  and state>=0  and level={level}";
            if (string.IsNullOrEmpty(industr))
            {
                wheresql += $" and (Industr ='' or Industr is null)";
            }
            else
            {
                wheresql += $" and Industr ='{industr}'";
            }
            return GetCount(wheresql);
        }

        public int GetCountParent(int aid, string industr, int typeid)
        {
            string wheresql = $"ParentId={typeid} and aid={aid} and state>=0 ";
            if (string.IsNullOrEmpty(industr))
            {
                wheresql += $" and (Industr ='' or Industr is null)";
            }
            else
            {
                wheresql += $" and Industr ='{industr}'";
            }
            return GetCount(wheresql);
        }

        public List<EntIndutypes> GetListByIndustr(int aid, string industr, int level, int typeid)
        {
            string wheresql = $"AId={aid}  and state>=0  and ParentId={typeid} and level={level}";
            if (string.IsNullOrEmpty(industr))
            {
                wheresql += $" and (Industr ='' or Industr is null)";
            }
            else
            {
                wheresql += $" and Industr ='{industr}'";
            }
            return GetList(wheresql); ;
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="model"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public EntIndutypes AddIndustrType(EntIndutypes model, ref string msg)
        {
            if (model == null)
            {
                msg = "数据不能为空";
                return new EntIndutypes();
            }

            if (model.AId <= 0)
            {
                msg = "产品id不能小于0";
                return model;
            }


            if (model.ParentId < 0)
            {
                msg = "参数父类Id不能小于0";
                return model;
            }
            if (model.ParentId > 0)
            {
                var ptypemodel = GetModel($"TypeId={model.ParentId}");
                if (ptypemodel == null)
                {
                    msg = "找不到参数父类数据";
                    return model;
                }
            }

            if (model.ShowType != 0)
            {
                if (string.IsNullOrEmpty(model.Imgurl))
                {
                    msg = "图片不能为空";
                    return model;
                }
            }
            model.AddTime = DateTime.Now;
            model.UpdateTime = DateTime.Now;
            var obj = SqlMySql.ExecuteScalar(connName, System.Data.CommandType.Text, $"select max(typeid) from entindutypes  where aid = {model.AId} and Industr='{model.Industr}'");

            var maxtypeid = Convert.ToInt32(obj == DBNull.Value ? "0" : obj);
            model.TypeId = maxtypeid + 1;
            msg = "操作成功";
            model.Id = Convert.ToInt32(Add(model));
            if (model.Id <= 0)
            {
                msg = "操作失败";
            }

            return model;
        }

        /// <summary>
        /// 获取参数集合，相同key合并为一组，值用；隔开
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="extypes"></param>
        /// <returns></returns>
        public List<IndutypesItem> GetIndutypeList(int aid, string extypes)
        {
            List<IndutypesItem> datas = new List<IndutypesItem>();

            if (string.IsNullOrEmpty(extypes))
                return datas;

            List<EntIndutypes> list = GetListByAId(aid);
            if (list == null || list.Count <= 0)
                return datas;

            string[] array = extypes.Split(',');

            foreach (string item in array)
            {
                string[] ptypearray = item.Split('-');
                if (ptypearray == null || ptypearray.Length < 1)
                    continue;

                int temptypeid = 0;
                if (!int.TryParse(ptypearray[0], out temptypeid) || !int.TryParse(ptypearray[1], out temptypeid))
                {
                    log4net.LogHelper.WriteInfo(this.GetType(),$"专业版参数值无效,{extypes}");
                    continue;
                }

                EntIndutypes pkey = list.FirstOrDefault(f => f.TypeId == Convert.ToInt32(ptypearray[0]));
                EntIndutypes pvalue = list.FirstOrDefault(f => f.TypeId == Convert.ToInt32(ptypearray[1]));
                if (pkey == null || pvalue == null)
                    continue;

                IndutypesItem data = datas.FirstOrDefault(f => f.PTypeId == pkey.TypeId);
                if (data != null)
                {
                    datas[datas.FindIndex(f => f.PTypeId == pkey.TypeId)].PValue += ";" + pvalue.TypeName;
                }
                else
                {
                    datas.Add(new IndutypesItem { PTypeId = pkey.TypeId, PKey = pkey.TypeName, PValue = pvalue.TypeName });
                }
            }

            return datas;
        }

        public List<EntIndutypes> GetListByAidIndutye(int aid,string industry)
        {
            string sqlWhere = $"aid={aid} and state=1 and industr='{industry}' and parentid=0";
            return GetList(sqlWhere);
        }
    }
}
