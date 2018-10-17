using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Qiye
{
    /// <summary>
    /// 企业员工信息表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class QiyeEmployee
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 小程序appId 
        /// </summary>
        [SqlField]
        public int Aid { get; set; }

        /// <summary>
        /// AppId
        /// </summary>
        [SqlField]
        public string AppId { get; set; }



        /// <summary>
        /// 所属部门ID
        /// </summary>
        [SqlField]
        public int DepartmentId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [SqlField]
        public string Name { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        [SqlField]
        public string Avatar { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        [SqlField]
        public string Phone { get; set; }

        /// <summary>
        /// 在职状态 0表示在职 -1表示离职
        /// </summary>
        [SqlField]
        public int WorkState { get; set; }


        /// <summary>
        /// 员工码（表示）
        /// </summary>
        [SqlField]
        public string WorkID { get; set; }

        /// <summary>
        ///员工在C_UserInfo里的ID 前端通过员工码绑定过来
        /// </summary>
        [SqlField]
        public int UserId { get; set; }


        /// <summary>
        /// 是否是公司客服 0不是 1是
        /// </summary>
        [SqlField]
        public int Kefu { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }


        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 职位
        /// </summary>
        [SqlField]
        public string Job { get; set; }


        /// <summary>
        /// 名片码
        /// </summary>
        [SqlField]
        public string Qrcode { get; set; }


        /// <summary>
        /// 状态 0正常 -1删除
        /// </summary>
        [SqlField]
        public int State { get; set; }


        /// <summary>
        /// 微信号
        /// </summary>
        [SqlField]
        public string WxAccount { get; set; }


        /// <summary>
        /// 邮箱
        /// </summary>
        [SqlField]
        public string Email { get; set; }



        /// <summary>
        /// 客户数量
        /// </summary>
        public int CustomerNum { get; set; }

        /// <summary>
        /// 访问量
        /// </summary>
        public int PV { get; set; }

        /// <summary>
        /// 所属部门名称
        /// </summary>
        public string DepartMentName { get; set; }

        /// <summary>
        /// 微信绑定状态
        /// </summary>
        public string WxBindStateStr
        {
            get
            {
                return UserId > 0 ? "已绑定" : "未绑定";
            }
        }

        /// <summary>
        /// 员工绑定的微信用户信息
        /// </summary>
        public WxUserInfo WxBindUserInfo { get; set; } = new WxUserInfo();


        /// <summary>
        /// 在职状态
        /// </summary>
        public string WorkStateStr
        {
            get
            {
                return WorkState == 0 ? "在职" : "已离职";
            }
        }
        /// <summary>
        /// 是否是客服
        /// </summary>
        public string KefuStateStr
        {
            get
            {
                return Kefu == 0 ? "不是" : "是";
            }
        }

        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 公司地址
        /// </summary>
        public string CompanyAddress { get; set; }

        /// <summary>
        /// 公司地址经纬度
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 公司座机电话
        /// </summary>
        public string CompanyPhone { get; set; }

        /// <summary>
        /// 聊天消息数量
        /// </summary>
        public int MsgCount { get; set; }

        /// <summary>
        /// 点赞数量
        /// </summary>
        public int DzCount { get; set; }

        /// <summary>
        /// 转发数量
        /// </summary>
        public int ShareCount { get; set; }


        /// <summary>
        /// 推荐动态
        /// </summary>
        public List<QiyePostMsg> listQiyePostMsg { get; set; } = new List<QiyePostMsg>();


        /// <summary>
        /// 推荐商品
        /// </summary>
        public List<QiyeGoods> listQiyeGoods { get; set; } = new List<QiyeGoods>();

        /// <summary>
        /// 来源 用于名片列表，表示我浏览的名片记录来源
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 当前登录用户对该名片是否点赞过
        /// </summary>
        public bool IsDzed { get; set; }
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsCheck { get; set; } = false;

    }


    /// <summary>
    /// 员工绑定的微信号信息
    /// </summary>
    public class WxUserInfo
    {

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string Avatar { get; set; }

    }


    /// <summary>
    /// 名片数据雷达
    /// </summary>
    public class DataList
    {
        /// <summary>
        /// 人气
        /// </summary>
        public int PV { get; set; }

        /// <summary>C
        /// 点赞数量
        /// </summary>
        public int DzCount { get; set; }

        /// <summary>
        /// 转发数量
        /// </summary>
        public int ShareCount { get; set; }


        /// <summary>
        /// 当月累积顾客数量
        /// </summary>
        public int MonthTotalCustomer { get; set; }

        /// <summary>
        /// 总顾客数量
        /// </summary>
        public int TotalCustomer { get; set; }


    }


}
