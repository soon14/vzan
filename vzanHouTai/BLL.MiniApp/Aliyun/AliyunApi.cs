namespace BLL.MiniApp.Aliyun
{
    /// <summary>
    /// 调用阿里云 oss 等相关API 封装
    /// </summary>
    public class AliyunApi
    {
        //public static ImageDetectionPostResult AliImageDetection(string imgUrl)
        //{
        //    ImageDetectionPostResult result = new ImageDetectionPostResult();
        //    var parameters = new Dictionary<string, string>()
        //    {
        //        { "Action", "ImageDetection" },
        //        { "RegionId","cn-hangzhou"},
        //        { "Async","false" },
        //        { "ImageUrl.1",imgUrl},
        //        { "Scene.1","porn"}
        //        //图片检测风险场景，目前支持的风险场景有porn、illegal、ocr, porn：黄图, illegal:暴恐敏感,ocr：文字识别. 单张图片一次支持多个场景的检测, 如(Scene.1= “porn”, Scene.2= “illegal”, Scene.3= “ocr”), 只用黄图检测，请填写Scene.1= “porn”
        //    };
        //    AliClient aliRequest = new AliClient(HttpMethod.Post, parameters);
        //    aliRequest.Version = "2016-12-16";
        //    string postApi = aliRequest.GetUrl(WebConfigBLL.AliApiImageDetection);
        //    string strRes =  HttpHelper.PostData(postApi);
        //    result = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageDetectionPostResult>(strRes);
        //    return result;
        //}
        //public static ImagePornResult GetImageDetectionRate(string imgUrl)
        //{
        //    ImagePornResult pornRes = new ImagePornResult();
        //    if (string.IsNullOrEmpty(imgUrl))
        //    {
        //        return pornRes;
        //    }
        //    ImageDetectionPostResult result = AliImageDetection(imgUrl);
        //    if (result == null) return pornRes;
        //    if(result.Code != "Success" || result.ImageResults==null || result.ImageResults.ImageResult==null || result.ImageResults.ImageResult.Count==0)
        //    {
        //        return pornRes;
        //    }
        //    return result.ImageResults.ImageResult[0].PornResult;
        //}

    }
}
