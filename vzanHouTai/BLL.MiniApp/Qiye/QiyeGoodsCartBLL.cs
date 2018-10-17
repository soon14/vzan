using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Qiye;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Qiye
{
    public class QiyeGoodsCartBLL : BaseMySql<QiyeGoodsCart>
    {
        #region 单例模式
        private static QiyeGoodsCartBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyeGoodsCartBLL()
        {

        }

        public static QiyeGoodsCartBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyeGoodsCartBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public int GetCartGoodsCountByUserId(int userid)
        {
            string sql = $"select sum(count) as count from QiyeGoodsCart where userid = {userid} and state=0 and gotobuy=0";
            var result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);

            if (DBNull.Value != result)
            {
                return Convert.ToInt32(result);
            }
            return 0;
        }
        public QiyeGoodsCart GetModelBySpec(int userId,int goodsid,string specids,int gotobuy)
        {
            return base.GetModel($" UserId={userId} and goodsid={goodsid} and SpecIds='{specids}' and State = 0 and gotobuy={gotobuy}");
        }
        public QiyeGoodsCart GetModelByGoodsId(int orderid, int goodsid)
        {
            return base.GetModel($" OrderId ={orderid} and GoodsId={goodsid}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="userId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="gotobuy">立即购买，1：是，0：否</param>
        /// <returns></returns>
        public List<QiyeGoodsCart> GetListByUserId(int aid, int userId,int pageSize,int pageIndex,ref int count,int gotobuy=0)
        {
            string sqlWhere = $"aid={aid} and UserId={userId} and State=0 and gotobuy={gotobuy}";
            count = base.GetCount(sqlWhere);
            return base.GetList(sqlWhere,pageSize,pageIndex,"","id desc");
        }

        public List<QiyeGoodsCart> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<QiyeGoodsCart>();

            string sqlwhere = $"id in ({ids})";
            return base.GetList(sqlwhere);
        }

        public List<QiyeGoodsCart> GetListByOrderIds(string orderids)
        {
            if (string.IsNullOrEmpty(orderids))
                return new List<QiyeGoodsCart>();

            string sqlwhere = $"orderid in ({orderids})";
            return base.GetList(sqlwhere);
        }

        /// <summary>
        /// 获取已确认收货并未评价的购物车数据，每次获取前1000条
        /// </summary>
        /// <returns></returns>
        public List<QiyeGoodsCart> GetSuccessDataList(int iscomment = 0, int day = -15)
        {
            List<QiyeGoodsCart> list = new List<QiyeGoodsCart>();
            string sqlwhere = "";
            //1:已评论,0:未评论
            if (iscomment >= 0)
            {
                sqlwhere = $" and c.iscommentting={iscomment} ";
            }
            string sql = $"select c.* from qiyeGoodsCart c right JOIN qiyegoodsorder o on c.OrderId=o.id where o.State={(int)PlatChildOrderState.已完成} and o.AcceptTime<='{DateTime.Now.AddDays(day)}' {sqlwhere} LIMIT 100";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                list = base.GetList(dr);
            }

            return list;
        }
    }
}
