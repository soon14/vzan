using DAL.Base;
using Entity.MiniApp.Pin;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Pin
{
    public class PinCommentBLL: BaseMySql<PinComment>
    {
        #region 单例模式
        private static PinCommentBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinCommentBLL()
        {

        }

        public static PinCommentBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinCommentBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 评价
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool AddComment(PinComment comment,PinGoodsOrder order)
        {
            TransactionModel tran = new TransactionModel();
            tran.Add(BuildAddSql(comment));
            tran.Add($" update pingoodsorder set state = {(int)PinEnums.PinOrderState.已评价} where id={order.id} ");
            tran.Add($" update pingoods set  CommentCount=CommentCount+1 where id={order.goodsId}");
            return ExecuteTransactionDataCorect(tran.sqlArray);
        }
    }
}
