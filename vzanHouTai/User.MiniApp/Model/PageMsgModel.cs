using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Entity.MiniApp;

namespace User.MiniApp.Model
{
    /// <summary>
    /// 错误提示页
    /// </summary>
    public class PageMsgModel
    {
        //默认为 WeUIMsgType.ParameterIsNotComplete 类型的错误
        public PageMsgModel()
        {
            this.Title = "非法请求";
            this.Description = "未提供完整或可用参数！";
        }
        public PageMsgModel(WeUIMsgType msgtype)
        {
            switch (msgtype)
            {
                case WeUIMsgType.TopicIsNull:
                    this.Title = "话题不存在。";
                    this.Description = "话题不存在，或已删除，无法查看";
                    break;
                //未审核说明话题存在可以返回直播间，需要提供直播间
                case WeUIMsgType.TopicIsNotAudit:
                    this.Title = "此话题未通过审核，无法查看。";
                    this.Description = "经核实，此话题涉嫌违反相关法律法规，查看<a href='http://mp.weixin.qq.com/mp/opshowpage?action=oplaw&id=32'>对应规则</a>。";
                    break;
                case WeUIMsgType.LiveRoomIsNull:
                    this.Title = "直播间不存在";
                    this.Description = "直播间不存在，或已删除，无法查看";
                    break;
                //未审核通过
                case WeUIMsgType.LiveIsNotAudit:
                    this.Title = "此直播间未通过审核，无法查看。";
                    this.Description = "经核实，此直播间涉嫌违反相关法律法规，查看<a href='http://mp.weixin.qq.com/mp/opshowpage?action=oplaw&id=32' style='color:red'>对应规则</a>。";
                    break;
                case WeUIMsgType.ChannelIsNull:
                    this.Title = "频道不存在。";
                    this.Description = "频道不存在，或已删除，无法查看";
                    break;
            }
        }

        /// <summary>
        /// 无title,无button(只有返回),默认图标:提示
        /// </summary>
        /// <param name="description"></param>
        /// <param name="IconType"></param>
        public PageMsgModel(string description, WeUIIconTypes iconType = WeUIIconTypes.weui_icon_info, params WeUIButton[] button)
        {
            this.Title = "";
            this.Description = description;
            this.IconType = iconType;
            if (button == null || button.Length <= 0)
                this.Buttons = new List<WeUIButton>();
            else
                this.Buttons = button.ToList();
        }

        /// <summary>
        /// 当前直播间
        /// </summary>
        //public zbSiteInfo CurrentSite { get; set; }
        /// <summary>
        /// 当前话题
        /// </summary>
        //public zbTopics CurrentTopic { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// 图标 默认是错误图标
        /// </summary>
        public WeUIIconTypes IconType { get; set; } = WeUIIconTypes.weui_icon_warn;
        // 按钮 默认一个返回按钮
        public List<WeUIButton> Buttons { get; set; } = new List<WeUIButton>() {
                            new WeUIButton{ButtonType=WeUIButtonTypes.weui_btn_default, Text="返回",Link=string.Format("javascript:history.go(-1);")  }
                        };
        /// <summary>
        /// 详情链接 默认为空不显示
        /// </summary>
        public string DetailLink { get; set; } = string.Empty;
    }
    /// <summary>
    /// 按钮
    /// </summary>
    public class WeUIButton
    {
        /// <summary>
        /// 按钮类型 默认为白色
        /// </summary>
        public WeUIButtonTypes ButtonType { get; set; } = WeUIButtonTypes.weui_btn_default;
        /// <summary>
        /// 按钮文字
        /// </summary>
        public string Text { get; set; } = string.Empty;
        /// <summary>
        /// 按钮链接
        /// </summary>
        public string Link { get; set; } = string.Empty;
    }
    /// <summary>
    /// 错误提示类型
    /// </summary>
    public enum WeUIMsgType
    {
        /// <summary>
        /// 默认错误图标，传入的title,description,返回链接
        /// </summary>
        Default,
        /// <summary>
        /// 话题不存在
        /// </summary>
        TopicIsNull,
        /// <summary>
        /// 频道不存在
        /// </summary>
        ChannelIsNull,
        /// <summary>
        /// 话题审核不通过 需要提供zbtopicinfo 以显示返回直播间按钮
        /// </summary>
        TopicIsNotAudit,
        /// <summary>
        /// 直播间不存在
        /// </summary>
        LiveRoomIsNull,
        /// <summary>
        /// 未提供完整或可用的参数
        /// </summary>
        ParameterIsNotComplete,
        /// <summary>
        /// 直播间审核不通过
        /// </summary>
        LiveIsNotAudit
    }

    /// <summary>
    /// 图标类型
    /// </summary>
    public enum WeUIIconTypes
    {
        /// <summary>
        /// 成功
        /// </summary>
        weui_icon_success,
        /// <summary>
        /// 提示
        /// </summary>
        weui_icon_info,
        /// <summary>
        /// 警告
        /// </summary>
        weui_icon_warn,
        /// <summary>
        /// 等待
        /// </summary>
        weui_icon_waiting,
        /// <summary>
        /// 安全类成功
        /// </summary>
        weui_icon_safe_success,
        /// <summary>
        /// 安全类提示
        /// </summary>
        weui_icon_safe_warn
    }
    /// <summary>
    /// 按钮类型
    /// </summary>
    public enum WeUIButtonTypes
    {
        /// <summary>
        /// 绿色
        /// </summary>
        weui_btn_primary,
        /// <summary>
        /// 红色
        /// </summary>
        weui_btn_warn,
        /// <summary>
        /// 白色
        /// </summary>
        weui_btn_default,
        /// <summary>
        /// 镂空黑边
        /// </summary>
        weui_btn_plain_default,
        /// <summary>
        /// 镂空绿边
        /// </summary>
        weui_btn_plain_primary
    }

}