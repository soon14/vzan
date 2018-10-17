using Entity.Base;
using System;
using System.Collections.Generic;
using Utility;
using Entity.MiniApp.User;

namespace Entity.MiniApp
{

    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class UserCashLog
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
        /// addtime
        /// </summary>		
        private DateTime _addtime = DateTime.Now;
        [SqlField]
        public DateTime addtime
        {
            get { return _addtime; }
            set { _addtime = value; }
        }
        ///<summary>
        /// userid  用户昵称
        /// </summary>		
        private string _nickname;
        [SqlField]
        public string nickname
        {
            get { return _nickname; }
            set { _nickname = value; }
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
        /// usercash 用户提成之后的总金额  
        /// </summary>		
        private int _usercash;
        [SqlField]
        public int usercash
        {
            get { return _usercash; }
            set { _usercash = value; }
        }
        ///<summary>
        /// type 收益类型
        /// </summary>		
        private int _cashtype;
        [SqlField]
        public int cashtype
        {
            get { return _cashtype; }
            set { _cashtype = value; }
        }
        ///<summary>
        /// money 本次提成收益金额
        /// </summary>		
        private int _cash;
        [SqlField]
        public int cash
        {
            get { return _cash; }
            set { _cash = value; }
        }	
        private int _vircash;
        ///<summary>
        /// money 订单原始总金额
        /// </summary>	
        [SqlField]
        public int vircash
        {
            get { return _vircash; }
            set { _vircash = value; }
        }
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
        /// percent 本次提成比例
        /// </summary>		
        private int _percent;
        [SqlField]
        public int percent
        {
            get { return _percent; }
            set { _percent = value; }
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
        /// isin 1==收益  0==支出
        /// </summary>		
        private int _isin;
        [SqlField]
        public int isin
        {
            get { return _isin; }
            set { _isin = value; }
        }

        /// <summary>
        /// 受益类型，用于视图显示
        /// </summary>
        public string cashName { get; set; }
    }
    
}