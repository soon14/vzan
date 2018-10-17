using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity.Base;
using Utility;

namespace Entity.MiniApp.Stores
{
    /// <summary>
    /// 小程序商城模板-代制卡券申请信息
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class StoreInsteadCardAuth
    {
        public StoreInsteadCardAuth() { }
        /// <summary>
        /// ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

      

        /// <summary>
        /// APPID  电商版对应storId  餐饮版对应appid
        /// </summary>
        [SqlField]
        public int AppId { get; set; }

        /// <summary>
        /// 子商户id，对于一个母商户公众号下唯一  创建后微信返回的
        /// </summary>
        [SqlField]
        public int Merchant_Id { get; set; }
        /// <summary>
        /// 店铺名称
        /// </summary>
        [SqlField]
        public string Brand_Name { get; set; }

        /// <summary>
        /// 店铺Logo  上传到微信后返回的地址
        /// </summary>
        [SqlField]
        public string Logo_Url { get; set; }

        /// <summary>
        /// 授权函ID，即通过上传临时素材接口上传授权函后获得的meida_id
        /// </summary>
        [SqlField]
        public int Protocol { get; set; }


        /// <summary>
        /// 授权函文件地址 上传到阿里云后的地址
        /// </summary>
        [SqlField]
        public string AuthLetter { get; set; }

        /// <summary>
        /// 授权函截止时间  授权函有效期截止时间（东八区时间，单位为秒），
        /// 需要与提交的扫描件一致 10位数Unix时间戳（Unix timestamp）单位秒
        /// </summary>
        [SqlField]
        public DateTime End_Time { get; set; }



        /// <summary>
        /// 一级类目id
        /// </summary>
        [SqlField]
        public int Primary_Category_Id { get; set; }

        /// <summary>
        /// 二级类目id
        /// </summary>
        [SqlField]
        public int Secondary_Category_Id { get; set; }




        /// <summary>
        /// 授权函类别（0-无公章授权函；1-有公章授权函）
        /// </summary>
        [SqlField]
        public int AuthLetterType { get; set; }

      

        /// <summary>
        /// 营业执照或个体工商户营业执照彩照或扫描件-无公章授权函时必须
        /// 通过微信临时素材上传后得到的meida_id
        /// </summary>
        [SqlField]
        public string Agreement_Media_Id { get; set; }

        /// <summary>
        /// 营业执照或个体工商户营业执照彩照或扫描件-无公章授权函时必须
        /// 上传到阿里云得到的地址
        /// </summary>
        [SqlField]
        public string AgreementFile { get; set; }


        /// <summary>
        /// 营业执照内登记的经营者身份证彩照或扫描件-无公章授权函时必须
        /// 通过微信临时素材上传后得到的meida_id
        /// </summary>
        [SqlField]
        public string Operator_Media_Id { get; set; }

        /// <summary>
        /// 营业执照内登记的经营者身份证彩照或扫描件-无公章授权函时必须
        /// 上传到阿里云得到的地址
        /// </summary>
        [SqlField]
        public string OperatorFile { get; set; }


        /// <summary>
        /// 子商户状态，"CHECKING" 审核中, "APPROVED" , 已通过；"REJECTED"被驳回, "EXPIRED"协议已过期
        /// </summary>
        [SqlField]
        public string Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// 状态（-1-未通过审核;0-等待审核中;1-已通过审核）
        /// </summary>
        [SqlField]
        public int IsPass { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        [SqlField]
        public DateTime? AuditDate { get; set; }

      

        /// <summary>
        /// 审核信息（驳回信息）
        /// </summary>
        [SqlField]
        public string Reason { get; set; }
    }
}
