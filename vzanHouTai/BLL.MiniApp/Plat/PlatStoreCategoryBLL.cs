using DAL.Base;
using Entity.MiniApp.Plat;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{
    public class PlatStoreCategoryBLL : BaseMySql<PlatStoreCategory>
    {
        #region 单例模式
        private static PlatStoreCategoryBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatStoreCategoryBLL()
        {

        }

        public static PlatStoreCategoryBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatStoreCategoryBLL();
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
        /// <param name="isFirstType">1则表示大类 否则为小类</param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="msgTypeName"></param>
        /// <param name="orderWhere"></param>
        /// <returns></returns>
        public List<PlatStoreCategory> getListByaid(int aid, out int totalCount, int isFirstType = 1, int pageSize = 10, int pageIndex = 1, string orderWhere = "sortNumber desc,addTime desc",int parentId=0)
        {
            string strWhere = $"aid={aid} and state<>-1";
            if (isFirstType == 1)
            {
                strWhere += $"  and parentId=0 ";
            }
            else if(isFirstType==2)
            {
                strWhere += $"  and parentId<>0 ";
            }
            else
            {
                
                strWhere += $"  and parentId={parentId} ";
            }
            totalCount = base.GetCount(strWhere);
            List<PlatStoreCategory> list = base.GetListByParam(strWhere, null, pageSize, pageIndex, "*", orderWhere);

            if (isFirstType != 0)
            {
                string cartgoryIds = string.Join(",",list.Select(s=>s.ParentId).Distinct());
                List<PlatStoreCategory> platStoreCategoryList = GetListByIds(cartgoryIds);

                list.ForEach(x =>
                {
                    PlatStoreCategory platStoreCategory = platStoreCategoryList?.FirstOrDefault(f=>f.Id == x.ParentId);
                    if (platStoreCategory != null)
                    {
                        x.ParentName = platStoreCategory.Name;
                    }
                });
            }
            return list;
        }


        /// <summary>
        /// 根据id集合获取PlatStoreCategory列表数据
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<PlatStoreCategory> GetListByIds(int aid, string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<PlatStoreCategory>();

            string strWhere = $"aid={aid} and state<>-1 and Id in({ids})";
            return base.GetList(strWhere);
        }

        public List<PlatStoreCategory> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<PlatStoreCategory>();

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

        public PlatStoreCategory msgTypeNameIsExist(int aid, string msgTypeName, int isFirstType = 0)
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
            List<PlatStoreCategory> list = base.GetListByParam(strWhere, parameters.ToArray());
            if (list != null && list.Count > 0)
            {
                foreach (PlatStoreCategory item in list)
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
        public string GetChildIds(int Aid,int Id)
        {
            List<PlatStoreCategory> list = base.GetList($"aid={Aid} and parentId={Id} and state<>-1");
            if (list != null && list.Count > 0)
            {
                List<int> listIds = new List<int>();
                foreach(PlatStoreCategory item in list)
                {
                    listIds.Add(item.Id);
                }
                return string.Join(",",listIds);
            }
            else
            {
                return "0";
            }
        }

       


    }
}
