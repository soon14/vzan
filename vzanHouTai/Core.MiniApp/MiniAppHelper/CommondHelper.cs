using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.AliOss;

namespace Core.MiniApp
{
    public static class CommondHelper
    {
        public static readonly string _resetPasswordkey = "dz_phone_{0}_bindvalidatecode_resetpwd";

        /// <summary>
        /// 获取小程序二维码
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        public static qrcodeclass GetMiniAppQrcode(string access_token, string page, string scene = "", int width = 200)
        {
            qrcodeclass model = new qrcodeclass();
            try
            {
                string qrCode = "";
                string errorMessage = "";
                if (string.IsNullOrEmpty(scene))
                {
                    //表示新增
                    string postData = JsonConvert.SerializeObject(new
                    {
                        path = page,
                        //page = page,
                        width = width,
                        auto_color = true,
                        line_color = new { r = "0", g = "0", b = "0" }
                    });
                    qrCode = HttpPostSaveImg("https://api.weixin.qq.com/wxa/getwxacode?access_token=" + access_token, postData,ref errorMessage);
                }
                else
                {
                    //表示新增
                    string postData = JsonConvert.SerializeObject(new
                    {
                        scene = scene,
                        page = page,
                        //path = page,
                        width = width,
                        auto_color = true,
                        line_color = new { r = "0", g = "0", b = "0" }
                    });
                    qrCode = HttpPostSaveImg("https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token=" + access_token, postData,ref errorMessage);
                }

                if (string.IsNullOrEmpty(qrCode))
                {
                    model.isok = -1;
                    model.msg = $"获取失败!{errorMessage}";
                    return model;
                }
                model.url = qrCode;
                model.isok = 1;
                model.msg = "成功";
            }
            catch (Exception ex)
            {
                model.msg = "异常" + ex.Message;
            }

            return model;
        }

        /// <summary>
        /// 获取二维码使用b接口
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="path"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static qrcodeclass GetWxQrcode(string access_token, string path, string scene, int width = 430)
        {
            qrcodeclass model = new qrcodeclass();
            if (string.IsNullOrEmpty(path) || width <= 0)
            {
                model.isok = -1;
                model.msg = "参数错误";
                return model;
            }
            string postData = JsonConvert.SerializeObject(new
            {
                scene,
                page = path,
                width = 200,
                is_hyaline = false,
            });
            string errorMessage = "";
            string qrCode = HttpPostSaveImg("https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token=" + access_token, postData,ref errorMessage);
            if (string.IsNullOrEmpty(qrCode))
            {
                model.isok = -1;
                model.msg = $"获取失败!{errorMessage}";
                return model;
            }
            model.url = qrCode;
            model.isok = 1;
            model.msg = "成功";
            return model;
        }

        /// <summary>
        /// 获取小程序码不带参数
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="dmsg"></param>
        /// <param name="pagePath"></param>
        /// <returns></returns>
        public static string GetQrcode(string token, string pagePath, string scene = "")
        {
            string qrcodeImg = string.Empty;
            //获取二维码
            qrcodeclass resultQcode = GetMiniAppQrcode(token, pagePath, scene);
            if (resultQcode != null)
            {
                if (resultQcode.isok > 0)
                {
                    qrcodeImg = resultQcode.url;
                }
            }
            return qrcodeImg;
        }

        /// <summary>
        /// 获取小程序二维码
        /// </summary>
        /// <param name="url">获取token的路径</param>
        /// <param name="AppId"></param>
        /// <returns></returns>
        public static qrcodeclass GetMiniAppQrcode_wxaapp(string access_token, string path, int width = 200)
        {
            qrcodeclass model = new qrcodeclass();
            try
            {
                //小程序二维码
                string qrCode = "";
                string postData = JsonConvert.SerializeObject(new
                {
                    path = path,
                    width = width,
                });
                string errorMessage = "";
                qrCode = HttpPostSaveImg("https://api.weixin.qq.com/cgi-bin/wxaapp/createwxaqrcode?access_token=" + access_token, postData,ref errorMessage);
                if (string.IsNullOrEmpty(qrCode))
                {
                    model.isok = -1;
                    model.msg = $"获取失败!{errorMessage}";
                    return model;
                }

                model.url = qrCode;
                model.isok = 1;
                model.msg = "成功";
            }
            catch (Exception)
            {
            }

            return model;
        }

