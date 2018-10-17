using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Qiye
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class QiyePostMsg
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int Aid { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        [SqlField]
        public int UserId { get; set; }

        /// <summary>
        /// 信息详情
        /// </summary>
        [SqlField]
        public string MsgDetail { get; set; }


        /// <summary>
        /// 图片 以逗号分隔
        /// </summary>
        [SqlField]
        public string Imgs { get; set; }

        public List<string> ImgList
        {

            get
            {
                List<string> imgArry = new List<string>();
                if (!string.IsNullOrEmpty(Imgs))
                {
                    imgArry = Imgs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                return imgArry;
            }

        }


        /// <summary>
        /// 状态0正常 -1 删除
        /// </summary>
        [SqlField]
        public int State { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        public string AddTimeStr
        {

            get
            {
                return AddTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }


    }
}
