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
    /// 商品表
    /// </summary>
    [SqlTable(dbEnum.QLWL)]
    public class Product
    {

        private int _productcode = 0;
        /// <summary>
        /// auto_increment
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int ProductCode
        {
            get { return _productcode; }
            set { _productcode = value; }
        }
        private DateTime _createtiondate = DateTime.Now;
        /// <summary>
        /// CreatetionDate
        /// </summary>
        [SqlField]
        public DateTime CreatetionDate
        {
            get { return _createtiondate; }
            set { _createtiondate = value; }
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
        private string _productname = string.Empty;
        /// <summary>
        /// ProductName
        /// </summary>
        [SqlField]
        public string ProductName
        {
            get { return _productname; }
            set { _productname = value; }
        }
        private string _classcode = string.Empty;
        /// <summary>
        /// ClassCode
        /// </summary>
        [SqlField]
        public string ClassCode
        {
            get { return _classcode; }
            set { _classcode = value; }
        }
        private string _commontitle = string.Empty;
        /// <summary>
        /// CommonTitle
        /// </summary>
        [SqlField]
        public string CommonTitle
        {
            get { return _commontitle; }
            set { _commontitle = value; }
        }
        private string _introduction = string.Empty;
        /// <summary>
        /// Introduction
        /// </summary>
        [SqlField]
        public string Introduction
        {
            get { return _introduction; }
            set { _introduction = value; }
        }
        private bool _isvisiable = true;
        /// <summary>
        /// IsVisiable
        /// </summary>
        [SqlField]
        public bool IsVisiable
        {
            get { return _isvisiable; }
            set { _isvisiable = value; }
        }
        private float _saleprice = 0;
        /// <summary>
        /// SalePrice
        /// </summary>
        [SqlField]
        public float SalePrice
        {
            get { return _saleprice; }
            set { _saleprice = value; }
        }
        private string _unit = string.Empty;
        /// <summary>
        /// Unit
        /// </summary>
        [SqlField]
        public string Unit
        {
            get { return _unit; }
            set { _unit = value; }
        }
        private int _productstatus = 1;
        /// <summary>
        /// ProductStatus
        /// </summary>
        [SqlField]
        public int ProductStatus
        {
            get { return _productstatus; }
            set { _productstatus = value; }
        }
        private string _producturl = string.Empty;
        /// <summary>
        /// ProductUrl
        /// </summary>
        [SqlField]
        public string ProductUrl
        {
            get { return _producturl; }
            set { _producturl = value; }
        }

    }
}