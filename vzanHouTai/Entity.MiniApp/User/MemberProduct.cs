using System;
using Entity.Base;
using Utility;

/// <summary>
/// Member转移过来的
/// </summary>
namespace Entity.MiniApp
{
    [Serializable]
    /// <summary>
    ///memberproduct:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [SqlTable(dbEnum.QLWL)]
    public class MemberProduct
    {

        private int _id = 0;
        /// <summary>
        /// auto_increment
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private Guid _accountid = new Guid();
        /// <summary>
        /// 用户
        /// </summary>
        [SqlField]
        public Guid AccountId
        {
            get { return _accountid; }
            set { _accountid = value; }
        }
        private int _productcode = 0;
        /// <summary>
        /// 商品
        /// </summary>
        [SqlField]
        public int ProductCode
        {
            get { return _productcode; }
            set { _productcode = value; }
        }
        private DateTime _creationdate = DateTime.Now;
        /// <summary>
        /// 第一次购买时间
        /// </summary>
        [SqlField]
        public DateTime CreationDate
        {
            get { return _creationdate; }
            set { _creationdate = value; }
        }
        private DateTime _lastmodified = DateTime.Now;
        /// <summary>
        /// LastModified
        /// </summary>
        [SqlField]
        public DateTime LastModified
        {
            get { return _lastmodified; }
            set { _lastmodified = value; }
        }
        private int _statustype = 1;
        /// <summary>
        /// 使用状态（1.使用中，2.被封锁）
        /// </summary>
        [SqlField]
        public int StatusType
        {
            get { return _statustype; }
            set { _statustype = value; }
        }
        private DateTime _invalidtime = DateTime.Now;
        /// <summary>
        /// 失效日期
        /// </summary>
        [SqlField]
        public DateTime InvalidTime
        {
            get { return _invalidtime; }
            set { _invalidtime = value; }
        }

        private int _producttype = 1;
        /// <summary>
        /// 商品类别（1.普通收费商品，2.微社区）
        /// </summary>
        [SqlField]
        public int ProductType
        {
            get { return _producttype; }
            set { _producttype = value; }
        }

    }
}