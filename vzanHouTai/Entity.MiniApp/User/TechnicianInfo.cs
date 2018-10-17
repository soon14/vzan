using Entity.Base;
using Entity.MiniApp.Ent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.User
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class TechnicianInfo
    {
        /// <summary>
        /// 足浴版-技师信息表
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; } = 0;

        [SqlField]
        public int appId { get; set; } = 0;

        [SqlField]
        public int storeId { get; set; } = 0;
        /// <summary>
        /// 关注公众号unionId
        /// </summary>
        [SqlField]
        public string unionId { get; set; } = string.Empty;

        /// <summary>
        /// 工号
        /// </summary>
        [SqlField]
        public string jobNumber { get; set; } = string.Empty;

        /// <summary>
        /// 姓名
        /// </summary>
        [SqlField]
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// 微信昵称
        /// </summary>
        public string wxname { get; set; } = string.Empty;
        /// <summary>
        /// 性别 
        /// </summary>
        [SqlField]
        public int sex { get; set; } = 0;

        /// <summary>
        /// 头像
        /// </summary>
        [SqlField]
        public string headImg { get; set; } = string.Empty;

        /// <summary>
        /// 生日
        /// </summary>
        [SqlField]
        public DateTime birthday { get; set; }
        public int age
        {
            get
            {
                return DateTime.Now.Year - birthday.Year;
            }
        }

        public string showBirthday { get; set; }

        [SqlField]
        public string phone { get; set; } = string.Empty;

        /// <summary>
        /// 简介
        /// </summary>
        [SqlField]
        public string desc { get; set; } = string.Empty;

        /// <summary>
        /// 接单数
        /// </summary>
        [SqlField]
        public int serviceCount { get; set; } = 0;

        /// <summary>
        /// 接单基数
        /// ·
        /// ·
        /// ·
        /// ·
        /// ·
        /// ·
        /// 就是造假基数啦
        /// </summary>
        [SqlField]
        public int baseCount { get; set; } = 0;

        /// <summary>
        /// 可以服务项目id
        /// </summary>
        [SqlField]
        public string itemId { get; set; } = string.Empty;

        public List<EntGoods> serviceList { get; set; }
        /// <summary>
        /// 可服务的项目名称
        /// </summary>
        public string serviceItems { get; set; } = string.Empty;
        /// <summary>
        /// 配置
        /// </summary>
        [SqlField]
        public string switchConfig { get; set; } = string.Empty;


        public TechnicianSwitch switchModel { get; set; }

        /// <summary>
        /// 相册
        /// </summary>
        [SqlField]
        public string photo { get; set; } = string.Empty;



        /// <summary>
        /// 状态 见枚举 TechnicianState
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        public string stateName
        {
            get
            {
                return  Enum.GetName(typeof(TechnicianState), state); //GetStateName(state);
            }
        }
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime createDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime updateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 获取工作状态名称
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public string GetStateName(int state)
        {
            string result = string.Empty;
            switch (state)
            {
                case 0: result = "空闲"; break;
                case 1: result = "上钟"; break;
                case 2: result = "将下钟"; break;
                case 3: result = "休息中"; break;
            }
            return result;
        }

        /// <summary>
        /// 手机号码
        /// </summary>
        [SqlField]
        public string TelPhone { get; set; } = string.Empty;

        /// <summary>
        /// 收到的花朵
        /// </summary>
        [SqlField]
        public int GetItGiftCount { get; set; } = 0;
    }

    public class TechnicianSwitch
    {
        public bool showIndex { get; set; } = true;
    }
}
