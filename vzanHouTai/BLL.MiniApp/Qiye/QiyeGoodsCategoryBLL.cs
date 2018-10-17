using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Qiye;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Qiye
{
    public class QiyeGoodsCategoryBLL : BaseMySql<QiyeGoodsCategory>
    {
        #region 单例模式
        private static QiyeGoodsCategoryBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyeGoodsCategoryBLL()
        {

        }

        public static QiyeGoodsCategoryBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyeGoodsCategoryBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private readonly string Redis_QiyeGoodsCategoryList = "QiyeGoodsCategory_{0}_{1}_{2}_{3}";
        private readonly string Redis_QiyeGoodsCategoryList_version = "QiyeGoodsCategory_version_{0}_{1}";


        /// <summary>
        /// 根据aid获取列表以及总数
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="totalCount"></param>
        /// <param name="isFirstType">1则表示大类 否则为小类</param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="msgTypeName"></param>
        /// <param name="orderWhere"></param>
        /// <returns></returns>
        public List<QiyeGoodsCategory> getListByaid(int aid, out int totalCount, int isFirstType = 1, int pageSize = 10, int pageIndex = 1, string orderWhere = "sortNumber desc,addTime desc", int parentId = 0)
        {
            string strWhere = $"aid={aid} and state<>-1";
            if (isFirstType == 1)
            {
                strWhere += $"  and parentId=0 ";
            }
            else if (isFirstType == 2)
            {
                strWhere += $"  and parentId<>0 ";
            }
            else
            {

                strWhere += $"  and parentId={parentId} ";
            }

            totalCount = base.GetCount(strWhere);
            List<QiyeGoodsCategory> list = base.GetListByParam(strWhere, null, pageSize, pageIndex, "*", orderWhere);

            if (isFirstType != 0)
            {
                string categoryIds = string.Join(",",list.Select(s=>s.ParentId).Distinct());
                List<QiyeGoodsCategory> qiyeGoodsCategoryList = GetListByIds(categoryIds);

                list.ForEach(x =>
                {
                    QiyeGoodsCategory QiyeProductCategory = qiyeGoodsCategoryList?.FirstOrDefault(f=>f.Id == x.ParentId);
                    if (QiyeProductCategory != null)
                    {
                        x.ParentName = QiyeProductCategory.Name;
                    }
                });
            }
            return list;
        }


        /// <summary>
        /// 根据id集合获取QiyeProductCategory列表数据
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<QiyeGoodsCategory> GetListByIds(int aid, string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<QiyeGoodsCategory>();

            string strWhere = $"aid={aid} and state<>-1 and Id in({ids})";
            return base.GetList(strWhere);
        }

        public List<QiyeGoodsCategory> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<QiyeGoodsCategory>();

            string strWhere = $"Id in({ids})";
            return base.GetList(strWhere);
        }

        /// <summary>
        /// 判断类别名称是否存在
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="msgTypeName"></param>
        /// <param name="isFirstType">0则表示大类 否则为小类</param>
        /// <returns></returns>

        public QiyeGoodsCategory msgTypeNameIsExist(int aid, string msgTypeName, int isFirstType = 0)
        {

            string strWhere = $"aid={aid} and state<>-1 and name=@msgTypeName";
            if (isFirstType == 0)
            {
                strWhere += $"  and parentId=0 ";
            }
            else
            {
                strWhere += $"  and parentId<>0 ";
            }
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@msgTypeName", "" + msgTypeName + ""));
            return base.GetModel(strWhere, mysqlParams.ToArray());
        }


        public int GetSecondCategoryCount(int ParentId)
        {
            return base.GetCount($"ParentId={ParentId} and State<>-1");
        }


        /// <summary>
        /// 根据类别名称模糊匹配类别集合Id
        /// </summary>
        /// <returns></returns>
        public List<int> GetCategoryIdName(string categoryName)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            List<int> listAgentId = new List<int>();
            parameters.Add(new MySqlParameter("@categoryName", $"%{categoryName}%"));
            string strWhere = " ParentId<>0 and name like @categoryName";
            List<QiyeGoodsCategory> list = base.GetListByParam(strWhere, parameters.ToArray());
            if (list != null && list.Count > 0)
            {
                foreach (QiyeGoodsCategory item in list)
                {
                    listAgentId.Add(item.Id);
                }
            }
            return listAgentId;
        }

        /// <summary>
        /// 根据大类Id获取其下的子类Id
        /// </summary>
        /// <param name="Aid"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public string GetChildIds(int Aid, int Id)
        {
            List<QiyeGoodsCategory> list = base.GetList($"aid={Aid} and parentId={Id} and state<>-1");
            if (list != null && list.Count > 0)
            {
                List<int> listIds = new List<int>();
                foreach (QiyeGoodsCategory item in list)
                {
                    listIds.Add(item.Id);
                }
                return string.Join(",", listIds);
            }
            else
            {
                return "0";
            }
        }



        /// <summary>
        /// 根据类别名称获取id集合
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public string GetGoodCategoryIds(int appId, int pageSize, int pageIndex, string typeName)
        {
            int count = 0;

            List<string> listIds = new List<string>();
            List<QiyeGoodsCategory> goodTypeList = new List<QiyeGoodsCategory>();
            List<QiyeGoodsCategory> goodTypes = GetListByCach(appId, 100, 1, ref count);//获取小类的
            if (goodTypes != null && goodTypes.Count > 0)
            {
                goodTypeList = goodTypes.FindAll(x => x.Name.Contains(typeName));
                foreach (QiyeGoodsCategory item in goodTypeList)
                {
                    listIds.Add(item.Id.ToString());
                }
            }

            if (listIds.Count > 0)
            {
                return string.Join(",", listIds);
            }
            return string.Empty;
        }



        /// <summary>
        /// 获取产品分类（加缓存）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<QiyeGoodsCategory> GetListByCach(int appId, int pageSize, int pageIndex, ref int count, int typeIndex = -1)
        {
            string strwhere = $"aid={appId} and state<>-1 ";
            if (typeIndex == 0)
            {
                strwhere += " and parentId=0";
            }
            else
            {
                strwhere += " and parentId<>0";
            }
            string key = string.Format(Redis_QiyeGoodsCategoryList, appId, pageSize, pageIndex, typeIndex);
            string version_key = string.Format(Redis_QiyeGoodsCategoryList_version, appId, typeIndex);
            int QiyeGoodsCategoryList_version = RedisUtil.GetVersion(version_key);

            RedisModel<QiyeGoodsCategory> list = RedisUtil.Get<RedisModel<QiyeGoodsCategory>>(key);
            list = null;
            if (list == null || list.DataList == null || list.DataList.Count <= 0 || list.Count <= 0 || QiyeGoodsCategoryList_version != list.DataVersion)
            {
                list = new RedisModel<QiyeGoodsCategory>();
                list.DataList = GetList(strwhere, pageSize, pageIndex, "*", "SortNumber desc, id asc ");
                list.DataList.ForEach(x =>
                {
                    x.ParentName = GetQiyeGoodsCategoryName(x.ParentId.ToString());
                });
                count = GetCount(strwhere);
                list.Count = count;
                list.DataVersion = QiyeGoodsCategoryList_version;

                RedisUtil.Set<RedisModel<QiyeGoodsCategory>>(key, list, TimeSpan.FromHours(12));
            }
            else
            {
                count = list.Count;
            }

            return list.DataList;
        }

        public string GetQiyeGoodsCategoryName(string ptypes)
        {
            string sql = $"SELECT GROUP_CONCAT(`name`) from QiyeGoodsCategory where FIND_IN_SET(id,@ptypes)";
            return DAL.Base.SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                CommandType.Text, sql,
                new MySqlParameter[] { new MySqlParameter("@ptypes", ptypes) }).ToString();
        }


    }
}
