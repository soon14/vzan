// pages/me/payment.js
const page = require('../../utils/pageRequest.js');
const addr = require("../../utils/addr.js");
const http = require("../../utils/http.js");
const tools = require("../../utils/tools.js");
let app = getApp();

Page({

  /**
   * 页面的初始数据
   */
  data: {
    payMoney: "",//支付金额
    money_coupon: 0,//优惠券优惠的钱
    money_vip: 0,//会员折扣优惠的钱
    money_cal: "",//小计金额，单位分
    money_cal_fmt: "",//用于显示单位元
    canpay: false,
    ispost: false,
    couponsShow: false,
    vmMycoupon: {
      list: [],
      ispost: false,
      loadall: false,
      pageindex: 1,
      pagesize: 10,
      state: 0,
      listname: "pickmycoupon",
    },
    vmStorecoupon: {
      list: [],
      ispost: false,
      loadall: false,
      pageindex: 1,
      pagesize: 10,
    },
    pickCoupon: null,
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {

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
    this.setData({
      vmMycoupon: {
        list: [],
        ispost: false,
        loadall: false,
        pageindex: 1,
        pagesize: 10,
        state: 0,
        listname: "pickmycoupon",
      },
    });
    this.init();
  },
  init: function () {
    let that = this;
    let openid = wx.getStorageSync("userInfo").openId
    let uid = wx.getStorageSync("userInfo").UserId
    page.userOfmoneyRequest(openid, that);
    page.memberInfo(uid, that).then(res => {
      if (res.isok) {
        that.setData({ vipinfo: res.model });
        that.loadMyCoupon();
      }
    })
  },
  loadMyCoupon: function () {
    var that = this;
    var d = that.data;
    var vm = that.data.vmMycoupon;
    if (vm.ispost || vm.loadall)
      return;

    if (!vm.ispost) {
      this.setData({
        "vmMycoupon.ispost": true,
      });
    }

    tools.GetMyCouponList({
      appId: app.globalData.appid,
      userId: app.globalData.userInfo.UserId,
      pageIndex: vm.pageindex,
      state: vm.state,
      goodsId: 0,
    }).then(function (res) {
      console.log(res);
      if (res.isok) {
        if (res.postdata.length >= vm.pagesize) {
          vm.pageindex += 1;
        }
        else {
          vm.loadall = true;
        }
        vm.list = vm.list.concat(res.postdata);
        vm.ispost = false;
      }
      that.setData({
        vmMycoupon: vm
      })
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
    this.init();
    setTimeout(function () {
      wx.stopPullDownRefresh();
    }, 500);
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
  //输入金额
  inputMoney: function (e) {
    var value = e.detail.value;
    var money = Number(value) || 0;
    if (/[^\d.]/gi.test(value)) {
      this.setData({
        "payMoney": "",
        money_cal_fmt: "",
        money_vip: 0,
        money_coupon: 0,
      });
      return;
    }
    if (value === ".") {
      this.setData({ "payMoney": "0." });
      return;
    }
    //整数位大于7
    if ((value.split('.')).length - 1 == 0 && value.length > 7) {
      this.setData({
        "payMoney": value.substring(0, 7),
        "canpay": false
      });
      return;
    }
    //多个小数点
    if ((value.split('.')).length - 1 > 1) {
      var v = value.split('.');
      this.setData({
        "payMoney": v[0] + ".",
        "canpay": false
      });
      return;
    }
    //小数位大于2
    if ((value.split('.')).length - 1 == 1 && value.split('.')[1].length > 2) {
      var v = value.split('.');
      this.setData({
        "payMoney": v[0] + "." + v[1].substring(0, 2),
        "canpay": false
      });
      return;
    }

    if (!/^[\d]{1,7}.?([\d]{1,2})?$/gi.test(value)) {
      this.setData({
        "payMoney": "",
        "money_cal_fmt": "",
        money_vip: 0,
        money_coupon: 0,
        "canpay": false
      });
      return;
    }
    
    this.setData({ "canpay": true, payMoney: value });
    this.calMoney();

  },
  calMoney: function () {
    var money = Number(this.data.payMoney) || 0;
    money = money * 100;
    var calMoney = money;
    var money_coupon = 0;
    var money_vip = 0;

    var d = this.data;
    var coupon = d.pickCoupon;
    if (money > 0) {

      //如果有会员折扣
      var vip = d.vipinfo;
      if (vip.levelInfo && vip.levelInfo.type == 1) {
        var precalMoney = calMoney;
        calMoney = calMoney.mul(vip.levelInfo.discount.div(100));
        var vip_p = (1).sub(vip.levelInfo.discount.div(100));
        money_vip = precalMoney.mul(vip_p).div(100);
      }
      if (calMoney < 0) {
        calMoney = 0;
      }

      //如果使用了优惠券
      if (coupon != null) {
        if (coupon.LimitMoney <= 0 || calMoney >= coupon.LimitMoney) {
          coupon.Money = Number(coupon.Money);
          //指定金额 - 优惠
          if (coupon.CouponWay == 0) {
            calMoney = calMoney.sub(coupon.Money);
            money_coupon = coupon.Money.div(100);
          }
          //折扣 * 折扣
          else if (coupon.CouponWay == 1) {
            var p = Number(coupon.Money_fmt).div(10);
            //会员卡折扣后的金额
            var money_aftervip = calMoney;
            calMoney = calMoney.mul(p);
            var coupon_p = (1).sub(p);
            money_coupon = money_aftervip.mul(coupon_p).div(100);
          }

          if (calMoney < 0) {
            calMoney = 0;
          }
        }
        else {
          wx.showModal({
            title: '提示',
            content: '未达到优惠券使用条件！',
          });
          this.setData({
            pickCoupon: null,
          });
          return;
        }
      }
      if (calMoney > this.data.saveMoney*100) {
        this.setData({
          "canpay": false
        });
      }
      else{
        this.setData({
          "canpay": true
        });
      }
    }
    this.setData({
      money_cal: calMoney,
      money_cal_fmt: calMoney.div(100).toFixed(2),
      money_vip: money_vip.toFixed(2),
      money_coupon: money_coupon.toFixed(2),
    });
  },
  pay: function () {
    var that = this;
    wx.showModal({
      title: '确认支付吗？',
      showCancel: true,
      success: function (res) {
        if (res.confirm) {
          that.payRequest();
        }
      },
    })
  },
  //支付
  payRequest: function () {
    var that = this;
    var d = this.data;
    var paymoney = Number(d.payMoney);//输入金额
    var calmoney = Number(d.money_cal);//支付金额


    if (!app.globalData.userInfo) {
      wx.showModal({
        title: '提示',
        content: '请先登录！',
        showCancel: false
      })
      return;
    }
    if (d.payMoney <= 0 || !d.canpay) {
      return;
    }
    if (!('saveMoney' in d) || !d.saveMoney) {
      wx.showModal({
        title: '提示',
        content: '请先充值',
        showCancel: false
      })
      return;
    }
    if (calmoney > d.saveMoney*100) {
      wx.showModal({
        title: '提示',
        content: '余额不足',
        showCancel: false
      })
      return;
    }
    //检查优惠券是否可用
    if (d.pickCoupon) {
      //有使用限制
      if (d.pickCoupon.LimitMoney > 0) {
        if (paymoney.mul(100) < d.pickCoupon.LimitMoney) {
          wx.showModal({
            title: '提示',
            content: '未达到优惠券使用条件！',
            showCancel: false
          })
          return false;
        }
      }
    }

    if (d.ispost)
      return;

    if (!d.ispost) {
      this.setData({ ispost: true });
    }


    wx.showLoading({
      title: '正在支付',
    })
    http.postAsync(addr.Address.PayByStoredvalue,
      {
        appid: app.globalData.appid,
        userId: app.globalData.userInfo.UserId,
        money: paymoney * 100,//输入金额
        openId: app.globalData.userInfo.openId,
        couponid: d.pickCoupon == null ? 0 : d.pickCoupon.Id,
        money_cal: calmoney,//支付金额
        money_coupon: d.money_coupon,
        money_vip: d.money_vip,
        levelid: d.vipinfo.levelid
      }).then(function (res) {
        console.log(res);
        wx.showToast({
          title: res.msg,
        })
        if (res.result) {
          setTimeout(function () {
            wx.redirectTo({
              url: '/pages/me/payresult?orderid=' + res.obj.orderid,
            })
          }, 1000);
        }
      }, function (res) {
        console.log(res);
      });
  },
  chooseCoupons: function () {
    this.setData({
      couponsShow: !this.data.couponsShow
    });


  },
  useMyCoupon: function (e) {
    var ds = e.currentTarget.dataset;
    console.log(ds);
    this.setData({
      pickCoupon: this.data.vmMycoupon.list[ds.index]
    });
    this.pickCouponOK();
    this.calMoney();
  },
  pickCouponOK: function () {
    this.setData({
      "couponsShow": false,
    });
    this.calMoney();
  },
  notuseCoupon: function () {
    this.setData({
      "pickCoupon": null,
      "couponsShow": false,
    });
    this.calMoney();
  },
  reachCouponBottom: function () {
    var vm = this.data.vmMycoupon;
    if (!vm.ispost && !vm.loadall) {
      this.loadMyCoupon();
    }
  }
})