using System;
using DAL.Base;
using Entity.MiniApp.Dish;
using System.Collections.Generic;
using System.Linq;

namespace BLL.MiniApp.Dish
{
    public class DishPrintBLL : BaseMySql<DishPrint>
    {
        #region 单例模式
        private static DishPrintBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishPrintBLL()
        {

        }

        public static DishPrintBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishPrintBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public DishPrint GetModelByAid_StoreId_Id(int aId, int storeId, int id)
        {
            string sqlwhere = $" aid={aId} and storeId={storeId} and id={id}";
            return GetModel(sqlwhere);
        }

        /// <summary>
        /// 根据打单类型获取相应打印机
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="print_type"></param>
        /// <returns></returns>
        public List<DishPrint> GetPrintsByParams(int aId, int storeId,int print_type = 0,int? pageIndex = null,int? pageSize = null)
        {
            string sqlwhere = $" aid={aId} and storeId={storeId} {(print_type > 0 ? $" and print_type = {print_type}": "")} and state = 1 ";
            if(pageIndex.HasValue && pageSize.HasValue)
            {
                return GetList(sqlwhere, pageSize.Value, pageIndex.Value);
            }
            return GetList(sqlwhere);
        }

        public int GetCountByStoreId(int storeId)
        {
            string sqlwhere = $" storeId={storeId} and state>-1";
            return GetCount(sqlwhere);
        }

        public void SyncPrintByPrintTag(int aId, int storeId, int tagsId)
        {
            List<DishPrint> prints = GetPrintsByParams(aId, storeId, 0);
            int[] selTagsId = null;
            foreach (DishPrint p in prints)
            {
                if (string.IsNullOrWhiteSpace(p.print_tags)) continue;
                selTagsId = p.print_tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(t => Convert.ToInt32(t)).ToArray();
                selTagsId?.Where(s => s != tagsId);

                p.print_tags = string.Join(",", selTagsId);

                Update(p, "print_tags");
            }
        }

        public List<DishPrint> GetByIds(string printerIds)
        {
            return GetList($"Id in ({printerIds})");
        }
    }
}
