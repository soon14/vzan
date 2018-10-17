using Entity.MiniApp;

namespace BLL.MiniApp
{
    public static class MessageBLL
    {
        
        public static string GetPicUrl(Message model)
        {
            if (model.imagesUrl!=null&&model.imagesUrl.Length > 0)
            {
                return model.imagesUrl[0];
            }
            else if (model.imagesThumbUrl!=null&&model.imagesThumbUrl.Length > 0)
            {
                return model.imagesThumbUrl[0];
            }
            return "";
        }
    }
}
