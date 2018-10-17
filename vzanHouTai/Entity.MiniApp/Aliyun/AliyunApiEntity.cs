using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Aliyun
{
    public class AliyunApiEntity
    {

    }

    public class ImageDetectionPostResult
    {
        public string Msg { get; set; }
        public string Code { get; set; }
        public ImageResults ImageResults { get; set; }
    }

    public class ImageResults
    {
        public List<ImageResult> ImageResult { get; set; }
    }

    public class ImageResult
    {
        public string TaskId { get; set; }
        public string ImageName { get; set; }
        public ImagePornResult PornResult { get; set; }
    }
    public class ImagePornResult
    {
        /// <summary>
        /// 色情图像识别结果，json对象。包含两个key-value对：{“Label”:1, “Rate”:100}。Rate~100范围的一个浮点值，越接近100，表示色情图像的概率越高（精度到小数点后2位）。Rate，0表示正常，1表示色情，2表示未确认。用户可以根据自己的场景采信这个值（注：绿网会根据图片样本的不断积累，动态调整建议值的阈值设定）, (异步图片检测时该字段内容为空, 需要根据taskid调用取图片检测结果接口进行轮询结果)
        /// </summary>
        public int Label { get; set; }

        public float Rate { get; set; }
    }
    public enum LabelEnum
    {
        正常=0,
        色情=1,
        未确认=2
    }
}
