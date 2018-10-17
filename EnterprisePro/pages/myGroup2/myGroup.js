// pages/myGroup/myGroup.js
var http = require("../../utils/http.js");
var addr = require("../../utils/addr.js");
var tools = require("../../utils/tools.js");
var util = require("../../utils/util.js");
var app = getApp();

var timer_countdown = null;

Page({

	/**
	 * 页面的初始数据
	 */
	data: {
		vm: {
			ispost: false,
			loadall: false,
			list: [],
			state: 0,
			pagesize: 10,
			pageindex: 1,
		},
		groupState: [
			{ name: "全部", state: 0 },
			{ name: "进行中", state: 1 },
			{ name: "已成功", state: 2 },
		],
		goodState_groupState: [
			{ name: "待付款", state: 0 },
			{ name: "未成团", state: 1 },
			{ name: "待收货", state: 2 },
			{ name: "交易成功", state: 3 },
			{ name: "已完成", state: 4 },
			{ name: "待发货", state: 5 },
			{ name: "订单失效", state: 6 },
		],
		orderState: {
			"1": { name: "已发货" },
			"0": { name: "待发货" },
			"-1": { name: "已收货" },
			"-2": { name: "已退款" },
			"-3": { name: "退款中" },
			"-4": { name: "已过期" },
		},
		groupState_fmt: {
			"0": { name: "单买" },
			"1": { name: "进行中" },
			"2": { name: "拼团成功" },
			"-1": { name: "拼团失败" },
		}
	},

	/**
	 * 生命周期函数--监听页面加载
	 */
	onLoad: function (options) {
		var that = this;
		tools.getUserInfo().then(function (user) {
			that.setData({
				"vm.uinfo": user
			});
			that.loadMore();
		})
	},
	loadMore: function () {
		var that = this, vm = this.data.vm, user = vm.uinfo;
		if (vm.ispost || vm.loadall || vm.uinfo == undefined)
			return;
		if (!vm.ispost) {
			that.setData({
				"vm.ispost": true
			})
		}
		http
			.postAsync(addr.Address.GetMyGroupList2,
			{
				appId: app.globalData.appid,
				userId: user.UserId,
				state: vm.state,
				pageIndex: vm.pageindex
			})
			.then(function (data) {
				for (var i = 0; i < data.postdata.length; i++) {
					if (data.postdata[i].state == 3) {
						data.postdata[i].state = 4
					}
					if (data.postdata[i].groupstate == 2 && data.postdata[i].state == 1) {
						data.postdata[i].state = 5
					}
					if (data.postdata[i].groupstate == -4 && data.postdata[i].state == -4) {
						data.postdata[i].state = 1
					}
					if (data.postdata[i].groupstate == 0 && data.postdata[i].state == -1) {
						data.postdata[i].state = 6
					}
					if (data.postdata[i].groupstate == 1 && data.postdata[i].state == -1) {
						data.postdata[i].state = 6
					}
					if (data.postdata[i].groupstate == 1 && data.postdata[i].state == 8){
						data.postdata[i].state = 1
					}
					if (data.postdata[i].groupstate == 2 && data.postdata[i].state == 8){
						data.postdata[i].state = 5
					}
				}
				wx.setNavigationBarTitle({
					title: that.data.groupState.find(p => p.state == vm.state).name + "拼团",
				})
				console.log(data);

				if (data.isok) {
					if (data.postdata.length < vm.pagesize) {
						vm.loadall = true;
					}

					if (data.postdata.length > 0) {
						if (vm.pageindex <= 1) {
							vm.list = data.postdata
						}
						else {
							vm.list = vm.list.concat(data.postdata);
						}

					}
					vm.ispost = false;
					that.setData({
						"vm": vm
					});
					that.initCountDown();
				}
				else {
					tools.alert("提示", data.msg);
				}


			});
	},
	//初始化倒计时
	initCountDown: function () {
		var vm = this.data.vm, that = this;
		if (vm.list.length > 0) {
			for (var i = vm.list.length - 1; i >= 0; i--) {
				if (vm.list[i].MState == 1) {
					var timespan = tools.getTimeSpan(vm.list[i].ShowDate);
					if (timespan <= 0) {
						vm.list.splice(i, 1)
					}
					else {
						var timeFormatArray = tools.formatMillisecond(timespan);
						var timeFormat = "";
						if (timeFormatArray[0] > 0) {
							timeFormat += timeFormatArray[0] + '天';
						}
						timeFormat += timeFormatArray[1] + '时' + timeFormatArray[2] + '分' + timeFormatArray[3] + '秒';
						vm.list[i].countdown = timeFormat;
					}

				}
			}
			that.setData({
				"vm": vm
			});
		}
		timer_countdown = setTimeout(function () {
			that.initCountDown();
		}, 1000);
	},
	changeMyGroupState: function (e) {
		console.log(e);
		var that = this, ds = e.currentTarget.dataset, vm = this.data.vm;
		that.resetVM();
		that.setData({
			"vm.state": ds.state
		});
		that.loadMore();
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
		this.resetVM();
		this.loadMore();
		setTimeout(function () {
			wx.stopPullDownRefresh()
		}, 1000)
	},
	resetVM: function () {
		var vm = this.data.vm;
		vm.ispost = false;
		vm.loadall = false;
		vm.pageindex = 1;
		vm.list = [];
		this.setData({
			"vm": vm
		});
	},
	/**
	 * 页面上拉触底事件的处理函数
	 */
	onReachBottom: function () {
		var vm = this.data.vm;
		this.setData({
			"vm.ispost": false,
			"vm.pageindex": vm.pageindex += 1
		});
		this.loadMore();
	},

	/**
	 * 用户点击右上角分享
	 */
	onShareAppMessage: function (res) {
		var group = res.target.dataset.group
		return {
			title: group.goodList[0].goodsMsg.name,
			imageUrl: group.goodList[0].goodsMsg.img,
			path: 'pages/myGroup2/myGroupDetail?groupid=' + group.groupid + '&aid=' + wx.getStorageSync('PageSetting').msg.aid
		}
	},
	// 二次支付
	gotopay: function (e) {
		this.goPayFuc(e.currentTarget.dataset.payid, e.currentTarget.dataset.detailid)
	},
	goPayFuc: function (citymorderId, orderId) {
		var newparam = {
			openId: app.globalData.userInfo.openId,
			orderid: citymorderId,
			'type': 1,
		}
		util.PayOrder(newparam, {
			success: function (res) {
				if (res == "wxpay") {
				} else if (res == "success") {
					wx.showToast({
						title: '支付成功',
						duration: 1000,
						icon: "loading"
					})
					setTimeout(function () {
						wx.redirectTo({ url: '/pages/group2/orderSuccess?gOrderid=' + orderId })
					}, 1000)
				}
			},
			failed: function () {
				wx.showToast({
					title: '您取消了支付',
					duration: 1000,
					icon: "loading"
				})
			},
		})
	},
	// 更改订单状态
	cancelorder: function (e) {
		var that = this
		wx.showModal({
			title: '提示',
			content: '亲，确认取消该次拼团吗？',
			success: function (res) {
				if (res.confirm) {
					that.changeStatusRequest(e.currentTarget.dataset.detailid, -1)
				}
			}
		})
	},
	changeOrderStatus: function (e) {
		var that = this
		wx.showModal({
			title: '提示',
			content: '亲，是否确认收货?',
			success: function (res) {
				if (res.confirm) {
					that.changeStatusRequest(e.currentTarget.dataset.detailid, 3)
				}
			}
		})
	},
	changeStatusRequest: function (goodsorderid, state) {
		var that = this
		http.postAsync(
			addr.Address.updateMiniappGoodsOrderState,
			{
				appid: app.globalData.appid,
				openid: app.globalData.userInfo.openId,
				orderId: goodsorderid,
				State: state
			}).then(function (data) {
				if (data.isok == 1) {
					wx.startPullDownRefresh()
				}
			})
	},

})