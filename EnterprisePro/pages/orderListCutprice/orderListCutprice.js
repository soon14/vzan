// pages/orderList/orderList.js
var util = require("../../utils/util.js");
var addr = require("../../utils/addr.js");

var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		remark:'',//砍价下单remark 默认''
		goodId: 0,//商品id
		buid: 0,//砍价记录领取的buid
		Newallprice: 0,//应付价格
		newArray: [],//砍价商品集合
		// new

		//商品详情也传值
		gooddetaiSpecInfo: "",
		gooddetailqty: 0,
		gooddetailattrSpacStr: '',
		gooddetailgoodid: '',

		// 默认地址
		Address: [],
		openId: '',
		// 运费
		sum: 0,
		// 总价格
		totalMoney: 0,
		// 共amount件
		amount: 0,
		// 商品数量
		nums: 0,
		// 配送选择器属性
		chooseModel: [],
		array1: ['微信支付', '储值支付'],//支付方式
		array: [],
		index: 0,
		index1: 0,//支付方式下标
		//购物车Ids,以，分开
		goodCarIdStr: "",
		goods: [
			{ ImgUrl: '', SpecInfo: "", Introduction: "", Price: 1.01, Count: 0 }
		],
		sumprice: 0
	},
	// 选择地址提示
	showToastchooseaddress: function (e) {
		// 提交备用formId
		var formId = e.detail.formId
		util.commitFormId(formId, this)
		wx.showToast({
			title: '请选择地址',
			icon: 'loading'
		})
	},
	// 选择配送
	bindPickerChange: function (e) {
		console.log('picker发送选择改变，携带值为', e.detail.value)
		console.log(this.data.chooseModel)
		this.setData({
			index: e.detail.value,
			sumprice: (parseFloat(this.data.totalMoney) + parseFloat(this.data.chooseModel[e.detail.value].sum)).toFixed(2)
		})
		console.log(this.data.chooseModel[this.data.index])
	},
	// 跳转到我的地址页
	navTomyAddress: function () {
		var that = this
		wx.chooseAddress({ // 调用微信接口获取地址
			success: function (res) {
				that.setData({ Address: res })
			}
		})
	},
	inputMessage: function (e) {
		this.data.remark = e.detail.value
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		var Newallprice = parseFloat(Number(options.curPrcie) + Number(options.Freight)).toFixed(2)
		util.GetUserWxAddress(that) //查询默认地址
    if (app.globalData.storeConfig.funJoinModel.canSaveMoneyFunction) {
			that.data.array1 = that.data.array1
		} else {
			that.data.array1 = that.data.array1.splice(0, 1)
		}
		that.setData({ array1: that.data.array1, newArray: options, Newallprice: Newallprice, buid: options.buid, goodId: options.goodId })
    util.setPageSkin(that);
	},
	checkgooddetail: function (e, index1) {
		var that = this
		// 提交备用formId
		var formId = e.detail.formId
		util.commitFormId(formId, that)
		if (index1 == 1) {
			that.setData({ index1: 0 })
		}
		if (that.data.index1 == 0) {
			app.globalData.payType = 1 //1是微信支付
			var address = {
				userName: that.data.Address.userName,
				postalCode: that.data.Address.postalCode,
				provinceName: that.data.Address.provinceName,
				cityName: that.data.Address.cityName,
				countyName: that.data.Address.countyName,
				detailInfo: that.data.Address.detailInfo,
				telNumber: that.data.Address.telNumber
			}
			address = JSON.stringify(address)
			// 请求把商品加入购物车
			wx.request({
				url: addr.Address.AddBargainOrder,
				data: {
					AppId: app.globalData.appid,
					UserId: app.globalData.userInfo.UserId,
					buid: parseInt(that.data.buid),
					address: address,
					Remark: that.data.remark
				},
				method: "POST",
				header: {
					'content-type': 'application/json'
				},
				success: function (res) {
					if (res.data.isok) {
						that.inite5(res.data.orderId)
					}
					else {
						wx.showModal({
							title: '提示',
							content: '该商品已售罄',
							showCancel: false,
							success: function (res) {
								if (res.confirm) {
									wx.redirectTo({
										url: '../allbargain/allbargain',
									})
								}
							}
						})
					}
				}
			})
		}
		else {
			wx.showModal({
				title: '提示',
				content: '确认使用储值支付吗？',
				success: function (res) {
					if (res.confirm) {
						that.inite3(that.data.buid)
					} else if (res.cancel) {
						console.log('用户点击取消')
					}
				}
			})
		}
		// 提交备用formId
		// var formId = e.detail.formId
		// util.commitFormId(formId, that)
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
	// // 选择支付方式
	changePayway: function (e) {
		console.log('picker发送选择改变，携带值为', e.detail.value)
		this.setData({
			index1: e.detail.value
		})
	},
	// 执行支付
	inite5: function (orderid) {
		var that = this
		// var orderId = parseInt(app.globalData.orderid)
		var oradid = parseInt(orderid)
		var newparam = {
			openId: app.globalData.userInfo.openId,
			orderid: orderid,
			'type': 1,
		}
		util.PayOrder(newparam, {
			failed: function () {
				wx.showModal({
					title: '提示',
					content: '支付失败，客户取消支付',
				})
			},
			success: function (res) {
				if (res == "wxpay") {
				} else if (res == "success") {
					wx.showToast({
						title: '支付成功',
						duration: 500
					})
					app.globalData.dbOrder = that.data.buid
					app.globalData.payType = 2 //2是余额支付
					util.UpdateWxCard(that)
					wx.redirectTo({
						url: '../paysuccess/paysuccess?totalMoney=' + that.data.Newallprice,
					})
				}
			}
		})
	},
	// 申请砍价
	inite3: function (buid) {
		var that = this
		var address = {
			userName: that.data.Address.userName,
			postalCode: that.data.Address.postalCode,
			provinceName: that.data.Address.provinceName,
			cityName: that.data.Address.cityName,
			countyName: that.data.Address.countyName,
			detailInfo: that.data.Address.detailInfo,
			telNumber: that.data.Address.telNumber
		}
		address = JSON.stringify(address)
		wx.request({
			url: addr.Address.AddBargainOrder,
			data: {
				buid: buid,
				UserId: app.globalData.userInfo.UserId,
				AppId: app.globalData.appid,
				PayType: 2,
				address: address,
				Remark: that.data.remark
			},
			method: "POST",
			header: {
				'content-type': 'application/json'
			},
			success: function (res) {
				if (res.data.isok == true) {
					app.globalData.dbOrder = that.data.buid
					app.globalData.payType = 2 //2是余额支付
					wx.redirectTo({
						url: '../paysuccess/paysuccess?totalMoney=' + that.data.Newallprice,
					})

				} else if (res.data.isok == false && res.data.msg == '数据不存在') {
					wx.showModal({
						title: '提示',
						content: '很抱歉，该活动已过期!',
						showCancel: false,
						confirmText: '我知道了',
						success: function (res) {
							if (res.confirm) {
								wx.navigateBack({
									delta: 1
								})
							}
						}
					})
				}
				else {
					wx.showModal({
						title: '储值余额不足',
						content: '您的余额不足，是否使用微信支付？',
						success: function (res) {
							if (res.confirm) {
								that.checkgooddetail(e, 1)
							} else if (res.cancel) {
								wx.redirectTo({
									url: '../mycutprice/mycutprice?State=0',
								})
							}
						}
					})
				}
			},
			fail: function () {
				console.log("获取信息出错")
				wx.showToast({
					title: '获取信息出错',
				})
			}
		})
	},
})