
namespace Core.MiniApp
{
    /// <summary>
    /// 发送图文消息
    /// </summary>
    public class WXMesage
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        public string PicUrl { get; set; }
        public string Url { get; set; }
        public string MsgType { get; set; }
        public string MediaId { get; set; }
    }
}
