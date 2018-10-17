using Entity.Base;
using System;
using Utility;
namespace Entity.MiniApp
{

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class ReFundQueue
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
        /// userid
        /// </summary>		
        private int _userid;
        [SqlField]
        public int userid
        {
            get { return _userid; }
            set { _userid = value; }
        }
        ///<summary>
        /// addtime
        /// </summary>		
        private DateTime _addtime;
        [SqlField]
        public DateTime addtime
        {
            get { return _addtime; }
            set { _addtime = value; }
        }
        ///<summary>
        /// minisnsId
        /// </summary>		
        private int _minisnsid;
        [SqlField]
        public int minisnsId
        {
            get { return _minisnsid; }
            set { _minisnsid = value; }
        }
        ///<summary>
        /// orderid
        /// </summary>		
        private int _orderid;
        [SqlField]
        public int orderid
        {
            get { return _orderid; }
            set { _orderid = value; }
        }
        ///<summary>
        /// traid
        /// </summary>		
        private string _traid;
        [SqlField]
        public string traid
        {
            get { return _traid; }
            set { _traid = value; }
        }
        ///<summary>
        /// trano
        /// </summary>		
        private string _trano;
        [SqlField]
        public string trano
        {
            get { return _trano; }
            set { _trano = value; }
        }
        ///<summary>
        /// trano
        /// </summary>		
        private int _money;
        [SqlField]
        public int money
        {
            get { return _money; }
            set { _money = value; }
        }
        ///<summary>
        /// trano
        /// </summary>		
        private string _note;
        [SqlField]
        public string note
        {
            get { return _note; }
            set { _note = value; }
        }
        ///<summary>
        /// trano
        /// </summary>		
        private int _state;
        [SqlField]
        public int state
        {
            get { return _state; }
            set { _state = value; }
        }
        ///<summary>
        /// 退款类型：0，论坛，1同城，2广告退款
        /// </summary>		
        [SqlField]
        public int retype
        {
            get;
            set;
        } = 0;
        
    }


    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class ReFundQueueReport
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
        /// userid
        /// </summary>		
        private int _userid;
        [SqlField]
        public int userid
        {
            get { return _userid; }
            set { _userid = value; }
        }
        ///<summary>
        /// addtime
        /// </summary>		
        private DateTime _addtime;
        [SqlField]
        public DateTime addtime
        {
            get { return _addtime; }
            set { _addtime = value; }
        }
        ///<summary>
        /// minisnsId
        /// </summary>		
        private int _minisnsid;
        [SqlField]
        public int minisnsId
        {
            get { return _minisnsid; }
            set { _minisnsid = value; }
        }
        ///<summary>
        /// orderid
        /// </summary>		
        private int _orderid;
        [SqlField]
        public int orderid
        {
            get { return _orderid; }
            set { _orderid = value; }
        }
        ///<summary>
        /// traid
        /// </summary>		
        private string _traid;
        [SqlField]
        public string traid
        {
            get { return _traid; }
            set { _traid = value; }
        }
        ///<summary>
        /// trano
        /// </summary>		
        private string _trano;
        [SqlField]
        public string trano
        {
            get { return _trano; }
            set { _trano = value; }
        }
        ///<summary>
        /// trano
        /// </summary>		
        private int _money;
        [SqlField]
        public int money
        {
            get { return _money; }
            set { _money = value; }
        }
        ///<summary>
        /// trano
        /// </summary>		
        private string _note;
        [SqlField]
        public string note
        {
            get { return _note; }
            set { _note = value; }
        }
        ///<summary>
        /// trano
        /// </summary>		
        private int _state;
        [SqlField]
        public int state
        {
            get { return _state; }
            set { _state = value; }
        }
        ///<summary>
        /// 退款类型：0，论坛，1同城，2广告退款
        /// </summary>		
        [SqlField]
        public int retype
        {
            get;
            set;
        } = 0;


        [SqlField]
        public string state_name { get; set; }
        [SqlField]
        public string result_msg { get; set; }
        [SqlField]
        public string result_note { get; set; }
    }

    /// <summary>
    /// 报表日,周,月 统计金额
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class ReFundQueueReportAmount
    {
        [SqlField]
        public double day_amount { get; set; } = 0;
        [SqlField]
        public double week_amount { get; set; } = 0;
        [SqlField]
        public double month_amount { get; set; } = 0;
    }
}