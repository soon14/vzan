using Entity.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.User
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class MiniAccount
    {
        #region Model

        [SqlField(IsPrimaryKey = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember]
        public Int32 Id { set; get; }

        /// <summary>
        /// 
        /// </summary>
        [Key]
        [SqlField]
        [DataMember]
        public String AccountId { set; get; }

        [SqlField]
        [DataMember]
        public String SystemId { set; get; }

        private string _masterId = string.Empty;

        [SqlField]
        [DataMember]
        public string MasterId
        {
            get { return _masterId; }
            set { _masterId = value; }
        }

        private DateTime _dayBegin = DateTime.Now; // ModificationDate

        [SqlField]
        [DataMember]
        public DateTime DayBegin
        {
            get { return _dayBegin; }
            set { _dayBegin = value; }
        }

        private DateTime _dayEnd = DateTime.Now; // ModificationDate

        [SqlField]
        [DataMember]
        public DateTime DayEnd
        {
            get { return _dayEnd; }
            set { _dayEnd = value; }
        }

        private string _loginid = string.Empty; // LoginId

        [SqlField]
        [DataMember]
        public string LoginId
        {
            get { return _loginid; }
            set { _loginid = value; }
        }

        private string _password = string.Empty; // Password

        [SqlField]
        [DataMember]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        private string _Aliasname = string.Empty; // Name

        [SqlField]
        [DataMember]
        public string AliasName
        {
            get { return _Aliasname; }
            set { _Aliasname = value; }
        }

        private int _roleValue = 0;

        /// <summary>
        /// 1.个人，2.企业
        /// </summary>
        [SqlField]
        [DataMember]
        public int RoleValue
        {
            get { return _roleValue; }
            set { _roleValue = value; }
        }


        private string _mark = string.Empty;

        [SqlField]
        [DataMember]
        public string Mark
        {
            get { return _mark; }
            set { _mark = value; }
        }

        private bool _isFullTime = true;

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        [DataMember]
        public bool IsFullTime
        {
            get { return _isFullTime; }
            set { _isFullTime = value; }
        }

        private string _address = string.Empty;

        [SqlField]
        [DataMember]
        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        private string _announce = string.Empty;

        [SqlField]
        [DataMember]
        public string Announce
        {
            get { return _announce; }
            set { _announce = value; }
        }

        private string _phone = string.Empty;

        [SqlField]
        [DataMember]
        public string Phone
        {
            get { return _phone; }
            set { _phone = value; }
        }

        private bool _isBindingWX = false;

        /// <summary>
        /// 激活状态
        /// </summary>
        [SqlField]
        [DataMember]
        public bool IsBindingWX
        {
            get { return _isBindingWX; }
            set { _isBindingWX = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotMapped]
        private TableItemState ItemState = TableItemState.None;

        private int _state = 0;

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        [DataMember]
        public int State
        {
            get { return _state; }
            set
            {
                _state = value;

                TableItemState state;

                Enum.TryParse<TableItemState>(this._state.ToString(), out state);

                this.ItemState = state;
            }
        }

        private bool _status = true;

        /// <summary>
        /// 激活状态
        /// </summary>
        [SqlField]
        [DataMember]
        public bool Status
        {
            get { return _status; }
            set { _status = value; }
        }

        private DateTime _creationdate = DateTime.Now; // CreationDate

        [SqlField]
        [DataMember]
        public DateTime CreationDate
        {
            get { return _creationdate; }
            set { _creationdate = value; }
        }

        private DateTime _modificationDate = DateTime.Now; // ModificationDate

        [SqlField]
        [DataMember]
        public DateTime ModificationDate
        {
            get { return _modificationDate; }
            set { _modificationDate = value; }
        }

        private bool _isClientVisible = true;

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        [DataMember]
        public bool IsClientVisible
        {
            get { return _isClientVisible; }
            set { _isClientVisible = value; }
        }

        private string _midifier = string.Empty;

        [SqlField]
        [DataMember]
        public string Midifier
        {
            get { return _midifier; }
            set { _midifier = value; }
        }

        private int _parentId = 0;
        [SqlField]
        [DataMember]
        public int ParentId
        {
            get { return _parentId; }
            set { _parentId = value; }
        }

        private int _templateId = 0;
        [SqlField]
        [DataMember]
        public int TemplateId
        {
            get { return _templateId; }
            set { _templateId = value; }
        }

        #endregion Model 
    }

    ///// <summary>
    ///// MiniAccount:实体类(属性说明自动提取数据库字段的描述信息)
    ///// </summary>
    //[Serializable]
    //[SqlTable(dbEnum.QLWL)]
    //public class MiniAccount : EntityBase
    //{
    //    #region Model

    //    private Guid _id = Guid.NewGuid(); // Id

    //    [SqlField(IsPrimaryKey = true)]
    //    [DataMember]
    //    public Guid Id
    //    {
    //        get { return _id; }
    //        set { _id = value; }
    //    }

    //    private string _masterId = string.Empty;

    //    [SqlField]
    //    [DataMember]
    //    public string MasterId
    //    {
    //        get { return _masterId; }
    //        set { _masterId = value; }
    //    }

    //    private DateTime _dayBegin = DateTime.Now; // ModificationDate

    //    [SqlField]
    //    [DataMember]
    //    public DateTime DayBegin
    //    {
    //        get { return _dayBegin; }
    //        set { _dayBegin = value; }
    //    }

    //    private DateTime _dayEnd = DateTime.Now; // ModificationDate

    //    [SqlField]
    //    [DataMember]
    //    public DateTime DayEnd
    //    {
    //        get { return _dayEnd; }
    //        set { _dayEnd = value; }
    //    }

    //    private string _loginid = string.Empty; // LoginId

    //    [SqlField]
    //    [DataMember]
    //    public string LoginId
    //    {
    //        get { return _loginid; }
    //        set { _loginid = value; }
    //    }

    //    private string _password = string.Empty; // Password

    //    [SqlField]
    //    [DataMember]
    //    public string Password
    //    {
    //        get { return _password; }
    //        set { _password = value; }
    //    }

    //    private string _name = string.Empty; // Name

    //    [SqlField]
    //    [DataMember]
    //    public string Name
    //    {
    //        get { return _name; }
    //        set { _name = value; }
    //    }

    //    private int _roleValue = 0;

    //    /// <summary>
    //    /// 1.个人，2.企业
    //    /// </summary>
    //    [SqlField]
    //    [DataMember]
    //    public int RoleValue
    //    {
    //        get { return _roleValue; }
    //        set { _roleValue = value; }
    //    }


    //    private string _mark = string.Empty;

    //    [SqlField]
    //    [DataMember]
    //    public string Mark
    //    {
    //        get { return _mark; }
    //        set { _mark = value; }
    //    }

    //    private bool _isFullTime = true;

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    [SqlField]
    //    [DataMember]
    //    public bool IsFullTime
    //    {
    //        get { return _isFullTime; }
    //        set { _isFullTime = value; }
    //    }

    //    private string _address = string.Empty;

    //    [SqlField]
    //    [DataMember]
    //    public string Address
    //    {
    //        get { return _address; }
    //        set { _address = value; }
    //    }

    //    private string _announce = string.Empty;

    //    [SqlField]
    //    [DataMember]
    //    public string Announce
    //    {
    //        get { return _announce; }
    //        set { _announce = value; }
    //    }

    //    private string _phone = string.Empty;

    //    [SqlField]
    //    [DataMember]
    //    public string Phone
    //    {
    //        get { return _phone; }
    //        set { _phone = value; }
    //    }


    //    private string _email = string.Empty;

    //    [SqlField]
    //    [DataMember]
    //    public string Email
    //    {
    //        get { return _email; }
    //        set { _email = value; }
    //    }

    //    private bool _isBindingWX = false;

    //    /// <summary>
    //    /// 激活状态
    //    /// </summary>
    //    [SqlField]
    //    [DataMember]
    //    public bool IsBindingWX
    //    {
    //        get { return _isBindingWX; }
    //        set { _isBindingWX = value; }
    //    }

    //    #endregion Model 
    //}
}