        /// <summary>
        /// 根据url 请求获取图片
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="postDataStr"></param>
        /// <returns></returns>
        public static string HttpPostSaveImg(string Url, string postDataStr, ref string errorMessage)
        {
            string aliTempImgKey = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                CookieContainer cookie = new CookieContainer();
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
                request.CookieContainer = cookie;
                using (Stream myRequestStream = request.GetRequestStream())
                {
                    using (StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312")))
                    {
                        myStreamWriter.Write(postDataStr);
                    }
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        response.Cookies = cookie.GetCookies(response.ResponseUri);
                        Stream myResponseStream = response.GetResponseStream();

                        //  myResponseStream.Position = 0;//保证流可读
                        byte[] byteData = Utility.IO.StreamHelpers.ReadFully(myResponseStream);
                        myRequestStream.Close();
                        #region 判断是否返回错误
                        string result = Encoding.Default.GetString(byteData);

                        if (result.Contains("errcode"))
                        {
                            errorMessage = result;
                            return string.Empty;
                        }
                        #endregion

                        string ext = string.Empty;
                        ImgHelper.IsImgageType(byteData, "jpg", out ext);
                        HashSet<string> fileType = new HashSet<string>() { "png", "jpg", "jpeg", "gif", "bmp" };
                        if (fileType.Contains(ext))
                        {
                            SaveImageToAliOSS(byteData, out aliTempImgKey);
                        }
                        else
                        {
                            aliTempImgKey = "";
                        }
                    }
                }
            }
            catch (Exception)
            {
                
            }

            return aliTempImgKey;
        }

