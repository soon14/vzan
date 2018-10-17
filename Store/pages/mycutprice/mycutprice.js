// pages/mycutprice/mycutprice.js
var addr = require("../../utils/addr.js");
var util = require("../../utils/util.js");
var util = require("../../utils/util.js");
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
	data: {
		pageindex: 2,//控制上拉加载更多
		pullUpNums: 2,//上拉加载更多的页面
		goodId: 0,//商品id
		nickName: '',//当前登陆用户的名字
		avatarUrl: '',//当前登陆用户的头像
		cutpriceList: [],//帮砍记录列表
		vipwindow: false,//帮砍记录弹窗
		condition: -2,//头部选择栏状态，默认全部 -2
		item: [
			{ name: '全部', condition: -2 },
			{ name: '待付款', condition: 0 },
			{ name: '待发货', condition: 7 },
			{ name: '待收货', condition: 6 },
			{ name: '已完成/退款', condition: 8 }
		],    // 头部分类
		// 砍价单列表
		obj: [],
		// item1: [
		//   { img: '/image/test.png', name: '砍价标题', condition: '待发货', yuanjia: '198.00', xianjia: '190.00', renshu: '2' },
		//   { img: '/image/test.png', name: '砍价标题', condition: '待发货', yuanjia: '198.00', xianjia: '190.00', renshu: '2' },
		//   { img: '/image/test.png', name: '砍价标题', condition: '待发货', yuanjia: '198.00', xianjia: '190.00', renshu: '2' },
		//   { img: '/image/test.png', name: '砍价标题', condition: '待发货', yuanjia: '198.00', xianjia: '190.00', renshu: '2' },
		//   { img: '/image/test.png', name: '砍价标题', condition: '待发货', yuanjia: '198.00', xianjia: '190.00', renshu: '2' },
		//   { img: '/image/test.png', name: '砍价标题', condition: '待发货', yuanjia: '198.00', xianjia: '190.00', renshu: '2' },
		//   { img: '/image/test.png', name: '砍价标题', condition: '待发货', yuanjia: '198.00', xianjia: '190.00', renshu: '2' },
		//   { img: '/image/test.png', name: '砍价标题', condition: '待发货', yuanjia: '198.00', xianjia: '190.00', renshu: '2' },
		// ]
	},
	// 跳转到砍价详情
	navtoBargaindetail: function (e) {
		var Id = e.currentTarget.id
		wx.navigateTo({
			url: '../bargaindetail/bargaindetail?Id=' + Id,
		})
	},
	// 帮砍记录弹窗
	showVipmodal: function () {
		var vipwindow = this.data.vipwindow
		this.setData({ vipwindow: !this.data.vipwindow })
	},
	checkList: function (e) {
		var that = this
		var index = e.currentTarget.id
		that.inite1(index)
	},
	// 跳转到订单详情
	navtoOrderdetail: function (e) {
		var index = e.currentTarget.id
		app.globalData.dbOrder = index
		app.globalData.payType = 2
		wx.navigateTo({
			url: '../orderDetail/orderDetail?orderId=' + index
		})
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		if (options.State == 0) { //从其他页面直接跳转到此页面 查询未付款订单
			that.inite(1, 0, 0)
			// that.setData({ condition: 0 })
		} else {
			that.inite(1, that.data.condition, 0)
		}
		that.setData({ nickName: app.globalData.userInfo.nickName, avatarUrl: app.globalData.userInfo.avatarUrl })
	},
	// 改变头部状态
	changeType: function (e) {
		var that = this
		var index = e.currentTarget.id
		that.setData({ pullUpNums: 0, pageindex: 2 })
		that.inite(1, index, 0)

	},
	// 现价购买按钮
	buybynowPrice: function (e) {
		var that = this
		var index = e.currentTarget.id
		var goodId = e.currentTarget.dataset.goodid
		that.setData({ goodId: goodId })
		that.inite2(index)
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
		var that = this
		that.inite(that.data.pageindex, that.data.condition, 1)
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
	// 订单状态state为1的时候
	showyibuy: function (e) {
		wx.showModal({
			title: '提示',
			content: '亲，商家准备急速配送中，请耐心等待接单。',
			showCancel: false,
			success: function (res) {
				if (res.confirm) {
					console.log('用户点击确定')
				} else if (res.cancel) {
					console.log('用户点击取消')
				}
			}
		})
	},
	showreturnmoney: function (e) {
		wx.showModal({
			title: '提示',
			content: '亲，商家退款中。',
			showCancel: false,
			success: function (res) {
				if (res.confirm) {
					console.log('用户点击确定')
				} else if (res.cancel) {
					console.log('用户点击取消')
				}
			}
		})
	},
	// 确认收货按钮
	sureGetgood: function (e) {
		var that = this
		var index = e.currentTarget.id
		wx.showModal({
			title: '提示',
			content: '亲，是否确认收货？',
			success: function (res) {
				if (res.confirm) {
					that.inite3(index)
				} else if (res.cancel) {
					console.log('用户点击取消')
				}
			}
		})

	},
  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
	onPullDownRefresh: function () {
		this.setData({ pageindex: 2 })
		this.data.obj = []
		this.inite(1, -2, 0)
		wx.stopPullDownRefresh()
	},

  /**
   * 页面上拉触底事件的处理函数
   */
	onReachBottom: function () {
		var that = this
		that.inite(that.data.pageindex, that.data.condition, 1)
	},

  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {

	},
	//获取砍价单列表
	inite: function (pageIndex, State, pullUpNums) {
		var that = this
		var pullUpNums = pullUpNums
		// if (State == -1) {
		//   var State = ''
		// } else {
		//   var State = State
		// }
		wx.request({
			url: addr.Address.GetBargainUserList,
			data: {
				appId: app.globalData.appid,
				UserId: app.globalData.userInfo.UserId,
				pageIndex: pageIndex,
				pageSize: 5,
				State: State,
			},
			method: "POST",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				if (res.data.isok) {
					if (pullUpNums == 0) { //判断是否上拉加载更多时间，如果=1就直接保存，如果大于1就使用concat
						// if (State == '') {
						//   var condition = -1
						// }
						// if (State != '') {
						//   var condition = State
						// }
						// if (State == '') {
						//   var condition = -1
						// } else {
						//   var condition = State
						// }
						that.setData({ obj: res.data.obj, condition: State })
						return
					}
					if (pullUpNums == 1 && res.data.obj.length != 0) { //如果==1 则是上拉加载
						var obj = that.data.obj
						obj = obj.concat(res.data.obj)
						// if (State == -1) {
						//   var condition = ''
						// }
						// if (State != '') {
						//   var condition = State
						// }
						// if (State == '') {
						//   var condition = -1
						// } 
						// else {
						//   var condition = State
						// }
						var pageindex = that.data.pageindex + 1
						that.setData({ obj: obj, condition: State, pageindex: pageindex })
						return
					}
					// if (pullUpNums == 2) { //如果是2 则是点击头栏时间
					//   var newObj = res.data.obj
					//   that.setData({ obj: newObj,pullUpNums: pullUpNums })
					// }
				}
			},
			fail: function (e) {
				console.log('一键分享获取失败')
			}
		})
	},
	//查看砍价记录
	inite1: function (buid) {
		var that = this
		var vipwindow = this.data.vipwindow
		wx.request({
			url: addr.Address.GetBargainRecordList,
			data: {
				buid: buid,
				pageIndex: 1,
				pageSize: 100,
			},
			method: "POST",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				if (res.data.isok) {
					var cutpriceList = res.data.obj
					for (var i = 0; i < cutpriceList.length; i++) {
						cutpriceList[i].CreateDate = that.ChangeDateFormat(cutpriceList[i].CreateDate)
					}
					that.setData({ vipwindow: !that.data.vipwindow, cutpriceList: cutpriceList })
				}
			},
			fail: function (e) {
				console.log('一键分享获取失败')
			}
		})
	},
	ChangeDateFormat: function (val) {
		if (val != null) {
			var date = new Date(parseInt(val.replace("/Date(", "").replace(")/", ""), 10));
			//月份为0-11，所以+1，月份小于10时补个0
			var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
			var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
			var hour = date.getHours();
			var minute = date.getMinutes();
			var second = date.getSeconds();
			var dd = date.getFullYear() + "-" + month + "-" + currentDate + " " + hour + ":" + minute + ":" + second;
			// console.log(dd)
			return dd;
		}
		return "";
	},
	//现价购买
	inite2: function (buid) {
		var that = this
		var vipwindow = this.data.vipwindow
		wx.request({
			url: addr.Address.GetBargainUser,
			data: {
				buid: buid,
				userid: app.globalData.userInfo.UserId
			},
			method: "POST",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				if (res.data.isok) {
					wx.navigateTo({
						url: '../orderListCutprice/orderListCutprice?BName=' + res.data.obj.BName + '&Freight=' + res.data.obj.Freight + '&ImgUrl=' + res.data.obj.ImgUrl + '&curPrcie=' + res.data.obj.curPrcie + '&buid=' + buid + '&goodId=' + that.data.goodId,
					})
				}
			},
			fail: function (e) {

				console.log('一键分享获取失败')
			}
		})
	},
	// 二次支付
	gotopay: function (e) {
		var that = this
		// var orderId = parseInt(app.globalData.orderid)
		var oradid = e.currentTarget.id
		var newparam = {
			openId: app.globalData.userInfo.openId,
			orderid: oradid,
			'type': 1,
		}
		util.PayOrder(oradid, newparam, {
			failed: function (res) {
				wx.showModal({
					title: '提示',
					content: res.data.msg,
				})
			},
			success: function (res) {
				if (res == "wxpay") {

				} else if (res == "success") {
					wx.showToast({
						title: '支付成功',
						duration: 500
					})
					that.inite(that.data.pageindex, that.data.condition, 1)
				}
			}
			//       success: function (res) {
			//   if (res == "wxpay") {
			//   } else if (res == "success") {
			//     wx.showToast({
			//       title: '支付成功',
			//       duration: 500
			//     })
			//     // that.inite(0, 0, 2, 0)
			//     // that.inite(that.data.pageindex, that.data.condition, 1)
			//   }
			// }
		})
	},
	//确认收货
	inite3: function (buid) {
		var that = this
		var vipwindow = this.data.vipwindow
		wx.request({
			url: addr.Address.ConfirmReceive,
			data: {
				buid: buid,
				userid: app.globalData.userInfo.UserId,
				appId: app.globalData.appid
			},
			method: "POST",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				if (res.data.isok) {
					that.inite(1, -1, 0)
				}
			},
			fail: function (e) {

				console.log('一键分享获取失败')
			}
		})
	},
})