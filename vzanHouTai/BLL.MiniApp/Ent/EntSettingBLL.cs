using DAL.Base;
using Entity.MiniApp.Ent;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Utility;

namespace BLL.MiniApp.Ent
{
    public class EntSettingBLL : BaseMySql<EntSetting>
    {
        #region 单例模式
        private static EntSettingBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntSettingBLL()
        {

        }

        public static EntSettingBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntSettingBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public const string key = "miniapp_EntSetting_{0}";
        public override EntSetting GetModel(int aid)
        {
            EntSetting model = RedisUtil.Get<EntSetting>(string.Format(key, aid));
            if (model == null)
            {
                model = base.GetModel($"aid={aid}");
                if (model != null)
                {
                    RedisUtil.Set<EntSetting>(string.Format(key, aid), model, TimeSpan.FromHours(12));
                }
            }
            return model;
        }
        public override bool Update(EntSetting model)
        {
            model.updatetime = DateTime.Now;
            bool result = base.Update(model);
            if (result)
            {
                RemoveCache(model);
            }
            return result;
        }

        public override bool Update(EntSetting model, string columnFields)
        {
            bool result = base.Update(model, columnFields);
            if (result)
            {
                RemoveCache(model);
            }
            return result;
        }

        public void RemoveCache(EntSetting model)
        {
            RedisUtil.Remove(string.Format(key, model.aid));
        }

        /// <summary>
        /// 同步删除小程序编辑里选择的分类
        /// </summary>
        /// <param name="jsonPath">$..goodCat[?(@.id==" + id + ")]</param>
        public void SyncData(int aid, string jsonPath)
        {
            try
            {
                EntSetting setModel = GetModel($"aid={aid}");
                if (setModel != null
                    && !string.IsNullOrEmpty(setModel.pages)
                    && setModel.pages.StartsWith("[")
                    && setModel.pages.EndsWith("]"))
                {
                    JArray arr = JArray.Parse(setModel.pages);
                    JToken item = null;
                    int loopCounter = 0;
                    do
                    {
                        item = arr.SelectTokens(jsonPath).FirstOrDefault();
                        if (item != null)
                        {
                            item.Remove();
                        }
                        loopCounter += 1;
                    }
                    while (item != null && loopCounter < 200);
                    loopCounter = 0;

                    setModel.pages = arr.ToString(Newtonsoft.Json.Formatting.None);
                    Update(setModel);
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
        }

        /// <summary>
        /// 动态裁剪组件中的图片
        /// </summary>
        /// <returns></returns>
        public string ResizeComsImage(string pages)
        {
            try
            {
                if (!string.IsNullOrEmpty(pages) && pages.StartsWith("[") && pages.EndsWith("]"))
                {
                    JArray pageArray = JArray.Parse(pages);
                    //图片导航，图片裁剪为 90x90
                    IEnumerable<JToken> imgnavComs = pageArray.SelectTokens("$..coms[?(@.type=='imgnav')].navlist");
                    imgnavComs.ToList().ForEach(c =>
                    {
                        c.ToList().ForEach(i =>
                        {
                            if (i["img"] != null)
                                i["img"] = ImgHelper.ResizeImg(i["img"].ToString(), 90, 90, "fill", 100, "", 90);
                        });
                    });
                    //轮播图，图片裁剪为 750x400
                    IEnumerable<JToken> sliderComs = pageArray.SelectTokens("$..coms[?(@.type=='slider')].items");
                    sliderComs.ToList().ForEach(c =>
                    {
                        c.ToList().ForEach(i =>
                        {
                            if (i["img"] != null)
                                i["img_fmt"] = ImgHelper.ResizeImg(i["img"].ToString(), 750, 400);
                        });
                    });
                    //底部导航，图片裁剪为 22x22
                    IEnumerable<JToken> bottomnavComs = pageArray.SelectTokens("$..coms[?(@.type=='bottomnav')].navlist");
                    bottomnavComs.ToList().ForEach(c =>
                    {
                        c.ToList().ForEach(i =>
                        {
                            if (i["img"] != null)
                                i["img"] = ImgHelper.ResizeImg(i["img"].ToString(), 44, 44, "fill", 100, "", 44);
                        });
                    });

                    /*
                     产品
                     大图：315x315
                     小图：150x150
                     详情列表：150x150
                     轮播:100x100
                     */
                    Dictionary<string, int> goodImgFormatDic = new Dictionary<string, int>() {
                { "big",315},{ "small",150},{ "normal",150},{ "scroll",100}
            };
                    IEnumerable<JToken> goodComs = pageArray.SelectTokens("$..coms[?(@.type=='good')]");
                    goodComs.ToList().ForEach(c =>
                    {
                        string goodShowType = c["goodShowType"].ToString();
                        if (!string.IsNullOrEmpty(goodShowType))
                        {
                            IEnumerable<JToken> goodItems = c.SelectTokens("$..items[*]");
                            goodItems.ToList().ForEach(i =>
                            {
                                int currentFormat = goodImgFormatDic[goodShowType];
                                if (i["img"] != null)
                                    i["img_fmt"] = ImgHelper.ResizeImg(i["img"].ToString(), currentFormat, currentFormat);
                            });
                        }

                    });

                    //视频组件封面
                    IEnumerable<JToken> videoComs = pageArray.SelectTokens("$..coms[?(@.type=='video')]");
                    videoComs.ToList().ForEach(c =>
                    {
                        if (c["poster"] != null)
                            c["poster"] = ImgHelper.ResizeImg(c["poster"].ToString(), 750, 450);

                    });

                    //图片组件
                    IEnumerable<JToken> imageComs = pageArray.SelectTokens("$..coms[?(@.type=='img')]");
                    imageComs.ToList().ForEach(c =>
                    {
                        if (c["imgurl"] != null)
                            c["imgurl_fmt"] = ImgHelper.ResizeImg(c["imgurl"].ToString(), 750, 0, "lfit");

                    });

                    //直播组件封面
                    IEnumerable<JToken> liveComs = pageArray.SelectTokens("$..coms[?(@.type=='live')]");
                    liveComs.ToList().ForEach(c =>
                    {
                        if (c["img"] != null)
                            c["img"] = ImgHelper.ResizeImg(c["img"].ToString(), 250, 250);
                        //直播组件产品图片
                        IEnumerable<JToken> goodItems = c.SelectTokens("$..items[*]");
                        goodItems.ToList().ForEach(i =>
                        {
                            if (i["img"] != null)
                                i["img"] = ImgHelper.ResizeImg(i["img"].ToString(), 80, 80);
                        });

                    });

                    pages = pageArray.ToString();
                }
                return pages;
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return "[]";
            }
        }
    }
}
