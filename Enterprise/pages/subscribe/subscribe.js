
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
    selectedDate: "",
    selectedTime: "",
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    let that = this
    that.data.options = options
    util.setPageSkin(that);
  },
  // initPickDateTime(formSetting) {
  //   var now = new Date();
  //   var targetDateTime = new Date();
  //   var defaultDateTime = new Date();
  //   switch (formSetting.timetype) {
  //     case "分钟":
  //       targetDateTime.setMinutes(now.getMinutes() + parseInt(formSetting.timecount));
  //       break;
  //     case "小时":
  //       targetDateTime.setHours(now.getHours() + parseInt(formSetting.timecount));
  //       break;
  //     case "天":
  //       targetDateTime.setDate(now.getDate() + parseInt(formSetting.timecount));
  //       break;
  //   }
  //   formSetting.startDate = targetDateTime.getFullYear() + "-" + (targetDateTime.getMonth() + 1) + "-" + targetDateTime.getDate();
  //   formSetting.startTime = targetDateTime.getHours() + ":" + targetDateTime.getMinutes();
  //   var t1 = targetDateTime.Format("yyyy年MM月dd日 HH:mm:00"); //targetDateTime.getFullYear() + "年" + (targetDateTime.getMonth() + 1) + "月" + targetDateTime.getDate() + "日 " + targetDateTime.getHours() + ":" + targetDateTime.getMinutes() + ":00";
  //   var t2 = targetDateTime.getFullYear() + "年" + (targetDateTime.getMonth() + 1) + "月" + targetDateTime.getDate() + "日" 
  //   this.setData({
  //     selectedDate: targetDateTime.Format("yyyy年MM月dd日 HH:mm:ss"),
  //     selectedTime: targetDateTime.Format("yyyy年MM月dd日 HH:mm:ss"),
  //     showdata: t1,
  //     startDate: t2,
  //     beginTime: t1,
  //   });
  // },
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
      let sub = "";//wx.getStorageSync("sub")

      if (sub) {
        var formSetting = sub[0];
        // this.initPickDateTime(formSetting);
      }

      sub ? that.setData({ array: sub }) : that.subForm()
      that.data.pid = options.pid
      that.data.name = options.name
      // 时间选择动画
      that.datetimePicker = new pickerFile.pickerDatetime({
        page: that,
        animation: 'slide',
        duration: 200,
      })
      that.setData({
        form: options.form,
      })

      wx.hideShareMenu()
    }
    util.navBarTitle(navTitle)
  },
  onShow: function () {
    let that = this
    let options = that.data.options
    that.onloadFunc(options)
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
        // this.initPickDateTime(array[0]);

        that.setData({
          array: array,

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

    // var date = this.data.selectedDate;
    // var time = this.data.selectedTime;
    // var formSetting = this.data.array[0];
    // var now = new Date();
    // var targetDateTime = new Date();

    // var strTime = date.toString() + " " + time.toString() + ":00"
    // var submitDateTime = new Date(strTime);
    // var timespan = 0;
    // switch (formSetting.timetype) {
    //   case "分钟":
    //     targetDateTime.setMinutes(now.getMinutes() + parseInt(formSetting.timecount));
    //     timespan = submitDateTime.getMinutes() - targetDateTime.getMinutes();
    //     break;
    //   case "小时":
    //     targetDateTime.setHours(now.getHours() + parseInt(formSetting.timecount));
    //     timespan = submitDateTime.getHours() - targetDateTime.getHours();
    //     break;
    //   case "天":
    //     targetDateTime.setDate(now.getDate() + parseInt(formSetting.timecount));
    //     timespan = submitDateTime.getDate() - targetDateTime.getDate()
    //     break;
    // }

    // console.log(timespan);
    // if (timespan < 0) {
    //   wx.showModal({
    //     title: '提示',
    //     content: '预约时间不能早于' + targetDateTime.Format("yyyy-MM-dd HH:mm"),
    //   })
    // }
    // return;



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
    else if (value.showtime && detail.预约时间 == '') {
      tools.showLoadToast("请选择10分钟以后的时间")
    }

    else {
      let goods = JSON.stringify({ goods: { name: that.data.name, id: that.data.pid } })
      page.submitForm(JSON.stringify(detail), goods, that)
    }


  },
  bindDateChange: function (e) {
    this.setData({
      "selectedDate": e.detail.value
    });
  },
  bindTimeChange: function (e) {
    this.setData({
      "selectedTime": e.detail.value
    });
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