
const app = getApp();
const util = require("../../utils/util.js");
const page = require('../../utils/pageRequest.js')
const tools = require('../../utils/tools.js')
const pickerFile = require('../../template/picker/js/picker_datetime.js')

Page({

  /**
   * 页面的初始数据
   */
  data: {
    region: "请选择地址",
    array: [],
    array1: ['男', '女'],
    index: 0,
    listFrom: {
      ispost: false,
      loadall: false,
    },
    listViewModel: {
      pageindex: 1,
      pagesize: 5,
      list: [],
      ispost: false,
      loadall: false,
    },
    showModalUser: false
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    let that = this
    that.data.options = options
    util.setPageSkin(that);
  },
  onloadFunc: function (options) {
    let that = this
    let navTitle = ""
    // 预约列表
    if (options.sublist) {
      navTitle = "我的预约单"
      let listViewModel = wx.getStorageSync("listViewModel")
      listViewModel ? that.setData({ listViewModel: listViewModel }) : page.subMore(that)
      that.data.aid = wx.getStorageSync("aid")
      that.setData({ sublist: options.sublist })
    }
    // 预约表单
    else {
      navTitle = "预约"
      let sub = wx.getStorageSync("sub")
      sub ? that.setData({ array: sub }) : that.subForm()
      that.data.pid = options.pid
      that.data.name = options.name
      // 时间选择动画
      that.datetimePicker = new pickerFile.pickerDatetime({
        page: that,
        animation: 'slide',
        duration: 200,
      })
      that.setData({ form: options.form })
      wx.hideShareMenu()
    }
    util.navBarTitle(navTitle)
  },
  onShow: function () {
    let that = this
    let options = that.data.options
    wx.getSetting({
      success: (res) => {
        if (res.authSetting["scope.userInfo"]) {
          app.getUserInfo(res => {
            that.onloadFunc(options)
          });
          that.setData({ showModalUser: false })
        }
        else {
          that.setData({ showModalUser: true })
        }
      }
    })
  },
  onReachBottom: function () {
    page.subMore(this)
  },

  subForm: function () {
    let that = this
    page.pageData(wx.getStorageSync("aid")).then(data => {
      if (data.isok) {
        let [array, pages] = [[], JSON.parse(data.msg.pages)]
        console.log(pages)
        for (let i = 0, key; key = pages[i++];) {
          if (key.def_name == '产品预约') {
            array.push(key.coms[0])
          }
        }
        wx.setStorageSync("sub", array)
        that.setData({
          array: array
        })
      }
    })
  },
  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    var that = this
    tools.showLoadToast('正在刷新')
    setTimeout(res => {
      tools.showToast("刷新成功")
      if (that.data.sublist) {
        wx.setStorageSync("listViewModel", '')
        tools.reset(that.data.listViewModel)
        page.subMore(that)
      }
      else {
        wx.setStorageSync("sub", "")
        that.subForm()
      }
      wx.stopPullDownRefresh()
    }, 1000)
  },


  // 性别选择
  bindPickerChange: function (e) {
    this.setData({ index: e.detail.value })
  },
  // 三联地址选择
  bindRegionChange: function (e) {
    this.setData({ region: e.detail.value })
  },
  //时间
  startTap: function () {
    this.datetimePicker.setPicker('startDate');
  },
  //选择位置位置
  chooseLocation: function (e) {
    wx.chooseLocation({ success: res => { this.setData({ region: res.address }) } })
  },
  // 表单提交
  submitFunc: function (e) {
    let that = this
    let formId = e.detail.formId
    util.commitFormId(formId, that)
    let [value, detail] = [that.data.array[0], e.detail.value]
    let myreg = /^(((13[0-9]{1})|(15[0-9]{1})|(18[0-9]{1})|(17[0-9]{1}))+\d{8})$/
    if (value.showname && detail.姓名 == '') {
      tools.showLoadToast("请填写姓名")
    }
    else if (value.showphone && !myreg.test(detail.手机号码)) {
      tools.showLoadToast("手机号有误")
    }
    else if (value.showage && detail.年龄 == '') {
      tools.showLoadToast("请填写年龄")
    }
    else if (value.showage && detail.年龄 == '0') {
      tools.showLoadToast("年龄不能为0")
    }
    else if (value.showdress && value.showmap == false && detail.地址 == '请选择地址') {
      tools.showLoadToast("请选择地址")
    }
    else if (value.showmap && detail.地址 == '请选择地址') {
      tools.showLoadToast('请选择地址')
    }
    else if (value.showtime && detail.预约时间 == '') {
      tools.showLoadToast("请选择预约时间")
    }
    else {
      let goods = JSON.stringify({ goods: { name: that.data.name, id: that.data.pid } })
      page.submitForm(JSON.stringify(detail), goods, that)
    }
  },

  userFunc: function (e) {
    let id = Number(e.target.id)
    switch (id) {
      case 0:
        tools.goBackPage(1)
        break;
      case 1:
        wx.openSetting({
          success: (data) => {
            this.setData({ showModalUser: false })
          }
        })
        break;
    }
  },
  /**
  * 用户点击右上角分享
  */
  onShareAppMessage: function () {
    if (that.data.sublist) {
      return {
        title: "我的预约单",
        path: '/pages/subscribe/subscribe?sublist=true',
        imageUrl: "",
        success: res => { tools.showToast("转发成功") }
      }
    }
  }
})