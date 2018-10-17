using Entity.Base;
using System;
using Utility;
namespace Entity.MiniApp
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class ReFundResult
    {
        ///<summary>
        /// auto_increment
        /// </summary>		
        private int _id;
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id
        {
            get { return _id; }
            set { _id = value; }
        }
        ///<summary>
        /// return_code
        /// </summary>		
        private string _return_code;
        [SqlField]
        public string return_code
        {
            get { return _return_code; }
            set { _return_code = value; }
        }
        ///<summary>
        /// return_msg
        /// </summary>		
        private string _return_msg;
        [SqlField]
        public string return_msg
        {
            get { return _return_msg; }
            set { _return_msg = value; }
        }
        ///<summary>
        /// result_code
        /// </summary>		
        private string _result_code;
        [SqlField]
        public string result_code
        {
            get { return _result_code; }
            set { _result_code = value; }
        }
        ///<summary>
        /// err_code
        /// </summary>		
        private string _err_code;
        [SqlField]
        public string err_code
        {
            get { return _err_code; }
            set { _err_code = value; }
        }
        ///<summary>
        /// err_code_des
        /// </summary>		
        private string _err_code_des;
        [SqlField]
        public string err_code_des
        {
            get { return _err_code_des; }
            set { _err_code_des = value; }
        }
        ///<summary>
        /// appid
        /// </summary>		
        private string _appid;
        [SqlField]
        public string appid
        {
            get { return _appid; }
            set { _appid = value; }
        }
        ///<summary>
        /// mch_id
        /// </summary>		
        private string _mch_id;
        [SqlField]
        public string mch_id
        {
            get { return _mch_id; }
            set { _mch_id = value; }
        }
        ///<summary>
        /// device_info
        /// </summary>		
        private string _device_info;
        [SqlField]
        public string device_info
        {
            get { return _device_info; }
            set { _device_info = value; }
        }
        ///<summary>
        /// nonce_str
        /// </summary>		
        private string _nonce_str;
        [SqlField]
        public string nonce_str
        {
            get { return _nonce_str; }
            set { _nonce_str = value; }
        }
        ///<summary>
        /// sign
        /// </summary>		
        private string _sign;
        [SqlField]
        public string sign
        {
            get { return _sign; }
            set { _sign = value; }
        }
        ///<summary>
        /// transaction_id
        /// </summary>		
        private string _transaction_id;
        [SqlField]
        public string transaction_id
        {
            get { return _transaction_id; }
            set { _transaction_id = value; }
        }
        ///<summary>
        /// out_trade_no
        /// </summary>		
        private string _out_trade_no;
        [SqlField]
        public string out_trade_no
        {
            get { return _out_trade_no; }
            set { _out_trade_no = value; }
        }
        ///<summary>
        /// out_refund_no
        /// </summary>		
        private string _out_refund_no;
        [SqlField]
        public string out_refund_no
        {
            get { return _out_refund_no; }
            set { _out_refund_no = value; }
        }
        ///<summary>
        /// refund_id
        /// </summary>		
        private string _refund_id;
        [SqlField]
        public string refund_id
        {
            get { return _refund_id; }
            set { _refund_id = value; }
        }
        ///<summary>
        /// refund_channel
        /// </summary>		
        private string _refund_channel;
        [SqlField]
        public string refund_channel
        {
            get { return _refund_channel; }
            set { _refund_channel = value; }
        }
        ///<summary>
        /// refund_fee
        /// </summary>		
        private int _refund_fee;
        [SqlField]
        public int refund_fee
        {
            get { return _refund_fee; }
            set { _refund_fee = value; }
        }
        ///<summary>
        /// total_fee
        /// </summary>		
        private int _total_fee;
        [SqlField]
        public int total_fee
        {
            get { return _total_fee; }
            set { _total_fee = value; }
        }
        ///<summary>
        /// fee_type
        /// </summary>		
        private string _fee_type;
        [SqlField]
        public string fee_type
        {
            get { return _fee_type; }
            set { _fee_type = value; }
        }
        ///<summary>
        /// cash_fee
        /// </summary>		
        private int _cash_fee;
        [SqlField]
        public int cash_fee
        {
            get { return _cash_fee; }
            set { _cash_fee = value; }
        }
        ///<summary>
        /// cash_refund_fee
        /// </summary>		
        private int _cash_refund_fee;
        [SqlField]
        public int cash_refund_fee
        {
            get { return _cash_refund_fee; }
            set { _cash_refund_fee = value; }
        }
        ///<summary>
        /// coupon_refund_fee
        /// </summary>		
        private int _coupon_refund_fee;
        [SqlField]
        public int coupon_refund_fee
        {
            get { return _coupon_refund_fee; }
            set { _coupon_refund_fee = value; }
        }
        ///<summary>
        /// coupon_refund_count
        /// </summary>		
        private int _coupon_refund_count;
        [SqlField]
        public int coupon_refund_count
        {
            get { return _coupon_refund_count; }
            set { _coupon_refund_count = value; }
        }
        ///<summary>
        /// coupon_refund_id
        /// </summary>		
        private string _coupon_refund_id;
        [SqlField]
        public string coupon_refund_id
        {
            get { return _coupon_refund_id; }
            set { _coupon_refund_id = value; }
        }
        ///<summary>
        /// userid
        /// </summary>		
        private int _userid;
        [SqlField]
        public int userid
        {
            get { return _userid; }
            set { _userid = value; }
        }
        [SqlField]
        public int retype
        {
            get;
            set;
        } = 0;
        ///<summary>
        /// minisnsid
        /// </summary>		
        private int _minisnsid;
        [SqlField]
        public int minisnsid
        {
            get { return _minisnsid; }
            set { _minisnsid = value; }
        }
        ///<summary>
        /// minisnsid
        /// </summary>		
        private string _note;
        [SqlField]
        public string note
        {
            get { return _note; }
            set { _note = value; }
        }
        ///<summary>
        /// minisnsid
        /// </summary>		
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;

    }
}