        public static bool SaveImageToAliOSS(byte[] byteArray, out string aliTempImgKey)
        {
            string aliTempImgFolder = AliOSSHelper.GetOssImgKey("jpg", false, out aliTempImgKey);
            return AliOSSHelper.PutObjectFromByteArray(aliTempImgFolder, byteArray, 1, ".jpg");
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="msg"></param>
        /// <param name="type">修改密码 type=0  注册type=1</param>
        /// <param name="timeOutMinute">过期时间 默认5分钟</param>
        /// <returns></returns>
        public static Return_Msg GetVaildCode(int agentqrcodeid, string phone, Account account, int type = 0, int timeOutMinute = 5)
        {
            Return_Msg msg = new Return_Msg();
            string phonekey = phone;
            if (string.IsNullOrEmpty(phonekey))
            {
                msg.Msg = "手机号不能为空";
                msg.code = "-1";
                return msg;
            }
            if (string.IsNullOrEmpty(_resetPasswordkey))
            {
                msg.Msg = "缓存key错误";
                msg.code = "-1";
                return msg;
            }
            //Account account = bllAccount.GetModelByPhone(phone);
            //大于0不用判断手机号是否注册，因为非代理商用户可以成为代理商
            if (agentqrcodeid <= 0)
            {
                if (type == 0)
                {
                    if (account == null)
                    {
                        msg.Msg = "该手机号码还没绑定账号！";
                        msg.code = "-1";
                        return msg;
                    }
                }
                else if (type == 1)
                {
                    if (account != null)
                    {
                        msg.Msg = "亲，您的手机号码已绑定账号，赶快去登陆！";
                        msg.code = "-1";
                        return msg;
                    }
                }
            }

            if (Regex.IsMatch(phonekey, @"^1\d{10}$"))//StringHelper.IsMobile(phonekey)
            {
                string validatcode = Utility.EncodeHelper.getRandomString(6);
                //写入缓存。5分钟过期
                string key = string.Format(_resetPasswordkey, phone);
                if (!string.IsNullOrEmpty(RedisUtil.Get<string>(key)))
                {
                    msg.Msg = "亲，你已经发送过验证码了！";
                    msg.code = "0";
                    return msg;
                }
                else
                {
                    if (new SendMsgHelper().AliSend(phone, "{\"code\":\"" + validatcode + "\"}", "小未科技", 1))
                    {
                        RedisUtil.Set<string>(key, validatcode + "," + phone, TimeSpan.FromMinutes(timeOutMinute));//测试时5分钟改为30分钟
                        msg.Msg = "验证码已发送";
                        msg.code = "1";
                        msg.isok = true;
                        return msg;
                    }
                    else
                    {
                        msg.Msg = $"验证码发送太频繁，请稍后再试！";
                        msg.code = "2";
                        return msg;
                    }
                }
            }
            else
            {
                msg.Msg = "请输入正确的手机号码！";
            }

            msg.code = "-1";
            return msg;
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static Return_Msg CheckVaildCode(string phone, string code)
        {
            Return_Msg msg = new Return_Msg();
            msg.code = "1";
            msg.Msg = "成功";
            string phonekey = phone;
            if (string.IsNullOrEmpty(phonekey))
            {
                msg.Msg = "手机号不能为空";
                msg.code = "-1";
                return msg;
            }

            if (string.IsNullOrEmpty(code))
            {
                msg.Msg = "验证码不能为空";
                msg.code = "-1";
                return msg;
            }
            string key = string.Format(_resetPasswordkey, phone);
            string checkcode = RedisUtil.Get<string>(key);
            if (string.IsNullOrEmpty(checkcode))
            {
                msg.Msg = "验证码失效，请重新发送";
                msg.code = "-1";
                return msg;
            }
            if (checkcode != code + "," + phone)
            {
                msg.Msg = "验证码错误";
                msg.code = "-1";
                return msg;
            }

            msg.isok = true;
            return msg;
        }

        /// <summary>
        /// 通过原始Id 获取authorizer_access_token 不管是小程序还是服务号都有原始Id 唯一的微信那边
        /// </summary>
        /// <param name="user_name"></param>
        /// <returns></returns>
        public static string GetAuthorizer_Access_Token(string url)
        {
            string authorizer_access_token = string.Empty;
            string result = HttpHelper.GetData(url);
            if (result != null)
            {
                GetAccessTokenMsg accessTokenMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<GetAccessTokenMsg>(result);
                if (accessTokenMsg != null && accessTokenMsg.obj != null)
                {
                    if (accessTokenMsg.obj != null)
                    {
                        authorizer_access_token = accessTokenMsg.obj.access_token;
                    }
                }
            }

            return authorizer_access_token;
        }

        /// <summary>
        /// 创建微信会员卡卡套
        /// </summary>
        /// <param name="logo_url">会员卡Logo</param>
        /// <param name="brand_name">商户名字,字数上限为12个汉字。 </param>
        /// <param name="title">卡券名，字数上限为9个汉字</param>
        /// <param name="appOriginalId">小程序原始Id</param>
        /// <param name="access_token">小程序access_token</param>
        public static CreateCardResult AddVipWxCard(string logo_url, string brand_name, string title, string appOriginalId, string access_token, int PageType = 22)
        {
            //默认专业版的
            string center_app_brand_pass = "pages/my/myInfo";//专业版 个人中心
            string custom_app_brand_pass = "pages/index/index";//首页 专业版

            switch (PageType)
            {
                case (int)TmpType.小程序电商模板:
                    center_app_brand_pass = "pages/me/me";//个人中心页面
                    break;

                case (int)TmpType.小程序餐饮模板:
                    center_app_brand_pass = "pages/me/me";//个人中心页面
                    custom_app_brand_pass = "pages/home/home";//首页
                    break;

                case (int)TmpType.小程序足浴模板:
                    center_app_brand_pass = "pages/me/me";//个人中心页面
                    custom_app_brand_pass = "pages/book/book";//首页
                    break;

                case (int)TmpType.小程序多门店模板:
                    center_app_brand_pass = "pages/me/me";//个人中心页面
                    break;
                case (int)TmpType.小未平台子模版:
                    center_app_brand_pass = "pages/my/my-index/index";//个人中心页面
                    custom_app_brand_pass = "pages/home/shop-detail/index";//首页
                    break;
            }

            base_info _base_info = new base_info();
            _base_info.logo_url = logo_url;
            _base_info.code_type = "CODE_TYPE_TEXT";
            _base_info.brand_name = brand_name;
            _base_info.title = title;
            _base_info.color = "Color010";

            _base_info.center_title = "储值余额";
            _base_info.center_app_brand_user_name = $"{appOriginalId}@app";
            _base_info.center_app_brand_pass = center_app_brand_pass;

            _base_info.custom_url_name = "小程序";
            _base_info.custom_app_brand_user_name = $"{appOriginalId}@app";
            _base_info.custom_app_brand_pass = custom_app_brand_pass;
            _base_info.custom_url_sub_title = "点击进入";

            //_base_info.promotion_url_name = "门店地址";
            //_base_info.promotion_app_brand_user_name = $"{appOriginalId}@app";
            //_base_info.promotion_app_brand_pass = "pages/index/index";

            _base_info.description = $"{brand_name}会员卡";
            _base_info.notice = "使用时向服务员出示此券";

            Card _card = new Card();
            WxCard wxCard = new WxCard();
            member_card _member_Card = new member_card();
            _member_Card.auto_activate = true;
            _member_Card.base_info = _base_info;
            _member_Card.custom_field1 = new custom_fieldItem
            {
                name = "储值余额",
            };
            _member_Card.custom_field2 = new custom_fieldItem
            {
                name = "累计消费",
            };
            _member_Card.custom_field3 = new custom_fieldItem
            {
                name = "会员权益",
            };
            _member_Card.prerogative = "更快更便捷的了解积分余额等信息";

            _card.member_card = _member_Card;
            wxCard.card = _card;

            string json = JsonConvert.SerializeObject(wxCard);

            string result = Utility.IO.Context.PostData($"https://api.weixin.qq.com/card/create?access_token={access_token}", json);
            CreateCardResult _createCardResult = new CreateCardResult();
            //   _createCardResult.errmsg = json;
            if (string.IsNullOrEmpty(result))
                return _createCardResult;
            _createCardResult = JsonConvert.DeserializeObject<CreateCardResult>(result);
            return _createCardResult;
        }

        /// <summary>
        /// 上传图片到微信
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public static string WxUploadImg(string access_token, string path)
        {
            int pos = path.LastIndexOf("/");
            string fileName = path.Substring(pos + 1);
            return UploadImgByStream($"http://api.weixin.qq.com/cgi-bin/media/uploadimg?access_token={access_token}", WxUploadHelper.GetImgStream(path).ToArray(), fileName);
        }

        /// <summary>
        /// 上传图片到微信临时素材
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public static string WxUploadTemImg(string access_token, string path)
        {
            int pos = path.LastIndexOf("/");
            string fileName = path.Substring(pos + 1);
            return UploadImgByStream($"https://api.weixin.qq.com/cgi-bin/media/upload?access_token={access_token}&type=image", WxUploadHelper.GetImgStream(path).ToArray(), fileName);
        }

        public static string UploadImgByStream(string url, byte[] bArr, string fileName)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";

            string boundary = DateTime.Now.Ticks.ToString("X"); // 随机分隔线
            request.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;
            byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

            //请求头部信息
            StringBuilder sbHeader = new StringBuilder(string.Format("Content-Disposition:form-data;name=\"file\";filename=\"{0}\"\r\nContent-Type:application/octet-stream\r\n\r\n", fileName));
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(sbHeader.ToString());
            string str = string.Empty;
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
                postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                postStream.Write(bArr, 0, bArr.Length);
                postStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                //发送请求并获取相应回应数据
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    using (Stream respStream = response.GetResponseStream())
                    {
                        using (StreamReader sw = new StreamReader(respStream, Encoding.UTF8))
                        {
                            str = sw.ReadToEnd();
                        }
                    }
                }
            }

