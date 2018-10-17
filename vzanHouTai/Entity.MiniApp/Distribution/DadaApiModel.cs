using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    /// <summary>
    /// 返回值表单
    /// </summary>
    public class DadaApiReponseModel<T>
    {
        /// <summary>
        /// 响应返回吗，参考接口返回码
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 错误编码，与code一致
        /// </summary>
        public int errorCode { get; set; }
        /// <summary>
        /// 响应描述
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 响应状态，成功为"success"，失败为"fail"
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// 响应结果，JSON对象，详见具体的接口描述
        /// </summary>
        public T result { get; set; }
    }

    /// <summary>
    /// 请求表单
    /// </summary>
    public class DadaApiRequestModel
    {
        public string body { get; set; }
        public string format { get; set; }
        public string timestamp { get; set; }
        public string signature { get; set; }
        public string app_key { get; set; }
        public string v { get; set; }
        public string source_id { get; set; }
    }

    /// <summary>
    /// 订单状态改变回调表单
    /// </summary>
    public class OrderReponseModel
    {
        /// <summary>
        /// /返回达达运单号，默认为空
        /// </summary>
        public string client_id { get; set; }
        /// <summary>
        /// 添加订单接口中的origin_id值
        /// </summary>
        public string order_id { get; set; }
        /// <summary>
        /// 订单状态(待接单＝1 待取货＝2 配送中＝3 已完成＝4 已取消＝5 已过期＝7 指派单=8 妥投异常之物品返回中=9 妥投异常之物品返回完成=10 创建达达运单失败=1000 可参考文末的状态说明）
        /// </summary>
        public int order_status { get; set; }
        /// <summary>
        /// 订单取消原因,其他状态下默认值为空字符串
        /// </summary>
        public string cancel_reason { get; set; }
        /// <summary>
        /// 订单取消原因来源(1:达达配送员取消；2:商家主动取消；3:系统或客服取消；0:默认值)
        /// </summary>
        public int cancel_from { get; set; }
        /// <summary>
        /// 更新时间,时间戳
        /// </summary>
        public int update_time { get; set; }
        /// <summary>
        /// 对client_id, order_id, update_time的值进行字符串升序排列，再连接字符串，取md5值
        /// </summary>
        public string signature { get; set; }
        /// <summary>
        /// 达达配送员id，接单以后会传
        /// </summary>
        public int dm_id { get; set; }
        /// <summary>
        /// 配送员姓名，接单以后会传
        /// </summary>
        public string dm_name { get; set; }
        /// <summary>
        /// 配送员手机号，接单以后会传
        /// </summary>
        public string dm_mobile { get; set; }

    }

    /// <summary>
    /// （新增订单、重新发布订单、订单预发布、取消订单(线上环境)、查询追加配送员）返回值公共表单
    /// </summary>
    public class ResultReponseModel
    {
        /// <summary>
        /// 配送距离(单位：米)
        /// </summary>
        public double distance { get; set; }
        /// <summary>
        /// 实际运费(单位：元)，运费减去优惠券费用
        /// </summary>
        public double fee { get; set; }
        /// <summary>
        /// 运费(单位：元)
        /// </summary>
        public double deliverFee { get; set; }
        /// <summary>
        /// 运费(单位：分)
        /// </summary>
        public int deliverFeeInt { get; set; }
        /// <summary>
        /// 优惠券费用(单位：元)
        /// </summary>
        public double couponFee { get; set; }
        /// <summary>
        /// 平台订单编号
        /// </summary>
        public string deliveryNo { get; set; }

        #region 取消订单
        /// <summary>
        /// 扣除的违约金(单位：元)
        /// </summary>
        public double deduct_fee { get; set; }
        #endregion

        #region 订单取消原因或查询追加配送员返回值result参数
        /// <summary>
        /// 理由编号/配送员id
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 取消理由
        /// </summary>
        public string reason { get; set; }
        /// <summary>
        /// 配送员姓名
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 配送员城市
        /// </summary>
        public int city_id { get; set; }
        #endregion
    }
    /// <summary>
    /// 查询订单表单
    /// </summary>
    public class QueryOrderReponseModel
    {
        /// <summary>
        /// 第三方订单编号
        /// </summary>
        public string orderId { get; set; }
        /// <summary>
        /// 订单状态(待接单＝1 待取货＝2 配送中＝3 已完成＝4 已取消＝5 已过期＝7 
        /// 指派单= 8 妥投异常之物品返回中= 9 妥投异常之物品返回完成= 10 系统故障订单发布失败= 1000 可参考文末的状态说明）
        /// </summary>
        public int statusCode { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public string statusMsg { get; set; }
        /// <summary>
        /// 骑手姓名
        /// </summary>
        public string transporterName { get; set; }
        /// <summary>
        /// 骑手电话
        /// </summary>
        public string transporterPhone { get; set; }
        /// <summary>
        /// 骑手经度
        /// </summary>
        public string transporterLng { get; set; }
        /// <summary>
        /// 骑手纬度
        /// </summary>
        public string transporterLat { get; set; }

        /// <summary>
        /// 配送费, 单位为元
        /// </summary>
        public double deliveryFee { get; set; }
        /// <summary>
        /// 小费, 单位为元
        /// </summary>
        public double tips { get; set; }
        /// <summary>
        /// 优惠券费用, 单位为元
        /// </summary>
        public double couponFee { get; set; }
        /// <summary>
        /// 实际支付费用, 单位为元
        /// </summary>
        public double actualFee { get; set; }
        /// <summary>
        /// 配送距离, 单位为米
        /// </summary>
        public int distance { get; set; }
        /// <summary>
        /// 发单时间
        /// </summary>
        public string createTime { get; set; }
        /// <summary>
        /// 接单时间, 若未接单, 则为空
        /// </summary>
        public string acceptTime { get; set; }
        /// <summary>
        /// 取货时间, 若未取货, 则为空
        /// </summary>
        public string fetchTime { get; set; }
        /// <summary>
        /// 送达时间, 若未送达, 则为空
        /// </summary>
        public string finishTime { get; set; }
        /// <summary>
        /// 取消时间, 若未取消, 则为空
        /// </summary>
        public string cancelTime { get; set; }
        /// <summary>
        /// 收货码
        /// </summary>
        public string orderFinishCode { get; set; }
    }
    /// <summary>
    /// 新增门店返回值表单
    /// </summary>
    public class AddShowReponseModel
    {
        /// <summary>
        /// 成功导入门店的数量
        /// </summary>
        public int success { get; set; }
        /// <summary>
        /// 成功导入的门店
        /// </summary>
        public List<DadaShop> successList { get; set; }
        /// <summary>
        /// 导入失败门店编号以及原因
        /// </summary>
        public List<FaileShop> failedList { get; set; }
    }
    /// <summary>
    /// 导入失败的门店返回信息
    /// </summary>
    public class FaileShop
    {
        /// <summary>
        /// 门店编号
        /// </summary>
        public string shopNo { get; set; }
        /// <summary>
        /// 返回信息
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 门店名称
        /// </summary>
        public string shopName { get; set; }
    }
}
