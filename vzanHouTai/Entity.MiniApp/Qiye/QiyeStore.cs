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
    /// 企业店铺信息配置
    /// </summary>

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class QiyeStore
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId   小程序
        /// </summary>
        [SqlField]
        public int Aid { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        [SqlField]
        public string Phone { get; set; }


        /// <summary>
        /// 地址(用于取货)
        /// </summary>
        [SqlField]
        public string Location { get; set; }

        /// <summary>
        /// 地址 经度
        /// </summary>
        [SqlField]
        public double Lng { get; set; }


        /// <summary>
        ///  纬度
        /// </summary>
        [SqlField]
        public double Lat { get; set; }


        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }


        /// <summary>
        /// 开关设置
        /// </summary>
        [SqlField]
        public string SwitchConfig { get; set; } = string.Empty;


        /// <summary>
        /// 开关设置模型
        /// </summary>
        public QiyeStoreSwitchModel SwitchModel { get; set; } = new QiyeStoreSwitchModel();

        public QiyeStoreSwitchModel SwitchConfigModel
        {
            get
            {
                if (string.IsNullOrEmpty(SwitchConfig))
                {
                    return new QiyeStoreSwitchModel();
                }
                else
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<QiyeStoreSwitchModel>(SwitchConfig);
                }
            }
        }

        public string Name { get; set; }
    }





    public class QiyeStoreSwitchModel
    {

        /// <summary>
        /// 到店自取开关
        /// </summary>
        public bool SwitchReceiving { get; set; } = false;


        /// <summary>
        /// 运费计算方式（枚举：DeliveryFeeSumMethond）
        /// </summary>
        public int deliveryFeeSumMethond { get; set; }

        /// <summary>
        /// 启用新运费模板功能
        /// </summary>
        public bool enableDeliveryTemplate { get; set; } = true;

    }


}
