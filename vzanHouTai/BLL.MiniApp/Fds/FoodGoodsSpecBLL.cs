using DAL.Base;
using Entity.MiniApp.Fds;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BLL.MiniApp.Fds
{
    public class FoodGoodsSpecBLL : BaseMySql<FoodGoodsSpec>
    {
        #region 单例模式
        private static FoodGoodsSpecBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodGoodsSpecBLL()
        {

        }

        public static FoodGoodsSpecBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodGoodsSpecBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 餐饮版产规格缓存{AttrId属性Id} 
        /// </summary>
        public static readonly string foodGoodsSpecKey = "foodGoodsSpec_{0}";
        public List<FoodGoodsSpec> GetListBySpecIds(string specids)
        {
            return base.GetList($" Id in ({specids})");
        }

        /// <summary>
        /// 根据属性Id获取对应规格列表 已经加缓存处理
        /// </summary>
        /// <param name="AttrId"></param>
        /// <returns></returns>
        public List<FoodGoodsSpec> GetlistSpecByAttrId(int AttrId)
        {
            string key = string.Format(foodGoodsSpecKey, AttrId);
            List<FoodGoodsSpec> listGoodsSpec = RedisUtil.Get<List<FoodGoodsSpec>>(key);
            if (listGoodsSpec == null)
            {
                listGoodsSpec = GetList($"AttrId={AttrId} and State>=0");
                RedisUtil.Set(key, listGoodsSpec);
            }

            return listGoodsSpec;
        }

        /// <summary>
        /// 获取商品的规格属性
        /// </summary>
        /// <param name="goodList"></param>
        /// <returns></returns>
        public List<object> GetGoodsAttrSpaceByGoodsIds(List<FoodGoods> goodList)
        {
            List<FoodGoodsAttr> attrList = null;
            if (goodList == null || goodList.Count <= 0) return null;
            string goodsIds = string.Join(",", goodList.Select(goods => goods.Id));

            string sql = $"select attrspec.foodgoodsid, attr.id as attrId,attr.FoodId,attr.AttrName,attr.State as AttrState,spec.id as SpecId,spec.SpecName,spec.state specState from foodgoodsattrspec attrspec left join foodgoodsspec as spec on spec.id = attrspec.specid left join foodgoodsattr as attr on spec.attrid = attr.id and attr.id = attrspec.attrid  where attrspec.foodgoodsid  in ({goodsIds})";
            DataSet ds = SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);

            if (ds.Tables.Count <= 0) return null;
            DataTable dt = ds.Tables[0];
            List<FoodGoodsAttrSpecModel> list = null;
            if (dt != null && dt.Rows.Count > 0)
            {
                list = new List<FoodGoodsAttrSpecModel>();
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(ConvertToAttrSpecModel(row));
                }
            }

            List<object> objList = new List<object>();
            if (list != null && list.Count > 0)
            {
                foreach (var goods in goodList)
                {
                    //取出商品的规格属性
                    List<FoodGoodsAttrSpecModel> childList = list.Where(attrSpecModel => attrSpecModel.foodGoodsId == goods.Id).ToList();
                    attrList = new List<FoodGoodsAttr>();
                    
                    //取出商品规格并去除重复的
                    childList.ForEach(child =>
                    {
                        if (child.attr != null && attrList.Where(attr=>attr.Id==child.attr.Id).ToList().Count<=0)
                        {
                            attrList.Add(child.attr);
                        }
                    });

                    //根据规格取相应的属性值
                    attrList.ForEach(attr =>
                    {
                        attr.SpecList = new List<FoodGoodsSpec>();
                        childList.ForEach(child =>
                        {
                            if(child.spec!=null&& child.spec.AttrId==attr.Id && attr.SpecList.Where(spec=>spec.Id==child.spec.Id).ToList().Count<=0)
                            {
                                attr.SpecList.Add(child.spec);
                            }
                        });
                    });
                    //拼接数据
                    object obj = new
                    {
                        goods = goods,
                        lables = goods.labelNameStr?.Split(','),
                        attrList = attrList
                    };
                    objList.Add(obj);
                }
            }
            else
            {
                //拼接数据
                goodList.ForEach(goods =>
                {
                    object obj = new
                    {
                        goods = goods,
                        lables = goods.labelNameStr?.Split(','),
                        attrList = attrList
                    };
                    objList.Add(obj);
                });
            }
            return objList;

        }

        /// <summary>
        /// 连表数据转换实体
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private FoodGoodsAttrSpecModel ConvertToAttrSpecModel(DataRow row)
        {
            FoodGoodsAttrSpecModel model = new FoodGoodsAttrSpecModel();
            model.foodId = Convert.ToInt32(row["FoodId"]);
            model.foodGoodsId = Convert.ToInt32(row["foodgoodsid"]);
            model.attrId = Convert.ToInt32(row["attrId"]);
            model.attrName = row["AttrName"].ToString();
            model.attrState = Convert.ToInt32(row["AttrState"]);
            model.specId = Convert.ToInt32(row["SpecId"]);
            model.specName = row["SpecName"].ToString();
            model.specState = Convert.ToInt32(row["specState"]);
            //规格
            model.attr = new FoodGoodsAttr();
            model.attr.Id = Convert.ToInt32(row["attrId"]);
            model.attr.AttrName = row["AttrName"].ToString();
            model.attr.State = Convert.ToInt32(row["AttrState"]);
            model.attr.FoodId = Convert.ToInt32(row["FoodId"]);
            //属性
            model.spec = new FoodGoodsSpec();
            model.spec.Id = Convert.ToInt32(row["SpecId"]);
            model.spec.SpecName = row["SpecName"].ToString();
            model.spec.State = Convert.ToInt32(row["specState"]);
            model.spec.AttrId = Convert.ToInt32(row["attrId"]);

            return model;
        }
    }
}
