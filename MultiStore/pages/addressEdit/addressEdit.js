const app = getApp();
const tools = require("../../utils/tools.js");
const pageRequest = require("../../utils/public-request.js")
Page({

  /**
   * 页面的初始数据
   */
  data: {
    listViewModal: {
      pageindex: 1,
      pagesize: 10,
      list: [],
      ispost: false,
      loadall: false,
    }
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    this.data.options = options
  },
  onShow: function () {
    this.canUser(this.data.options)
  },
  // 检测用户是否授权
  canUser: function (options) {
    let that = this
    if (getCurrentPages()[1].route == "pages/addressSelect/addressSelect") {
      that.setData({ address: options.address })
      that.data.Lat = options.lat
      that.data.Lng = options.lng
    }
    else {
      // 修改地址
      Object.keys(options).length ? pageRequest.getMyaddress(that, options.id) : ""
    }

  },

  //选择位置位置
  chooseLocFunc: function (e) {
    console.log(e)
    let that = this
    wx.chooseLocation({
      success: res => {
        that.setData({ address: res.address }); that.data.Lat = res.latitude; that.data.Lng = res.longitude
      },
    })
  },
  // 提交地址信息
  sumbitFormFuc: function (e) {
    let that = this
    let [detail, myreg] = [e.detail.value, /^(((13[0-9]{1})|(15[0-9]{1})|(18[0-9]{1})|(17[0-9]{1}))+\d{8})$/]
    that.data.detail = detail
    if (detail.NickName == '') { tools.showToast('请填写姓名'); return }
    else if (!myreg.test(detail.TelePhone)) { tools.showToast("手机号码有误"); return }
    else if (detail.address == '') { tools.showToast("请填写地址"); return }
    else {
      let [Lat, Lng, Id] = [0, 0, 0]
      Lat = that.data.Lat
      Lng = that.data.Lng
      that.data.id ? Id = that.data.id : Id = 0
      let addressJson = {
        Id: Id,
        Name: detail.detailAddress,
        Lat: Lat,
        Lng: Lng,
        NickName: detail.NickName,
        TelePhone: detail.TelePhone,
        Address: detail.address,
      }
      addressJson = JSON.stringify(addressJson)
      pageRequest.saveAddress(addressJson, that)
    }
    console.log(detail)
  },
  //重置
  clearFunc: function () {
    this.setData({
      name: "",
      phone: "",
      address: "",
      detailAddress: "",
    })
  },
})