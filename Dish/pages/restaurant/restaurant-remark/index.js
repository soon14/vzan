const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
var app = getApp();
Page({
  data: {
    dish_data: [],
    this_dish_id: 0,
    this_beizhu_info: ''
  },
  onLoad: function(op) {
    this.setData({
      this_dish_id: op.dish_id,
      this_beizhu_info: wx.getStorageSync('dish_order_beizhu' + op.dish_id)
    });
  },
  onShow: function() {
    var that = this;
    var rq_data = {};
    rq_data.dish_id = that.data.this_dish_id;
    requestUtil.post(_DuoguanData.getDishInfo, rq_data, (info) => {
      that.setData({
        dish_data: info
      });
    }, this, {});
  },
  //选择备注
  select_beizhu_bind: function(e) {
    var beizhu_info = this.data.this_beizhu_info;
    if (beizhu_info) {
      beizhu_info += e.currentTarget.dataset.info + ' ';
    } else {
      beizhu_info = e.currentTarget.dataset.info + ' ';
    }
    this.setData({
      this_beizhu_info: beizhu_info
    });
  },
  //设置缓存
  beizhu_formSubmit: function(e) {
    wx.setStorageSync('dish_order_beizhu' + this.data.this_dish_id, e.detail.value.beizhu);
    wx.navigateBack({
      delta: 1
    });
  },
  textarea_input: function(e) {
    this.data.this_beizhu_info = e.detail.value
  },
})