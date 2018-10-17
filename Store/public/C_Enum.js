//已退款
const ReturnPrice= -2;
//取消订单
const CancelOrder = -1;
//未付款
const NoPay = 0;
//待发货
const WaitSendGoods = 3;
//正在配送
const SendingGoods = 4;
//待收货
const GetingGoods = 5;
//已收货
const GetedGoods = 6

module.exports = {
  ReturnPrice: ReturnPrice,
  CancelOrder: CancelOrder,
  NoPay: NoPay,
  WaitSendGoods: WaitSendGoods,
  SendingGoods: SendingGoods,
  GetingGoods: GetingGoods,
  GetedGoods: GetedGoods,
}