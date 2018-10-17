// pages/pageset/pageset.js
var WxParse = require("../../modules/wxParse/wxParse.js");
import { http, addr, tools, pageData, coms, formItem, formMutiSelectItem } from "../../modules/core.js";

var app = getApp();


Page({

  /**
   * 页面的初始数据
   */
  data: {
    formItemType: ['文本', '数字', '日期', '单选'],
    vm: Object.assign({}, pageData),
    isnav: false,
    saveShow:true,
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var id = options.id || 0;
    var title = id > 0 ? "编辑页面" : "制作页面";
    wx.setNavigationBarTitle({
      title: title
    })
    this.setData({ id });
    this.init();
  },
  init: function () {
    var vm = Object.assign({}, pageData);
    var id = this.data.id;
    vm.id = id;
    this.setData({ vm });

    var that = this;
    if (id > 0) {
      app.getUserInfo().then(function (user) {
        http.postAsync(addr.GetUserPageInfo, { id: id, uid: user.uid }).then(function (data) {
          if (data.obj.content != "" && typeof data.obj.content == "string") {
            data.obj.content = JSON.parse(data.obj.content);
          }
          that.setData({ vm: data.obj });
        });
      });
    }
    else {
      that.setData({ vm });
    }
  },
  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function () {

  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {
    var that = this;
    var d = that.data;
    d.vm.content.coms.forEach(p => {
      if (p.type == "txt") {
        var key =
          p.content_fmt = WxParse.wxParse('content', 'html', p.content)
      }
    });
  },

  /**
   * 生命周期函数--监听页面隐藏
   */
  onHide: function () {

  },

  /**
   * 生命周期函数--监听页面卸载
   */
  onUnload: function () {
    
  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    var that = this;
    tools.alert("刷新后编辑数据将丢失，确定要刷新吗？", "提示", function () {
      setTimeout(function () {

        that.init();
        wx.stopPullDownRefresh()
      }, 500);
    }, function () {
      wx.stopPullDownRefresh();
    });
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {

  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  },


  formitemAdd: function (e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var comIndex = ds.comIndex;
    var coms = this.data.vm.content.coms;
    wx.showActionSheet({
      itemList: that.data.formItemType,
      success: function (res) {
        console.log(res.tapIndex);
        var comItemName = "";
        switch (res.tapIndex) {
          case 0://文本
            comItemName = "text";
            break;
          case 1://数字
            comItemName = "number"
            break;
          case 2://日期
            comItemName = "date";
            break;
          case 3://单选
            comItemName = "radio";
            break;
        }
        coms[comIndex].items.push(Object.assign({}, formItem[comItemName]));
        var key = "vm.content.coms[" + comIndex + "]";
        that.setData({
          [key]: coms[comIndex]
        });
      }
    })
  },
  addMutiItem: function (e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var comIndex = ds.comIndex;
    var itemIndex = ds.itemIndex
    var coms = this.data.vm.content.coms;
    var newItem = JSON.parse(JSON.stringify(formMutiSelectItem));
    coms[comIndex].items[itemIndex].items.push(newItem);
    var key = "vm.content.coms[" + comIndex + "].items[" + itemIndex + "].items";
    this.setData({
      [key]: coms[comIndex].items[itemIndex].items,
    });
  },
  buildCom: function (comName) {
    return Object.assign({}, coms[comName]);
  },
  createCom: function (e) {
    var comName = e.currentTarget.dataset.comName;
    var vm = this.data.vm;
    vm.content.coms.push(this.buildCom(comName));
    this.setData({ vm });
  },
  removeCom: function (e) {
    var that = this;
    var comIndex = e.currentTarget.dataset.comIndex;
    var vm = that.data.vm;
    vm.content.coms.splice(comIndex, 1);
    this.setData({ vm: vm });
  },
  removeItem: function (e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var comIndex = ds.comIndex;
    var itemIndex = ds.itemIndex
    var vm = that.data.vm;
    vm.content.coms[comIndex].items.splice(itemIndex, 1);
    var key = "vm.content.coms[" + comIndex + "].items";
    this.setData({ [key]: vm.content.coms[comIndex].items });
  },
  removeSubItem: function (e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var comIndex = ds.comIndex;
    var itemIndex = ds.itemIndex;
    var subitemIndex = ds.subitemIndex;
    var vm = that.data.vm;
    vm.content.coms[comIndex].items[itemIndex].items.splice(subitemIndex, 1);
    var key = "vm.content.coms[" + comIndex + "].items[" + itemIndex + "].items";
    this.setData({ [key]: vm.content.coms[comIndex].items[itemIndex].items });
  },
  removeImg: function (e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var vm = that.data.vm;
    var comIndex = ds.comIndex;
    var itemIndex = ds.itemIndex;
    var field = ds.field || "src";
    var key = "";
    if (itemIndex != -1) {
      key = "vm.content.coms[" + comIndex + "].items[" + itemIndex + "]." + field;
    }
    else {
      key = "vm.content.coms[" + comIndex + "]." + field;
    }
    that.setData({
      [key]: ""
    });
  },
  //组件下移
  moveComDown: function (e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var vm = that.data.vm;
    var comIndex = ds.comIndex;
    var clickCom = vm.content.coms.splice(comIndex, 1)[0];
    vm.content.coms.splice(comIndex + 1, 0, clickCom);
    that.setData({ vm });
  },
  //组件上移
  moveComUp: function (e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var vm = that.data.vm;
    var comIndex = ds.comIndex;
    var clickCom = vm.content.coms.splice(comIndex, 1)[0];
    vm.content.coms.splice(comIndex - 1, 0, clickCom);
    that.setData({ vm });
  },
  editCom: function (e) {
    var that = this;
    var d = that.data;
    var ds = e.currentTarget.dataset;
    var vm = that.data.vm;
    var comIndex = ds.comIndex;
    var itemIndex = ds.itemIndex;
    var pages = getCurrentPages();
    var pageUrl = pages[pages.length - 1].route;
    if (d.isnav)
      return;
    if (!d.isnav)
      d.isnav = true;
    wx.navigateTo({
      url: 'txtComEdit?comIndex=' + comIndex + "&itemIndex=" + itemIndex + "&backUrl=" + pageUrl,
    })
  },
  //选择图片
  pickFile: function (e) {
    var ds = e.currentTarget.dataset;
    var comIndex = ds.comIndex;
    var itemIndex = ds.itemIndex;
    var fileType = ds.fileType || "img";
    var field = ds.field || "src";
    var that = this;
    var com = that.data.vm.content.coms[comIndex];
    tools.upload(fileType).then(function (url) {
      var key = "";
      if (itemIndex != -1) {
        key = "vm.content.coms[" + comIndex + "].items[" + itemIndex + "]." + field;
      }
      else {
        key = "vm.content.coms[" + comIndex + "]." + field;
      }
      that.setData({
        [key]: url
      });
    });
  },

  inputPageTitle: function (e) {
    this.setData({
      "vm.content.pageTitle": e.detail.value
    });
  },
  syncValue: function (e) {
    var that = this;
    var val = e.detail.value;
    var ds = e.currentTarget.dataset;
    var comIndex = ds.comIndex;
    var itemIndex = ds.itemIndex;
    var subitemIndex = ds.subitemIndex;
    var field = ds.field || "value";
    var key = "";
    console.log(subitemIndex);
    if (itemIndex != -1 && itemIndex != undefined) {
      if (subitemIndex != undefined) {
        key = "vm.content.coms[" + comIndex + "].items[" + itemIndex + "].items[" + subitemIndex + "]." + field;
      }
      else {
        key = "vm.content.coms[" + comIndex + "].items[" + itemIndex + "]." + field;
      }
    }
    else {
      key = "vm.content.coms[" + comIndex + "]." + field;
    }
    that.setData({
      [key]: val
    });
  },
  save: function () {
    var that = this;
    tools.savePage(that, function (id) {
      wx.navigateTo({
        url: 'pagePreview?id=' + id,
      })
    });

  },
  changeSaveShow:function(){
    var that=this;
    this.setData({
      saveShow: !that.data.saveShow
    });
  }
})