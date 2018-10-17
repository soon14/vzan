
// pages/orderList/orderList.js
const page = require('../../utils/pageRequest.js');
const tools = require("../../utils/tools.js");
const util = require("../../utils/util.js");
const addr = require("../../utils/addr.js");
var http = require('../../utils/http.js');
const app = getApp();
Page({

	/**
	 * 页面的初始数据
	 */
	data: {
		getWayinfo: [],//到店自取信息
		getWay: 3,//0自取1商家配送3默认
		Address: {},//地址
		sum: 0,  // 运费
		discountTotal: 0,    // 总价格
		totalCount: 0,    // 共totalCount件
		chooseModel: [],  // 配送选择器属性
		array1: ['微信支付', '储值支付'],//支付方式
		array: [],
		index: 0,
		paymode: 0,//支付方式 新增提交虚拟formid
		index1: 0,
		vmMycoupon: {
			list: [],
			ispost: false,
			loadall: false,
			pageindex: 1,
			pagesize: 10,
			state: 0,
			listname: "pickmycoupon",
		},
		pickCoupon: null,
		couponsShow: false,
		money_coupon: 0,
		sendWay: '',
		fee: 0, //运费
		canpay: false, //是否可以下单（有没有超出商家配送范围）
		requestMark: false,
		sumMoney: 0,

	},
	/**
	 * 生命周期函数--监听页面加载
	 */
	onLoad: function (options) {
		let that = this
		page.GetStoreInfo(this, 0)
		if (options.datajson != undefined) {
			var totalCount = 0
			var originalPriceItem = 0
			var menu = JSON.parse(options.datajson)
			for (let i = 0; i < menu.length; i++) {
				totalCount += menu[i].Count
				var oldPrice = menu[i].oldPrice//单个初始价格
				originalPriceItem += oldPrice * menu[i].Count//累加初始价格
			}
			var originalPrice = parseFloat(originalPriceItem).toFixed(2)//初始总价单位换算
			if (options.isgroup == 1) {
				var totalPrice = parseFloat(Number(options.discountTotal) - Number(options.headerReducePrice)).toFixed(2)
			} else {
				var totalPrice = parseFloat(options.discountTotal).toFixed(2)
			}
			var lessPrice = parseFloat(originalPrice - options.discountTotal).toFixed(2)
			that.data.sumMoney = Number(totalPrice).toFixed(2)
			that.setData({
				sumMoney: that.data.sumMoney,
			})
			that.setData({
				goods: menu,
				lessPrice: lessPrice,//显示折扣优惠
				totalPrice: totalPrice < 0.01 ? 0.00 : totalPrice,//折扣总价
				totalCount: totalCount,//数量
				originalPrice: originalPrice,
				headerReducePrice: options.headerReducePrice,
				isgroup: options.isgroup,
				vipdiscount: options.vipdiscount
			})

			that.data.goodCarIdStr = options.goodCarIdStr
			that.data.discountTotal = options.discountTotal
			that.data.GId = options.GId
		}

		util.setPageSkin(that)


		//	that.freightRequest(that.data.goodCarIdStr) //旧版的运费请求
		// that.AddressRequest()
		page.memberInfo(wx.getStorageSync("userInfo").UserId, that).then(data => {
			that.setData({ vipinfo: data });
			that.loadMyCoupon();
		});

	},
	onShow: function () {


		page.GetStoreInfo(this, 0)
		wx.getSetting({
			success: (res) => {
				if (res.authSetting["scope.address"]) {
					if (res.authSetting["scope.address"]) {
						this.setData({ showModalUser: false })
					}
					else {
						this.setData({ showModalUser: true })
					}
				}
			}
		})

		var that = this
		that.data.Address.userName = app.globalData.shippingAddress.contact
		that.data.Address.telNumber = app.globalData.shippingAddress.phone
		that.data.Address.provinceName = app.globalData.shippingAddress.province
		that.data.Address.cityName = app.globalData.shippingAddress.city
		that.data.Address.countyName = app.globalData.shippingAddress.district
		that.data.Address.detailInfo = app.globalData.shippingAddress.street
		// console.log("地址显示", that.data.Address.userName)
		that.setData({
			Address: that.data.Address
		})
		if (that.data.Address.provinceName != undefined && that.data.requestMark == true)
		// if (!that.data.requestMark)
		{
			//  console.log("是否进行了判断")
			// that.setData({
			// 	requestMark: true,
			// })
			// return

			that.getFee(that.data.goodCarIdStr)
		}

	},
	//运费模板请求
	getFee: function (goodCarIdStr) {
		let that = this
		console.log("发送的省份是", that.data.Address.provinceName)
		http.postAsync(
			addr.Address.GetFreightFee,
			{
				appId: app.globalData.appid,
				openId: app.globalData.userInfo.openId,
				goodCartIds: goodCarIdStr,
				province: that.data.Address.provinceName,
				city: that.data.Address.cityName,

			}
		)

			.then(function (data) {
				if (data.isok == 1) {
					that.data.fee = data.data.fee / 1000
					that.data.canpay = data.data.canpay
					that.data.sumMoney = (Number(that.data.totalPrice) + Number(data.data.fee / 100)).toFixed(2)
					that.setData({
						fee: data.data.fee / 100,//显示运费
						canpay: data.data.canpay,
						sumMoney: that.data.sumMoney,
						//provinceName: "undefined",
						requestMark: false,
						getWay: 1
					})
				}
			})
	},




	//快递运费请求
	freightRequest: function (goodCarIdStr) {
		let that = this
		http.getAsync(
			addr.Address.getOrderGoodsBuyPriceByCarIds,
			{
				appid: app.globalData.appid,
				openid: app.globalData.userInfo.openId,
				goodCarIdStr: goodCarIdStr
			}
		)
			.then(function (data) {
				if (data.isok == 1) {
					var array = []
					that.data.chooseModel = data.postdata
					for (let i = 0; i < data.postdata.length; i++) {
						var model = data.postdata[i]
						array.push(model.Name)
					}
					that.setData({
						array: array,
						chooseModel: data.postdata,//显示运费
					})
				}
			})
	},
	//默认地址请求
	// AddressRequest: function () {
	// 	var that = this
	// 	http.postAsync(
	// 		addr.Address.GetUserWxAddress,
	// 		{
	// 			appId: app.globalData.appid,
	// 			UserId: app.globalData.userInfo.UserId
	// 		}).then(function (data) {
	// 			if (data.isok == true) {
	// 				var Address = "";
	// 				if (typeof data.obj.WxAddress.WxAddress == "string" && data.obj.WxAddress.WxAddress != "") {
	// 					Address = JSON.parse(data.obj.WxAddress.WxAddress)
	// 				}

	// 				that.setData({
	// 					Address: Address
	// 				})
	// 			}
	// 		})
	// },

	// 选择配送
	bindPickerFunc: function (e) {
		var totalPrice = (parseFloat(this.data.discountTotal) + parseFloat(this.data.chooseModel[e.detail.value].sum)).toFixed(2)
		this.setData({
			index: e.detail.value,
			totalPrice: totalPrice,//运费+折扣总价
		})
		this.calMoney();
	},
	// 选择支付方式
	changePayFunc: function (e) {
		this.data.paymode = e.detail.value
		this.setData({ index1: e.detail.value, })
	},
	// 支付方式选择
	payFunc: function (e) {
		var that = this
		if (that.data.canpay == false && that.data.getWay != 0) {
			wx.showModal({
				title: '提示',
				content: '抱歉，您的地址超出配送范围，无法下单',
			})
			return;
		}
		var formId = e.detail.formId
		util.commitFormId(formId, that)
		if (that.data.Address.storeaddress != undefined && that.data.getWay == 0 || that.data.Address.detailInfo != undefined && that.data.getWay == 1) {
			if (that.data.paymode == 0) {
				that.payRequest(1)
			} else {
				wx.showModal({
					title: '确认使用储值支付吗？',
					showCancel: true,
					success: function (res) {
						if (res.confirm) {
							that.payRequest(2)
						}
					},
				})
			}
		} else {
			wx.showToast({
				title: "请选择地址",
				icon: 'loading'
			})
		}
	},
	// 选择我的地址页
	addressGoto: function () {
		let that = this
		if (that.data.storeInfo.funJoinModel.openInvite) {
			wx.showActionSheet({
				itemList: ['到店自取', '商家配送'],
				success: function (res) {
					if (res.tapIndex == 0) {
						wx.navigateTo({ url: '/pages/orderList/setgetgoodInfo' })
						// that.data.sumMoney = Number(that.data.sumMoney) - Number(that.data.fee)
						that.setData({
							sendWay: '到店自取',
							getWay: 0,
							// sumMoney: (that.data.sumMoney).toFixed(2)
						})
					}
					if (res.tapIndex == 1) {
						// wx.chooseAddress({
						// 	success: function (res) {
						// 		that.setData({ Address: res, showModalUser: false })
						// 	},
						// 	fail: function (res) {
						// 		console.log(res)

						// 		wx.getSetting({
						// 			success: (res) => {
						// 				if (res.authSetting["scope.address"]) {
						// 					that.setData({ showModalUser: false })
						// 				}
						// 				else {
						// 					that.setData({ showModalUser: true })
						// 				}
						// 			}
						// 		})
						// 	}
						// })
						that.setData({
							sendWay: '商家配送'
						})

						tools.goNewPage("../me/address")
					}
					that.setData({ getWay: res.tapIndex })
				},
				fail: function (res) {
					console.log('客户取消了选择配送方式')
				}
			})
		} else {
			that.setData({
				sendWay: '商家配送',
			})
			tools.goNewPage("../me/address")
			// wx.chooseAddress({
			// 	// success: function (res0) {
			// 	//   that.setData({ Address: res0, getWay: 1 })
			// 	// }
			// 	success: function (res) {
			// 		that.setData({ Address: res, showModalUser: false, getWay: 1 })
			// 	},
			// 	fail: function (res) {
			// 		console.log(res)

			// 		wx.getSetting({
			// 			success: (res) => {
			// 				if (res.authSetting["scope.address"]) {
			// 					that.setData({ showModalUser: false })
			// 				}
			// 				else {
			// 					that.setData({ showModalUser: true })
			// 				}
			// 			}
			// 		})
			// 	}
			// })
		}
	},
	userFunc: function (e) {
		let id = Number(e.target.id)
		switch (id) {
			case 0:
				this.setData({ showModalUser: false });
				break;
			case 1:
				wx.openSetting({})
				break;
		}
	},
	inputMessage: function (e) {
		let messIput = e.detail.value
		this.data.messIput = messIput
	},
	// 支付请求
	payRequest: function (buyMode) {
		let that = this
		if (app.globalData.getWayinfo.name == '' && that.data.getWay == 0 || app.globalData.getWayinfo.phone == '' && that.data.getWay == 0) {
			wx.showModal({
				title: '提示',
				content: '到店自取请填写完善基本信息！',
				showCancel: false,
				success: function (res) {
					if (res.confirm) {
						wx.navigateTo({ url: '/pages/orderList/setgetgoodInfo' })
					}
				}
			})
			return
		}
		let Address = that.data.Address
		if (that.data.getWay == 0) {
			var addressInfo = {
				userName: app.globalData.getWayinfo.name,
				telNumber: app.globalData.getWayinfo.phone,
				postalCode: 0,
				detailInfo: Address.storeaddress
			}
		} else {
			var addressInfo = {
				userName: Address.userName,
				postalCode: Address.postalCode,
				provinceName: Address.provinceName,
				cityName: Address.cityName,
				countyName: Address.countyName,
				detailInfo: Address.detailInfo,
				telNumber: Address.telNumber,
			}
		}
		// if (that.data.getWay != 0) {
		// 	var FreightTemplateId = that.data.chooseModel[that.data.index].Id
		// } else {
		// 	var FreightTemplateId = 0
		// }
		let [wxaddressjson, order, orderIsDistribution] = [
			addressInfo,
			{
				// FreightTemplateId: FreightTemplateId,
				FreightTemplateId: 0,
				AccepterName: Address.userName,
				AccepterTelePhone: Address.telNumber,
				Message: that.data.messIput
			},
			{
				FromAppId: app.globalData.FromAppId,
				DistributionMoney: app.globalData.DistributionMoney,
				FreightTemplateId: 0,
				AccepterName: Address.userName,
				AccepterTelePhone: Address.telNumber,
				Message: that.data.messIput
			}
		]
		let wxaddressjsonP = JSON.stringify(wxaddressjson)
		let orderjsonP = JSON.stringify(order)
		let orderjsonI = JSON.stringify(orderIsDistribution)
		if (app.globalData.IsDistribution == true && app.globalData.FromAppId != undefined) {
			var orderjson = orderjsonI
		} else {
			var orderjson = orderjsonP
		}
		let coupon = this.data.pickCoupon;
		var pages = getCurrentPages();
		var options = pages[pages.length - 1].options;
		http
			.postAsync(addr.Address.addMiniappGoodsOrder, {
				appId: app.globalData.appid,
				openid: app.globalData.userInfo.openId,
				goodCarIdStr: that.data.goodCarIdStr,
				wxaddressjson: wxaddressjsonP,
				orderjson: orderjson,
				buyMode: buyMode,
				couponlogid: coupon == null ? 0 : coupon.Id,
				getWay: that.data.getWay,
				aid: wx.getStorageSync('aid'),
				//#region 新版拼团下单传递参数
				isgroup: that.data.isgroup,
				groupid: that.data.GId || 0,
				goodtype: options.goodtype || 0,
				//#end
				// 扫码购物
				storecodeid: getApp().globalData.storecodeid
				// #end
			})
			.then(function (data) {
				if (data.isok == 1) {
					app.globalData.reductionCart = data.reductionCart
					app.globalData.dbOrder = data.dbOrder

					//允许支付金额为0的情况
					if (('money') in data && data.money === 0) {
						wx.showToast({
							title: '支付成功',
							duration: 1000,
							icon: "loading"
						})
						setTimeout(function () {
							// tools.goNewPage('../orderDetail/orderDetail?orderId=' + data.dbOrder + "&ispay=1", )
							if (options.joinGroup == 1) { //新版拼团跳转
								tools.goNewPage('../group2/orderSuccess?gOrderid=' + app.globalData.dbOrder + '&isgroup=' + that.data.isgroup)
							} else { //普通订单流程
								tools.goNewPage('../orderDetail/orderDetail?orderId=' + data.dbOrder + "&ispay=1")
							}
						}, 1000)
						return;
					}

					// 支付方式判断 根据buyMode 1为微信支付
					if (buyMode == 1) {
						that.wxPayFunc(data.orderid)
					} else {
						// 2为储值支付
						wx.showToast({
							title: '支付成功',
							duration: 1000,
							icon: "loading"
						})
						setTimeout(function () {
							if (data.reductionCart == null) {
								// tools.goNewPage('../orderDetail/orderDetail?orderId=' + data.dbOrder)
								if (options.joinGroup == 1) { //新版拼团跳转
									tools.goNewPage('../group2/orderSuccess?gOrderid=' + app.globalData.dbOrder + '&isgroup=' + that.data.isgroup)
								} else { //普通订单流程
									tools.goNewPage('../orderDetail/orderDetail?orderId=' + data.dbOrder)
								}
							} else {
								if (that.data.pickCoupon == null) {
									// var paymoney = that.data.totalPrice
									var paymoney = that.data.sumMoney
								} else {
									var paymoney = that.data.money_cal_fmt
								}
								// tools.goNewPage('../paysuccess/smoneypaysuccess?orderId=' + data.dbOrder + '&paymoney=' + paymoney)
								if (options.joinGroup == 1) { //新版拼团跳转
									tools.goNewPage('../group2/orderSuccess?gOrderid=' + app.globalData.dbOrder + '&isgroup=' + that.data.isgroup)
								} else { //普通订单流程
									tools.goNewPage('../paysuccess/smoneypaysuccess?orderId=' + data.dbOrder + '&paymoney=' + paymoney)
								}
							}
						}, 1000)
					}
				}
				else {
					wx.showModal({
						title: '提示',
						content: data.msg,
						showCancel: false,
					})
				}
			})
	},
	// 调用微信支付
	wxPayFunc: function (oradidp) {
		let that = this
		let oradid = oradidp
		var pages = getCurrentPages();
		var options = pages[pages.length - 1].options;
		let newparam = {
			openId: app.globalData.userInfo.openId,
			orderid: oradid,
			'type': 1,
		}
		util.PayOrder(newparam, {
			failed: function () {
				wx.showToast({
					title: '您取消了支付',
					duration: 1000,
					icon: "loading"
				})
				setTimeout(function () {
					// tools.goNewPage('../orderDetail/orderDetail?orderId=' + app.globalData.dbOrder)
					if (options.joinGroup == 1) { //新版拼团跳转
						tools.goNewPage('../group2/orderSuccess?gOrderid=' + app.globalData.dbOrder + '&isgroup=' + that.data.isgroup)
					} else { //普通订单流程
						tools.goNewPage('../orderDetail/orderDetail?orderId=' + app.globalData.dbOrder)
					}
				}, 1000)
			},
			success: function (res) {
				if (res == "wxpay") {
				} else if (res == "success") {
					wx.showToast({
						title: '支付成功',
						duration: 1000,
						icon: "loading"
					})
					setTimeout(function () {
						// tools.goNewPage('../orderDetail/orderDetail?orderId=' + app.globalData.dbOrder + "&ispay=1")
						if (options.joinGroup == 1) { //新版拼团跳转
							tools.goNewPage('../group2/orderSuccess?gOrderid=' + app.globalData.dbOrder + '&isgroup=' + that.data.isgroup)
						} else { //普通订单流程
							tools.goNewPage('../orderDetail/orderDetail?orderId=' + app.globalData.dbOrder + "&ispay=1")
						}
					}, 1000)
				}
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
		var goodsid = "";
		if (this.data.goods) {
			goodsid = this.data.goods.map(p => p.goodid).join(',');
		}

		tools.GetMyCouponList({
			appId: app.globalData.appid,
			userId: app.globalData.userInfo.UserId,
			pageIndex: vm.pageindex,
			state: vm.state,
			goodsId: goodsid,
			goodsInfo: JSON.stringify(that.data.goods.map(function (item, index) {
				return {
					goodid: item.goodid,
					totalprice: Number(item.discountPrice) * item.Count * 100
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
	chooseCoupons: function () {
		this.setData({
			couponsShow: !this.data.couponsShow
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
					totalPrice += (Number(curValue.discountPrice) || 0).mul(Number(curValue.Count))
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
	pickCouponOK: function () {
		this.calMoney();
		this.setData({
			"couponsShow": false,
		});
	},
	calMoney: function () {
		var money = (Number(this.data.totalPrice) || 0) * 100;
		//运费
		//	var freight = (Number(this.data.chooseModel[this.data.index].sum) || 0) * 100;
		var freight = this.data.fee
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
							calMoney = (money).sub(coupon.Money);
							money_coupon = (coupon.Money).div(100);
						}
						//折扣 * 折扣
						else if (coupon.CouponWay == 1) {
							var p = (coupon.Money).div(100).mul(10).div(100);
							calMoney = (money).mul(p);
							var coupon_p = (1).sub(p);
							money_coupon = (money).mul(coupon_p).div(100);
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
						sumMoney += (Number(item.discountPrice)).mul(100).mul(Number(item.Count));
					});


					//如果满足使用条件
					if (coupon.LimitMoney <= 0 || sumMoney >= coupon.LimitMoney) {
						coupon.Money = Number(coupon.Money);
						//指定金额 - 优惠
						if (coupon.CouponWay == 0) {
							money_coupon = coupon.Money//(sumMoney).sub(coupon.Money);
						}
						//折扣 * 折扣
						else if (coupon.CouponWay == 1) {
							//重新计算价格


							var p = (coupon.Money).div(100).mul(10).div(100);
							var coupon_p = (1).sub(p);
							money_coupon = (sumMoney).mul(coupon_p)// (sumMoney).sub((sumMoney).mul(coupon_p));
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
						calMoney = (money).sub(money_coupon);

						if (calMoney < 0) {
							calMoney = 0;
						}


						//优惠券减掉的钱
						money_coupon = (money_coupon).div(100);
					}
				}

			}

		}
		//再把运费加回来
		calMoney = (calMoney).add(freight);

		var money_cal_fmt = (calMoney).div(100).toFixed(2);
		this.setData({
			//totalPrice: money_cal_fmt,
			money_cal: calMoney,
			money_cal_fmt: money_cal_fmt,
			money_coupon: money_coupon.toFixed(2),
		});
	},
	notuseCoupon: function () {

		this.setData({
			"pickCoupon": null,
			"couponsShow": false,
		});
	},
	reachCouponBottom: function () {
		var vm = this.data.vmMycoupon;
		if (!vm.ispost && !vm.loadall) {
			this.loadMyCoupon();
		}
	}

})