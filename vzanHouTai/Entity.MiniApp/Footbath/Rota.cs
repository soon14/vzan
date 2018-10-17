using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Footbath
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class Rota
    {
        /// <summary>
        /// 自增id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 小程序id
        /// </summary>
        [SqlField]
        public int aid { get; set; } = 0;

        /// <summary>
        /// 店铺id
        /// </summary>
        [SqlField]
        public int storeId { get; set; } = 0;

        /// <summary>
        /// 技师id
        /// </summary>
        [SqlField]
        public int tid { get; set; } = 0;

        /// <summary>
        /// 技师工号
        /// </summary>
        public string tname { get; set; } = string.Empty;
        /// <summary>
        /// 周类型：0单周  1双周
        /// </summary>
        [SqlField]
        public int dayType { get; set; } = 0;
        public string dayTypeName
        {
            get
            {
                return dayType == 0 ? "单周" : "双周";
            }
        }
        /// <summary>
        /// 周一值班 类型见RotaType
        /// </summary>
        [SqlField]
        public string monday { get; set; } = string.Empty;

        //public string mondayName { get; set; } = string.Empty;
        public WorkTimeState mondayState { get; set; }
        /// <summary>
        /// 周二值班 类型见RotaType
        /// </summary>
        [SqlField]
        public string tuesday { get; set; } = string.Empty;
        //public string tuesdayName { get; set; } = string.Empty;
        public WorkTimeState tuesdayState { get; set; }
        /// <summary>
        /// 周三值班 类型见RotaType
        /// </summary>
        [SqlField]
        public string wensday { get; set; } = string.Empty;
        //public string wensdayName { get; set; } = string.Empty;
        public WorkTimeState wensdayState { get; set; }

        /// <summary>
        /// 周四值班 类型见RotaType
        /// </summary>
        [SqlField]
        public string thursday { get; set; } = string.Empty;
        //public string thursdayName { get; set; } = string.Empty;
        public WorkTimeState thursdayState { get; set; }

        /// <summary>
        /// 周五值班 类型见RotaType
        /// </summary>
        [SqlField]
        public string friday { get; set; } = string.Empty;
        //public string fridayName { get; set; } = string.Empty;
        public WorkTimeState fridayState { get; set; }

        /// <summary>
        /// 周六值班 类型见RotaType
        /// </summary>
        [SqlField]
        public string saturday { get; set; } = string.Empty;
        //public string saturdayName { get; set; } = string.Empty;
        public WorkTimeState saturdayState { get; set; }

        /// <summary>
        /// 周日值班 类型见RotaType
        /// </summary>
        [SqlField]
        public string sunday { get; set; } = string.Empty;
        //public string sundayName { get; set; } = string.Empty;
        public WorkTimeState sundayState { get; set; }

        /// <summary>
        /// -1：删除 ， 0：正常
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;
    }
    /// <summary>
    /// 值班状态
    /// </summary>
    public class WorkTimeState
    {
        public bool morning { get; set; } = false;
        public bool noon { get; set; } = false;
        public bool evening { get; set; } = false;
    }
}
