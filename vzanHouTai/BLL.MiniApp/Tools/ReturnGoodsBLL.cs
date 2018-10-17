using BLL.MiniApp.Ent;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Tools
{
    public class ReturnGoodsBLL : BaseMySql<ReturnGoods>
    {
        #region 单例模式
        private static ReturnGoodsBLL _singleModel;
        private static readonly object SynObject = new object();

        private ReturnGoodsBLL()
        {

        }

        public static ReturnGoodsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new ReturnGoodsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 新增用户退货申请
        /// </summary>
        /// <param name="userPost"></param>
        /// <returns></returns>
        public bool AddReturnOrder(ReturnGoodsPost userPost)
        {
            object result = Add(new ReturnGoods
            {
                AddTime = DateTime.Now,
                OrderId = userPost.OrderId,
                GoodsId = userPost.GoodsId,
                Reason = userPost.Reason,
                Remark = userPost.Remark,
                ReturnAmount = userPost.ReturnAmount,
                UploadPics = userPost.UploadPics,
                ReturnType = userPost.ReturnType,
                ReturnState = (int)ReturnGoodsState.商家审核中,
            });
            int newId = 0;
            return int.TryParse(result.ToString(), out newId) && newId > 0;
        }

        /// <summary>
        /// 更新退货订单状态
        /// </summary>
        /// <param name="order"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool UpdateState(ReturnGoods order, ReturnGoodsState state/*, ReturnGoodsPost updateInfo = null*/)
        {
            Action successEvent = null;
            switch (state)
            {
                case ReturnGoodsState.等待用户发货:
                    if (order.ReturnState != (int)ReturnGoodsState.商家审核中)
                    {
                        return false;
                    }
                    break;
                case ReturnGoodsState.等待商家收货:
                    if (order.ReturnState != (int)ReturnGoodsState.等待用户发货)
                    {
                        return false;
                    }
                    break;
                case ReturnGoodsState.等待用户收货:
                    if (order.ReturnState != (int)ReturnGoodsState.等待商家收货 || order.ReturnType != (int)ReturnGoodsType.专业版退换货)
                    {
                        return false;
                    }
                    break;
                //case ReturnGoodsState.商家审核中:
                //    if (order.ReturnState != (int)ReturnGoodsState.拒绝退款)
                //    {
                //        return false;
                //    }
                //    order.GoodsId = updateInfo.GoodsId;
                //    order.Reason = updateInfo.Reason;
                //    order.Remark = updateInfo.Remark;
                //    order.UploadPics = updateInfo.UploadPics;
                //    order.ReturnAmount = updateInfo.ReturnAmount;
                //    order.ReturnType = order.ReturnType;
                //    order.ReturnState = (int)state;
                //    return Update(order, "GoodsId,Reason,Remark,UploadPics,ReturnAmount,ReturnType,ReturnState");
                default:
                    return false;
            }
            order.ReturnState = (int)state;
            bool result = Update(order, "ReturnState");
            if (result)
            {
                successEvent();
            }
            return result;
        }

        public ReturnGoods GetByOrderId(int orderId)
        {
            return GetModel($"orderId = {orderId} ");
        }

        /// <summary>
        /// 是否存在退款类型
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool isExistType(int value)
        {
            return Enum.IsDefined(typeof(ReturnGoodsType), value);
        }

        public bool CheckInfoVailded(ReturnGoodsPost info)
        {
            if (info == null || info.OrderId <= 0 || !isExistType(info.ReturnType))
            {
                return false;
            }
            if(info.ReturnType == (int)ReturnGoodsType.专业版退货退款)
            {
                return !string.IsNullOrWhiteSpace(info.Reason);
            }
            else if (info.ReturnType == (int)ReturnGoodsType.专业版退换货)
            {
                return !string.IsNullOrWhiteSpace(info.Reason) && !string.IsNullOrWhiteSpace(info.GoodsId);
            }
            else
            {
                return false;
            }
        }

        public List<EntGoodsCart> GetGoodList(ReturnGoods info)
        {
            if(info.ReturnType == (int)ReturnGoodsType.专业版退货退款)
            {
                return EntGoodsCartBLL.SingleModel.GetListByOrderIds(info.OrderId.ToString());
            }
            else if(info.ReturnType == (int)ReturnGoodsType.专业版退换货)
            {
                return EntGoodsCartBLL.SingleModel.GetList($"Id in ({info.GoodsId})");
            }
            else
            {
                return null;
            }
        }
    }
}
