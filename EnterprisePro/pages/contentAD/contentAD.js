// pages/contentAD/contentAD.js
const app = getApp();
const page = require('../../utils/pageRequest.js')
const tools = require("../../utils/tools.js");
const util = require("../../utils/util.js");
Page({

  /**
   * 页面的初始数据
   */
  data: {
    listViewModel: {
      pageindex: 1,
      pagesize: 5,
      list: [],
      ispost: false,
      loadall: false,
    },
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    let that = this
    let mr = ""
    let dl = ""
    if (options.mr) {
      page.contentMore(options.ids, options.typeid, that);
      that.data.typeid = options.typeid
      that.data.ids = options.ids
      that.data.aid = wx.getStorageSync("aid")
      util.navBarTitle("更多咨询")
      mr = options.mr
    }
    else {
      that.data.id = options.id
      page.contentDetail(options.id, that)
      dl = options.dl
    }
    that.setData({
      dl: dl,
      mr: mr
    })
    util.setPageSkin(that);
  },


  // 上啦加载
  onReachBottom: function () {
    page.contentMore(this.data.ids, this.data.typeid, this);
    this.setData({
      isShowBtn: true
    });
  },
  onPageScroll: function () {
    var vm = this
    var query = wx.createSelectorQuery()
    var VP = query.selectViewport()
    VP.scrollOffset(function(res) {
      var scrollTop = res.scrollTop
      var flag = vm.data.isShowBtn
      if (scrollTop < 10) {
        flag = false
        vm.setData({
          isShowBtn: flag 
        })
      }
    }).exec()
  },
  // 详情跳转
  detailsGoto: function (e) {
    let id = e.currentTarget.dataset.id
    tools.goNewPage('../contentAD/contentAD?id=' + id + "&dl=true")
  },
  // 点击查看大图
  preview: function (e) {
    let that = this
    let slider = e.currentTarget.dataset.slider
    let img = e.currentTarget.dataset.img
    let index = e.currentTarget.id
    if (slider) {
      util.preViewShow(slider[index], slider)
    }
    else {
      let urls = []
      urls.push(img)
      util.preViewShow(img, urls)
    }
  },
  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    let that = this
    tools.showLoadToast('正在刷新')
    setTimeout(res => {
      if (that.data.mr) {
        tools.reset(that.data.listViewModel)
        page.contentMore(that.data.ids, that.data.typeid, that);
      } else {
        //page.newsInfoRequest(that.data.id, that)
        page.contentDetail(that.data.id, that)
      }
      tools.showToast("刷新成功")
    }, 1000)
    wx.stopPullDownRefresh()
  },
  // 返回顶部
  goBack: function () {
    tools.pageTop(this)
  },
  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {
    let that = this
    let title = ''
    let path = ''
    let imageUrl = ''
    if (that.data.mr) {
      title = ''
      path = '/pages/contentAD/contentAD?aid=' + that.data.aid + "&typeid=" + that.data.typeid + "&ids=" + that.data.ids
      imageUrl = ''
    } else {
      title = that.data.msg.title
      path = '/pages/contentAD/contentAD?id=' + that.data.id
      that.data.msg.slideimgs ? imageUrl = that.data.slideimgs[0] : imageUrl = that.data.msg.img
    }
    return {
      title: title,
      path: path,
      imageUrl: imageUrl,
      success: res => { tools.showToast("转发成功") }
    }
  },

})








