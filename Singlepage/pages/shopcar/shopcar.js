// pages/shopcar/shopcar.js
var util = require("../../utils/util.js");
var network = require("../../utils/network.js");
var addr = require("../../utils/addr.js");
var mulpicker = require("../../public/picktime.js");
var WxParse = require('../../utils/wxParse/wxParse.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    isplay: 1, //是否播放 0否 默认1播放
    indicatorDots: true,
    autoplay: true,
    inputDisable: true,
    // 时间控件参数
    DateTimePickerNums: 0,
    multiArray: [],
    mulindex: ['2017', '7', '25', '16', '22'],
    multimes: '请选择预约时间',
    pickerArray: [], // 选项picker
    s_data: {}, //提交form表单的数据
  },
  navo_webview: function() {
    wx.navigateTo({
      url: '/pages/web_view/web_view?id=' + this.data.AgentConfig.QrcodeId
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    // app.getUserInfo(function (e) {
    that.GetPageMsg()
    that.GetAgentConfigInfo()
    // })
    mulpicker.inite(this) //时间空间初始化
  },
  onShareAppMessage: function() {},
  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function() {
    var that = this
    that.data.DateTimePickerNums = 0
    that.data.alltime = []
    wx.stopBackgroundAudio()
    that.setData({
      obj: []
    })
    that.GetAgentConfigInfo()
    that.GetPageMsg()

    setTimeout(function() {
      wx.showToast({
        title: '刷新成功',
        icon: 'success',
        duration: 500
      })
      wx.stopPullDownRefresh()
      that.setData({
        condition: false
      })
    }, 1000)
  },
  // 查看地图位置
  markertap: function(e) {
    var index = e.currentTarget.id
    var latitude = Number(this.data.obj[index].markers[0].latitude)
    var longitude = Number(this.data.obj[index].markers[0].longitude)
    var address = e.currentTarget.dataset.address
    wx.openLocation({
      latitude: latitude,
      longitude: longitude,
      scale: 28,
      address: address
    })
  },
  // 拨打电话
  makePhoneCall: function() {
    if (this.data.phoneNumber == '') {
      wx.showToast({
        title: '暂无客服联系电话',
      })
      return
    } else {
      wx.makePhoneCall({
        phoneNumber: this.data.phoneNumber
      })
    }
  },
  // 点击查看轮播图
  previewImageAPI: function(e) {
    var obj = this.data.obj //数据大集合
    var index = e.currentTarget.id //单页版内第几个轮播图的下标
    var imageIndex = e.target.dataset.index //单图片的下标
    var array = obj[index].items //数据大集合内的第几个轮播图
    var imageArray = []
    var linkArray = [] //存放切割后的link
    var link = [] //存放是否跳转的标记 -1不跳转，1跳转
    var paramArr = "" //存放跳转参数数组 ["a=123","b=456"]
    var param = [] //切割"="后的参数数组
    var paramObj = {} //存放转换后的参数对象;
    for (var i = 0; i < array.length; i++) {
      imageArray.push(array[i].imgurl)
      linkArray.push(array[i].link.split(";"))
    }
    for (var i = 0; i < linkArray.length; i++) {
      link.push(linkArray[i][0])
    }
    var imageUrl = imageArray[imageIndex]
    if (link[imageIndex] == -1) {
      wx.previewImage({
        current: imageUrl,
        urls: imageArray
      })
    } else if (link[imageIndex] == 1) {
      var isParam = linkArray[imageIndex][2].split("?")[1]
      if (isParam != undefined && isParam != null) {
        paramArr = linkArray[imageIndex][2].split("?")[1].split("&&") //获取跳转页面路径，截取参数
        for (var i = 0; i < paramArr.length; i++) { //遍历参数数组，进行“=”切割
          param.push(paramArr[i].split("="))
          for (var j = 0; j < param.length; j++) { //对["a","123"]进行对象类型转换
            paramObj[param[j][0]] = param[j][1]
          }
        }
      }
      wx.navigateToMiniProgram({
        appId: linkArray[imageIndex][1],
        path: linkArray[imageIndex][2],
        extraData: paramObj, //跳转小程序传递的参数对象
        // envVersion: 'trial', //默认跳转正式版
        success(res) {
        },
        fail(err) {
          wx.showModal({
            title: "提示",
            content: "跳转失败",
          })
        }
      })
    }

  },
  // 点击查看单张图片
  previewSingleImage: function(e) {
    var imageurl = e.target.dataset.img
    var imageArray = []
    imageArray.push(imageurl)
    var img = imageArray[0]
    wx.previewImage({
      current: img,
      urls: imageArray
    })
  },
  // 时间选择器确定按钮
  timesure: function(e) {
    this.data.timeindex = e.currentTarget.id
    if (!this.data.conditiontime) {
      this.data.multimes = this.data.year + "-" + this.data.month + "-" + this.data.day + " " + this.data.our + ":" + this.data.minue
    }
    mulpicker.timesure(e, this)
  },
  // 时间选择器取消按钮
  timecancel: function() {
    mulpicker.timecancel(this)
  },
  bindMultiPickerChange: function(e) {
    mulpicker.bindMultiPickerChange(e, this)
  },
  clickOk: function(e) {
    var that = this
    mulpicker.clickOk(that)
    that.setData({
      conditiontime: !that.data.conditiontime,
      value: that.data.value_1
    })
  },
  // 播放背景音乐
  playvoice: function() {
    var isplay = this.data.isplay
    if (isplay == 1) {
      wx.playBackgroundAudio({
        dataUrl: this.data.audio
      })
      this.setData({
        isplay: 0
      })

      return
    } else {
      wx.stopBackgroundAudio()

      this.setData({
        isplay: 1
      })
    }
  },
  // picker选项
  bindPickerChange: function(e) {
    var ds = e.currentTarget.dataset
    var p_array = this.data.pickerArray
    p_array[ds.index].id = e.detail.value
    p_array[ds.index].title = ds.title
    p_array[ds.index].name = ds.name
    this.setData({
      pickerArray: p_array
    })

  },

  // 播放对应视频
  // setautoPlay: function (e) {
  // 	var obj = this.data.obj
  // 	for (var i = 0; i < obj.length; i++) {
  // 		if (obj[i].type == 'video' && e.currentTarget.id != i) {
  // 			obj[i].items[0].isAutoPlay = 0
  // 		} else {
  // 			obj[i].items[0].isAutoPlay = 1
  // 		}
  // 	}
  // 	this.setData({ obj: obj })
  // },
  // 表单提交，提交真是姓名和手机号码
  formSubmit: function(e) {
    var that = this
    var pa = that.data.pickerArray
    for (var key in e.detail.value) {
      if (e.detail.value[key] == '') {
        wx.showToast({
          title: '信息未填写完整',
          icon: 'loading'
        })
        return
      }
    }
    that.data.s_data = e.detail.value

    wx.request({
      url: addr.Address.SetForm,
      data: {
        formId: e.detail.formId,
        FormTitle: e.detail.target.dataset.name,
        AppId: app.globalData.appid,
        openId: wx.getStorageSync('userInfo').OpenId,
        FormMsg: JSON.stringify(e.detail.value)
      },
      method: "POST",
      header: {
        'content-type': 'application/json'
      },
      success: function(res) {
        that.setData({
          obj: [],
        })
        that.GetPageMsg()
        if (res.data.isok == 1) {
          that.data.DateTimePickerNums = 0
          that.data.alltime = []
          that.data.pickerArray = []
          wx.showToast({
            title: '报名成功',
            icon: 'success',
            duration: 2000
          })

          setTimeout(function() {
            that.setData({
              condition: false
            })
          }, 2000)
        } else {
          wx.showToast({
            title: '报名失败',
          })
        }
      },
    })
  },
  //通过pageid获取对应内容
  GetPageMsg: function(e) {
    var that = this
    var isplay = that.data.isplay
    var hidden = that.data.hidden
    var phoneNumber = ''
    wx.request({
      url: addr.Address.GetPageMsg,
      data: {
        AppId: app.globalData.appid,
      },
      method: "GET",
      header: {
        'content-type': 'application/json' // 默认值
      },
      success: function(res) {
        if (res.data.obj != null) {
          var obj = JSON.parse(res.data.obj.JsonMsg)
          for (var i = 0; i < obj.length; i++) {
            if (obj[i].type == 'Call') {
              phoneNumber = obj[i].items[0].phoneNumber
            }
            if (obj[i].type == 'video') {
              if (obj[i].items[0].isAutoPlay == 1) {
                var isAutoPlay = 1
              } else {
                var isAutoPlay = 0
              }
            }
            if (obj[i].type == 'form') {
              var na = []
              for (var l = 0; l < obj[i].items.length; l++) {
                for (var k = 0; k < obj[i].items[l].items.length; k++) {

                  if (obj[i].items[l].items[k].type == 'DateTimePicker') {
                    that.data.alltimeLength = obj[i].items[l].items[k].sys_index + 1
                    that.data.DateTimePickerNums++
                  }
                  if (obj[i].items[l].items[k].type == 'CheckBox') {
                    na[l] = 0
                  }
                }
              }
              for (var n = 0; n < na.length; n++) {
                that.data.pickerArray.push({
                  id: 0
                })
              }
            }
            if (obj[i].type == 'editor') {
              // 替换富文本标签 控制样式
              obj[i].items[0].txt = obj[i].items[0].txt.replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>')
              obj[i].items[0].txt = obj[i].items[0].txt.replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>')
              obj[i].items[0].txt = obj[i].items[0].txt.replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
              obj[i].items[0].txt = WxParse.wxParse('article', 'html', obj[i].items[0].txt, that, 5)
            }
            if (obj[i].type == 'map') {
              var markers = []
              var Lat = obj[i].items[0].mapLat
              var Lng = obj[i].items[0].mapLng
              markers.push({
                id: i,
                latitude: Lat,
                longitude: Lng
              })
              obj[i].markers = markers
            }
          }
          var template = obj.find(f => f.type == 'bmg')
          if (template) {
            var audio = template.items[0].bmgPath
          } else {
            var audio = ''
          }
          that.setData({
            obj: obj,
            phoneNumber: phoneNumber,
            hidden: 0,
            audio: audio,
            isplay: 1,
            pickerArray: that.data.pickerArray
          })
          // 动态设置顶部标题栏
          wx.setNavigationBarTitle({
            title: res.data.obj.PageTitle,
          })
          if (template && isAutoPlay != 1) {
            that.playvoice()
          }
        }
      },
      fail: function() {
        console.log("获取页面信息出错")
      }
    })
  },
  GetAgentConfigInfo: function(e) {
    var that = this
    wx.request({
      url: addr.Address.GetAgentConfigInfo,
      data: {
        appid: app.globalData.appid,
      },
      method: "GET",
      header: {
        'content-type': 'application/json'
      },
      success: function(res) {

        if (res.data.isok == 1) {
          if (res.data.AgentConfig.isdefaul == 0) {
            res.data.AgentConfig.LogoText = res.data.AgentConfig.LogoText.split(' ')
          } else {
            res.data.AgentConfig.LogoText = res.data.AgentConfig.LogoText
          }
          //是否显示推广按钮
          if (('QrcodeId' in res.data.AgentConfig) && res.data.AgentConfig.QrcodeId > 0 &&
            ('OpenExtension' in res.data.AgentConfig) && res.data.AgentConfig.OpenExtension == 0) {
            that.data.QrcodeId = res.data.AgentConfig.QrcodeId;
          }
          that.setData({
            AgentConfig: res.data.AgentConfig,
          })
        }
      },
      fail: function() {
        console.log("获取水印配置出错")
      }
    })
  },
})