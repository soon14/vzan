var _jsonsort = require('../../../utils/common');
const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
var app = getApp();
Page({
  data: {
      this_dish_info:null
  },
  onLoad: function (options) {
      var that = this;
      that.setData({
          this_options: options,
          this_dish_id: options.dish_id
      });
  },
  onShow: function () {
      wx.hideToast();
      this.loadSingleDishData();
  },
  loadSingleDishData: function () {
      var that = this;
      requestUtil.get(_DuoguanData.duoguan_host_api_url + '/index.php/addon/DuoguanDish/Api/getDishOneInfo.html', { dish_id: that.data.this_dish_id }, (info) => {
          console.log(info)
          that.setData({this_dish_info: info});
          wx.setNavigationBarTitle({ title: info.dish_webview_text});
          wx.hideToast();
      }, that, { isShowLoading: false });
  },
})