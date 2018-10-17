using DAL.Base;
using Entity.MiniApp.Tools;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Tools
{
    public class BargainBLL : BaseMySql<Bargain>
    {
        #region 单例模式
        private static BargainBLL _singleModel;
        private static readonly object SynObject = new object();

        private BargainBLL()
        {

        }

        public static BargainBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new BargainBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 查找一个店铺的砍价活动 ,注意查找的字段不是全部
        /// </summary>
        /// <param name="StoreId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="IsEnd">是否结束 -1表示全部 1 结束 0进行中</param>
        ///   <param name="BargainType">0→电商版(默认) 1→餐饮版</param>
        /// <returns></returns>
        public List<Bargain> GetListByStoreId(int StoreId, int pageSize = 10, int pageIndex = 1, int IsEnd = -1, int BargainType = 0)
        {         
            string strWhere = $"(State<>-1 and IsDel<>-1) and StoreId={StoreId} and BargainType={BargainType}";
            if (IsEnd == 0)
            {
                strWhere = $"(State<>-1 and IsDel<>-1) and StoreId={StoreId} and BargainType={BargainType} and IsEnd<>1 ";
            }if (IsEnd == 1)
            {
                strWhere = $"(State<>-1 and IsDel<>-1) and StoreId={StoreId} and BargainType={BargainType} and IsEnd=1";
            }
            
            string selectColumn = "id,ImgUrl,BName,OriginalPrice,FloorPrice,RemainNum,CreateNum,StartDate,EndDate,IsEnd";
            return base.GetList(strWhere, pageSize, pageIndex, selectColumn, "CreateDate desc");
        }

        public List<int> GetBidsByStoreId(int aid)
        {
            List<int> listBId = new List<int>();
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, $"select Id from Bargain where StoreId={aid}", null))
            {
                while (dr.Read())
                {
                    if (dr["Id"] != DBNull.Value)
                    {
                        listBId.Add(Convert.ToInt32(dr["Id"]));
                    }
                }

            }
            return listBId;
        }
    }
}
