using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    [Serializable]
    public class RightItem
    {
        /// <summary>
        /// 发帖
        /// </summary>
        public bool AddTopic { get; set; }

        /// <summary>
        /// 删除主贴
        /// </summary>
        public bool DelTopic { get; set; }
        /// <summary>
        /// 回复
        /// </summary>
        public bool AddReply { get; set; }

        /// <summary>
        /// 删回复
        /// </summary>
        public bool DelReply { get; set; }
        /// <summary>
        /// 接受打赏
        /// </summary>
        public bool AddReward { get; set; }
        /// <summary>
        /// 打赏后看答案
        /// </summary>
        public bool RewardAnswer { get; set; }

        /// <summary>
        /// 聊天
        /// </summary>
        public bool IsChat { get; set; }

        /// <summary>
        /// 找附近的
        /// </summary>
        public bool IsNearby { get; set; }


        /// <summary>
        /// 管理论坛
        /// </summary>
        public bool ManagerForum { get; set; }

        /// <summary>
        /// 全局拉黑
        /// </summary>
        public bool BlackAll { get; set; }

        /// <summary>
        /// 普通拉黑
        /// </summary>
        public bool BlackOne { get; set; }

        /// <summary>
        /// 全站置顶
        /// </summary>
        public bool TopAll { get; set; }

        /// <summary>
        /// 版块置顶
        /// </summary>
        public bool TopOne { get; set; }

        /// <summary>
        /// 删除论坛，超级管理员和创建者的权限
        /// </summary>
        public bool DeleteMinisns { get; set; }
    }
}