            return str;
        }

        /// <summary>
        /// 时间转时间戳10位
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static long TimeToUnix(DateTime t)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            long timeStamp = (long)(t - startTime).TotalMilliseconds; // 相差毫秒数
            return timeStamp;
        }

        /// <summary>
        /// 10位数时间戳转时间字符串
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static string UnixToTime(long unixTimeStamp)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            DateTime dt = startTime.AddSeconds(unixTimeStamp);
            return dt.ToString("yyyy/MM/dd HH:mm:ss:ffff");
        }

        private static double rad(double d)
        {
            return d * Math.PI / 180.0;
        }

        /// <summary>
        /// 腾讯地图根据两个地点的经纬度,返回两个地点之间的距离(公里)
        /// 有误差,同腾讯地图api 8公里 误差在0.1米以内
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <returns></returns>
        public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double EARTH_RADIUS = 6378.137;
            double radLat1 = rad(lat1);
            double radLat2 = rad(lat2);
            double a = radLat1 - radLat2;
            double b = rad(lng1) - rad(lng2);
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }

        /// <summary>
        /// 根据时间差获取显示标识
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static string GetTimeSpan(TimeSpan ts)
        {
            if (Math.Floor(ts.TotalDays) >= 365)
            {
                return (int)(Math.Floor(ts.TotalDays) / 365) + "年前";
            }
            else if (Math.Floor(ts.TotalDays) >= 30)
            {
                return (int)(Math.Floor(ts.TotalDays) / 30) + "月前";
            }
            else if (Math.Floor(ts.TotalDays) >= 1)
            {
                return (int)(Math.Floor(ts.TotalDays)) + "天前";
            }
            else if (Math.Floor(ts.TotalHours) >= 1)
            {
                return (int)(Math.Floor(ts.TotalHours)) + "小时前";
            }
            else if (Math.Floor(ts.TotalMinutes) >= 1)
            {
                return (int)(Math.Floor(ts.TotalMinutes)) + "分钟前";
            }
            else
            {
                return (int)(Math.Floor(ts.TotalSeconds)) <= 0 ? "刚刚" : ((int)(Math.Floor(ts.TotalSeconds)) + "秒前");
            }
        }

        /// <summary>
        /// 通过IP定位 腾讯地图
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        public static IPToPoint GetLoctionByIP(string IP)
        {
            string url = $"http://apis.map.qq.com/ws/location/v1/ip?ip={IP}&key={WebSiteConfig.Tx_MapKey}";
            string result = HttpHelper.GetData(url);
            if (string.IsNullOrEmpty(result))
                return null;
            if (result.Contains("query ok"))
            {
                IPToPoint iPToPoint = JsonConvert.DeserializeObject<IPToPoint>(result);
                return iPToPoint;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 计算两个时间相差的TimeSpan
        /// </summary>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <returns></returns>
        public static TimeSpan DateDiff(DateTime dateStart, DateTime dateEnd)
        {

            TimeSpan sp = dateEnd.Subtract(dateStart);

            return sp;

        }

    }

    /// <summary>
    /// 上传图片到微信那边返回的结果
    /// </summary>
    public class WxUploadImgResult
    {
        /// <summary>
        /// 上传后返回的图片路径
        /// </summary>
        public string url { get; set; }
    }

    /// <summary>
    /// 上传临时素材到微信那边返回的结果
    /// </summary>
    public class WxUploadTemImgResult
    {
        public string type { get; set; }
        public string media_id { get; set; }
        public string created_at { get; set; }
    }

    [Serializable]
    public class qrcodeclass
    {
        public int isok { get; set; }
        public string msg { get; set; }
        public string url { get; set; }
    }

    /// <summary>
    /// 腾讯地图IP定位返回结果
    /// </summary>
    public class IPToPoint
    {
        public int status { get; set; }
        public string message { get; set; }
        public ResultItem result { get; set; }
    }

    public class ResultItem
    {
        public locationItem location { get; set; }
        public ad_infoItem ad_Info { get; set; }
    }

    public class locationItem
    {
        public double lng { get; set; }
        public double lat { get; set; }
    }

    public class ad_infoItem
    {
        public string nation { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public int adcode { get; set; }
    }

    /// <summary>
    /// 修正DateTime的JSON序列化格式
    /// （/Date(-62135596800000)/ 改为 '2018-04-20 15:49'）
    /// </summary>
    public class JsonResultFormat : JsonResult
    {

        public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

        public JsonResultFormat()
        {
        }

        public override void ExecuteResult(ControllerContext context)
        {
            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = "application/json";
            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;
            if (Data != null)
            {
                JsonTextWriter writer = new JsonTextWriter(response.Output) { Formatting = Formatting.Indented };
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    //NullValueHandling = NullValueHandling.Ignore,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                };
                settings.Converters.Add(new IsoDateTimeConverter { DateTimeFormat = DateFormat });
                JsonSerializer serializer = JsonSerializer.Create(settings);
                serializer.Serialize(writer, Data);
                writer.Flush();
            }
        }
    }
}