using DAL.Base;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{
   public class PlatGoodsRelationBLL : BaseMySql<PlatGoodsRelation>
    {
        #region 单例模式
        private static PlatGoodsRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatGoodsRelationBLL()
        {

        }

        public static PlatGoodsRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatGoodsRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public string GetGoodsIdsByAids(int aid, int goodsSync = -1)
        {
            string strWhere = $"Aid ={aid}";
            if (goodsSync != -1)
            {
                strWhere += $" and Synchronized={goodsSync}";
            }
            List<PlatGoodsRelation> list = base.GetList(strWhere);
            List<int> listGoodsId = new List<int>();
            foreach(PlatGoodsRelation item in list)
            {
                if (!listGoodsId.Contains(item.GoodsId))
                {
                    listGoodsId.Add(item.GoodsId);
                }
            }
            if (listGoodsId.Count > 0)
            {
                return string.Join(",",listGoodsId);
            }
            else
            {
                return string.Empty;
            }

        }

        public List<PlatGoodsRelation> GetPlatGoodsRelationListByGoodsId(int aid, string ids)
        {
            return base.GetList($"Aid={aid} and GoodsId in({ids})");
        }

        public PlatGoodsRelation GetPlatGoodsRelation(int aid,int goodsId)
        {
            return base.GetModel($"Aid={aid} and GoodsId ={goodsId}");
        }

    }
}
