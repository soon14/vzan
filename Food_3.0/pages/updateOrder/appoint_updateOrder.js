// pages/updateOrder/updateOrder.js
var util = require("../../utils/util.js");
var network = require("../../utils/network.js");
var addr = require("../../utils/addr.js");
var mulpicker = require("../../public/mulpicker.js");
var template = require('../../template/template.js');
var tool = require('../../template/Food2.0.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		clientTel: 0,//用户手机号码，用于渲染
		isOncepull: 0,//控制去支付按钮 防止多次点击

		couponsShow: false,//控制优惠券显隐
		vmMycoupon: {
			list: [],
			ispost: false,
			loadall: false,
			pageindex: 1,
			pagesize: 10,
			state: 0,
			listname: "pickmycoupon",
		},

		wayToPay: ['微信支付', '储值支付'],
		index: 0,//选择支付模式下标 0是微信 1是储值支付
		address: [],
		paymoeny: 0,//支付的总价格
		orderid: 0,//订单id
		ShippingFeeStr: 0,//配送费
		OutSideStr: 0,//起送价
		lengthCounr: 0, //循环请求商品加入购物车的次数 if == shopcartList.length,则提交完毕
		Message: "",
		TablesNo: '',
		logoimg: '',
		FoodsName: '',
		OrderType: '',//0 堂食 / 1 外卖
		allprice: 0,
		goodCarIdStr: '',//购物车id串
		addressId: 0,
		msg: '',
	},
	// 选择支付模式
	bindPickerChange: function (e) {
		this.setData({
			index: e.detail.value
		})
	},
	notuseCoupon: function () {
		this.setData({
			"pickCoupon": null,
			"couponsShow": false,
		});
	},
	pickCouponOK: function () {
		this.calMoney();
		this.setData({
			"couponsShow": false,
		});
	},
	chooseCoupons: function () {
		this.setData({ couponsShow: !this.data.couponsShow });
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		var ShippingFeeStr = app.globalData.ShippingFeeStr //配送费
		var OutSideStr = app.globalData.OutSideStr //起送价
		var shopcartList = JSON.parse(options.shopcartList)
		var allprice = options.allprice
		if (app.globalData.canSaveMoneyFunction != true) {
			that.data.wayToPay = that.data.wayToPay.splice(0, 1)
		}

		if (app.globalData.TablesNo == -999) { //外卖
			var paymoney = Number(options.alldiscountprice) + Number(ShippingFeeStr)
			if (Number(app.globalData.AccountMoneyStr) > Number(paymoney) && app.globalData.canSaveMoneyFunction == true) {
				var index = 1
				app.globalData.payType = 2 //储值支付
			} else {
				var index = 0
				app.globalData.payType = 1 //微信支付
			}
		}
		else {  //堂食
			var paymoney = Number(options.alldiscountprice)
			if (Number(app.globalData.AccountMoneyStr) > Number(paymoney) && app.globalData.canSaveMoneyFunction == true) { //堂食情况下，如果储值金额>买单金额 则默认选择储值金额 同上
				var index = 1
				app.globalData.payType = 2
			} else {
				var index = 0
				app.globalData.payType = 1
			}
		}


		that.setData({
			FoodsName: app.globalData.FoodsName,
			logoimg: app.globalData.logoimg,
			TablesNo: app.globalData.TablesNo,
			isOncepull: 0,
			clientTel: app.globalData.userInfo.TelePhone,
			wayToPay: that.data.wayToPay,
			index: index,//支付方式下标
			shopcartList: shopcartList,
			allprice: allprice, //商品总额
			reduceprice: (Number(allprice) - Number(options.alldiscountprice)).toFixed(2), //折扣优惠
			paymoney: (paymoney).toFixed(2),
			ShippingFeeStr: app.globalData.ShippingFeeStr, //配送费
		})
		that.loadMyCoupon()
		tool.GetReserveId(that)
	},
	// 跳转到选择地址页面
	navTosetAddress: function () {
		var that = this
		wx.navigateTo({
			url: '../setAddress/setAddress',
		})
	},
	// 订单备注
	inputMessage: function (e) {
		var Message = this.data.Message
		this.setData({ Message: e.detail.value })
	},
  /**
   * 生命周期函数--监听页面初次渲染完成
   */
	onReady: function () {

	},

  /**
   * 生命周期函数--监听页面显示
   */
	onShow: function (e) {
		if (app.globalData.dizhiId != '') {
			var addressId = parseInt(app.globalData.dizhiId)
		}
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
	// 获取用户手机号码
	getPhoneNumber: function (e) {
		var that = this
		console.log(e.detail.errMsg)
		console.log(e.detail.iv)
		console.log(e.detail.encryptedData)
		app.globalData.telEncryptedData = e.detail.encryptedData
		app.globalData.telIv = e.detail.iv
		app.globalData.isgetTel = 1
		app.getUserInfo(function (res) {
			if (res.TelePhone != '未绑定') {
				that.setData({ clientTel: res.TelePhone })
			}
		})
	},
	// 把商品加入购物车
	gotoPay: function (e) {
		var that = this

		app.globalData.paymoney = that.data.pickCoupon == null ? that.data.paymoney : that.data.money_cal_fmt
		// 提交备用formId
		var formId = e.detail.formId
		template.commitFormId(formId, that)
		app.globalData.isappoint_order = true
		var isclearshopcartList = 1
		app.globalData.isclearshopcartList = isclearshopcartList
		var shopcartList = that.data.shopcartList
		var lengthCounr = that.data.lengthCounr
		var goodCarIdStr = that.data.goodCarIdStr
		if (lengthCounr == 0 && that.data.isOncepull == 0) {
			that.data.isOncepull = 1
			for (var i = 0; i < shopcartList.length; i++) {
				//  循环请求
				wx.request({
					url: addr.Address.addGoodsCarData,
					data: {
						appId: app.globalData.appid,
						openid: app.globalData.userInfo.openId,
						goodid: shopcartList[i].goodid,
						attrSpacStr: shopcartList[i].attrSpacStr,
						SpecInfo: shopcartList[i].SpecInfo,
						GoodsNumber: shopcartList[i].buynums,
						newCartRecord: 1,
						isReservation: true
					},
					method: "POST",
					header: {
						'content-type': 'application/json' // 默认值
					},
					success: function (res) {
						if (res.data.isok == 1) {
							if (goodCarIdStr == '') {
								goodCarIdStr = res.data.cartid
							} else {
								goodCarIdStr = goodCarIdStr + ',' + res.data.cartid
							}
							lengthCounr++
							that.setData({
								goodCarIdStr: goodCarIdStr,
								lengthCounr: lengthCounr
							})
							if (lengthCounr == shopcartList.length) {
								if (that.data.index == 0) { //微信支付
									app.globalData.payType = 1
									that.PayOrder()
								} else { //储值支付
									wx.showModal({
										title: '提示',
										content: '是否确认使用储值支付？',
										success: function (res) {
											if (res.confirm) {
												app.globalData.payType = 2
												that.PayOrder()
											} else if (res.cancel) {
												that.data.isOncepull = 2
											}
										}
									})
								}
							}
						}
						console.log('~', goodCarIdStr)
					},
					fail: function () {
						console.log("提交购物车失败")
						wx.showToast({
							title: '提交购物车失败',
						})
					}
				})
			}
		} else {
			if (that.data.isOncepull == 2) {
				wx.showModal({
					title: '提示',
					content: '请返回上一页面再次提交订单',
					showCancel: false,
					success: function (res) {
						if (res.confirm) {
							app.goBackPage(1)
						}
					}
				})
			}
		}
	},




	// 支付
	PayOrder: function () {
		var that = this
		var lengthCounr = that.data.lengthCounr
		var orderid = that.data.orderid

		var orderjson = {
			AddressId: 0,
			Message: that.data.Message,
			TablesNo: "0",
			OrderType: 0,
		}
		var orderjson = JSON.stringify(orderjson)
		if (app.globalData.payType == 1) {
			var buyModeId = 1
		} else {
			var buyModeId = 2
		}
		var param = {
			appid: app.globalData.appid,
			openid: app.globalData.userInfo.openId,
			goodCarIdStr: that.data.goodCarIdStr,
			couponlogid: this.data.pickCoupon == null ? 0 : this.data.pickCoupon.Id,
			orderjson: orderjson,
			buyMode: buyModeId,
			reserveId: app.globalData.appoint_Id
		}
		util.AddOrder(param, function (e) {
			if (e == "failed") {
				wx.hideLoading()
				wx.showToast({ title: '您已取消了支付', icon: 'loading' })
				setTimeout(function () {
					tool.updateMiniappGoodsOrderState(app.globalData.dbOrder, 0, function (res) {
						wx.navigateBack({ delta: 1, })
					})
				}, 1000)
			}
			else {
				wx.showToast({ title: '支付成功', })
				setTimeout(function () {
					wx.redirectTo({ url: '../appointment_index/appointment_index' })
				}, 500)
			}
		}, "")
		that.setData({ lengthCounr: 99 })
	},
	// 储值支付
	buyOrderbySaveMoney: function () {
		var that = this
		wx.request({
			url: addr.Address.buyOrderbySaveMoney,
			data: {
				appid: app.globalData.appid,
				openid: app.globalData.userInfo.openId,
				goodsorderid: that.data.goodCarIdStr
			},
			method: "POST",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				if (res.data.isok == 1) {
					that.PayOrder()
				}
			},
			fail: function () {
				console.log("支付失败")
				wx.showToast({
					title: '获取首页出错',
				})
			}
		})
	},



	// 优惠券
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
		var goodsid = "";
		if (this.data.shopcartList) {
			goodsid = this.data.shopcartList.map(p => p.goodid).join(',');
		}

		template.GetMyCouponList({
			appId: app.globalData.appid,
			userId: app.globalData.userInfo.UserId,
			pageIndex: vm.pageindex,
			state: vm.state,
			goodsId: goodsid,
			goodsInfo: JSON.stringify(that.data.shopcartList.map(function (item, index) {
				return {
					goodid: item.goodid,
					totalprice: Number(item.discountprice) * item.buynums * 100
				}
			}))
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

	useMyCoupon: function (e) {
		var ds = e.currentTarget.dataset;
		console.log(ds);
		var selCoupon = this.data.vmMycoupon.list[ds.index];
		//如果选择的是指定商品优惠券，判断当前订单列表里的商品是否符合使用条件
		if (selCoupon.GoodsIdStr != "") {
			var specifiedGood = selCoupon.GoodsIdStr.split(',');

			//筛选出可优惠的产品
			var filterGood = this.data.goods.filter(function (item, index) {
				return specifiedGood.includes((item.goodid).toString());
			});

			//计算优惠商品的总价格 会员打折后的总价
			var totalPrice = 0;
			if (filterGood.length > 0) {
				filterGood.forEach(function (curValue) {
					totalPrice += (Number(curValue.discountPrice) || 0) * (Number(curValue.Count))
				})
			}
			console.log(filterGood);
			/*
			如果没有符合的指定商品
			或者指定商品的价格没有达到优惠标准
			*/
			if (filterGood.length == 0) {
				wx.showModal({
					title: '提示',
					content: '订单中没有优惠券指定的商品！',
				})
				this.setData({
					pickCoupon: null,
				});
				return;
			}
			else if (selCoupon.LimitMoney > 0 && totalPrice * 100 < selCoupon.LimitMoney) {
				wx.showModal({
					title: '提示',
					content: '指定商品满' + selCoupon.Money_fmt + '元才能使用此优惠券！',
				});
				this.setData({
					pickCoupon: null,
				});
				return;
			}
		}
		this.setData({
			pickCoupon: selCoupon
		});
		this.pickCouponOK();
	},





	calMoney: function () {
		var money = (Number(this.data.paymoney) || 0) * 100;
		//运费
		// var freight = (Number(this.data.chooseModel[this.data.index].sum) || 0) * 100;
		var freight = 0
		//先减去运费
		money = money - freight;


		var calMoney = money;
		var money_coupon = 0;//优惠的钱


		var d = this.data;
		var coupon = d.pickCoupon;
		if (money > 0) {

			//如果使用了优惠券
			if (coupon != null) {
				//全部商品
				if (coupon.GoodsIdStr == "") {
					if (coupon.LimitMoney <= 0 || money >= coupon.LimitMoney) {
						coupon.Money = Number(coupon.Money);
						//指定金额 - 优惠
						if (coupon.CouponWay == 0) {
							calMoney = Number(money) - Number(coupon.Money);
							money_coupon = (coupon.Money) / (100);
						}
						//折扣 * 折扣
						else if (coupon.CouponWay == 1) {
							var p = (coupon.Money) / (100) * (10) / (100);
							calMoney = (money) * (p);
							var coupon_p = (1) - Number(p);
							money_coupon = (money) * (coupon_p) / (100);
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
				//指定产品
				else {


					/*如果没有优惠券中指定的商品 提示不可用 */
					var specifiedGood = coupon.GoodsIdStr.split(',');
					var canUse = this.data.goods.some(function (item, index) {
						return specifiedGood.includes((item.goodid).toString());
					});
					if (!canUse) {
						wx.showModal({
							title: '提示',
							content: '不符合优惠券使用条件！',
						});
						this.setData({
							pickCoupon: null,
						});
						return;
					}


					//筛选出可优惠的产品
					var filterGood = this.data.goods.filter(function (item, index) {
						return specifiedGood.includes((item.goodid).toString());
					});

					//计算指定商品的价格之和
					//优惠券的金额都是分为单位 
					//商品的价格以元为单位
					var sumMoney = 0;
					filterGood.forEach(function (item, index) {
						sumMoney += (Number(item.discountPrice)) * (100) * (Number(item.Count));
					});


					//如果满足使用条件
					if (coupon.LimitMoney <= 0 || sumMoney >= coupon.LimitMoney) {
						coupon.Money = Number(coupon.Money);
						//指定金额 - 优惠
						if (coupon.CouponWay == 0) {
							money_coupon = coupon.Money;
						}
						//折扣 * 折扣
						else if (coupon.CouponWay == 1) {
							//重新计算价格


							var p = (coupon.Money) / (100) * (10) / (100);
							var coupon_p = (1) - Number(p);
							money_coupon = (sumMoney) * (coupon_p);
						}
						if (sumMoney < 0) {
							sumMoney = 0;
						}
						if (money_coupon < 0) {
							money_coupon = 0;
						}
						//优惠券优惠的最大金额就是指定商品的总价
						if (sumMoney - money_coupon < 0) {
							money_coupon = sumMoney;
						}
						calMoney = Number(money) - Number(money_coupon);

						if (calMoney < 0) {
							calMoney = 0;
						}


						//优惠券减掉的钱
						money_coupon = (money_coupon) / (100);
					}
				}

			}

		}
		//再把运费加回来
		calMoney = (calMoney) + (freight);

		var money_cal_fmt = (calMoney) / (100).toFixed(2);
		this.setData({
			money_cal: calMoney,
			money_cal_fmt: money_cal_fmt,
			money_coupon: money_coupon.toFixed(2),
		});
	},
})