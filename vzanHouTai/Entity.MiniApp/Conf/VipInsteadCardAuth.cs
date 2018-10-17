using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{

    /// <summary>
    /// 小程序商城模板-代制卡券申请信息
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class VipInsteadCardAuth
    {
        public VipInsteadCardAuth() { }
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
        ///   电商版对应0  餐饮版对应1 专业版 2
        /// </summary>
        [SqlField]
        public int Type { get; set; }

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
        public string Protocol { get; set; }




        /// <summary>
        /// 授权函截止时间  授权函有效期截止时间（东八区时间，单位为秒），
        /// 需要与提交的扫描件一致 10位数Unix时间戳（Unix timestamp）单位秒
        /// </summary>
        [SqlField]
        public long End_Time { get; set; }



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
        /// 营业执照内登记的经营者身份证彩照或扫描件-无公章授权函时必须
        /// 通过微信临时素材上传后得到的meida_id
        /// </summary>
        [SqlField]
        public string Operator_Media_Id { get; set; }


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
        public DateTime AuditDate { get; set; }



        /// <summary>
        /// 审核信息（驳回信息）
        /// </summary>
        [SqlField]
        public string Reason { get; set; }
    }


    /// <summary>
    /// 卡券开放类目
    /// </summary>
    public class CategoryResult
    {
        public List<PrimaryCategoryItem> category { get; set; }
        public int errcode { get; set; }
        public string errmsg { get; set; }
    }

    public class PrimaryCategoryItem
    {
        /// <summary>
        /// 一级目录id
        /// </summary>
        public int primary_category_id { get; set; }
        /// <summary>
        /// 类目名称
        /// </summary>
        public string category_name { get; set; }
        /// <summary>
        /// 包含的二级类目
        /// </summary>
        public List<SecondaryCategoryItem> secondary_category { get; set; }
    }

    /// <summary>
    /// 二级类目
    /// </summary>
    public class SecondaryCategoryItem
    {
        /// <summary>
        /// 二级目录id
        /// </summary>
        public int secondary_category_id { get; set; }
        /// <summary>
        /// 类目名称
        /// </summary>
        public string category_name { get; set; }

       public List<string> need_qualification_stuffs { get; set; }

    }



    /// <summary>
    /// 提交创建子商户的返回的结果
    /// </summary>
    public class VipInsteadCardAuthResult
    {
        public InsteadCardAuthResultItem info { get; set; }
    }

    public class InsteadCardAuthResultItem
    {
        public int merchant_id { get; set; }
        public string app_id { get; set; }
        public int create_time { get; set; }
        public int update_time { get; set; }
        public string brand_name { get; set; }
        public string logo_url { get; set; }
        public string status { get; set; }
        public int begin_time { get; set; }
        public int end_time { get; set; }
        public int primary_category_id { get; set; }
        public int secondary_category_id { get; set; }

    }

}
