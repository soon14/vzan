var http = require("../../utils/http.js");
var addr = require("../../utils/addr.js");
var tools = require("../../utils/tools.js");
var util = require("../../utils/util.js");
var app = getApp();

Page({

  /**
   * 页面的初始数据
   */
	data: {
		payTypeRange: [
			{ name: "微信支付", value: 0 },
			{ name: "储值支付", value: 1 }
		],
		payType: 0,
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

			http.postAsync(addr.Address.GetUserWxAddress, { appId: app.globalData.appid, UserId: user.uid }).then(function (useraddr) {
				if (useraddr.isok) {
					if (typeof useraddr.obj.WxAddress.WxAddress == "string" && useraddr.obj.WxAddress.WxAddress.length > 0) {
						var res = JSON.parse(useraddr.obj.WxAddress.WxAddress);
						that.setData({
							Address: res.provinceName + res.cityName + res.countyName + res.detailInfo,
							telephone: res.telNumber,
							username: res.userName,
						});
					}
				}
				console.log(useraddr);
			});
		});
		this.initOrderInfo(options);
		//判断储值开关
    if (app.globalData.storeConfig.funJoinModel.canSaveMoneyFunction) {
			that.data.payTypeRange = that.data.payTypeRange
		} else {
			that.data.payTypeRange = that.data.payTypeRange.splice(0, 1)
		}
		that.setData({ payTypeRange: that.data.payTypeRange })
    util.setPageSkin(that);
	},
	pickPayType: function (e) {
		this.setData({
			payType: e.detail.value
		})
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

	//初始化订单
	initOrderInfo: function (options) {
		var that = this;

		var _gsid = parseInt(options.gsid) || 0;
		var _groupid = parseInt(options.groupid) || 0;
		var _isGroup = parseInt(options.isGroup) || 0;
		var _isGHead = parseInt(options.isGHead) || 0;

		that.getGroupDetail(_groupid).then(function (data) {
			if (data.isok) {
				var _g = data.groupdetail
				var price = 0;
				if (_isGroup) {
					price = _g.DiscountPrice;
				}
				else {
					price = _g.UnitPrice;
				}


				//保存
				that.setData({
					gsid: _gsid,
					num: 1,
					isGroup: _isGroup, //是否团购
					isGHead: _isGHead,//是否团长
					groupId: _groupid,//
					GroupName: _g.GroupName,//拼团标题
					DiscountPrice: _g.DiscountPrice,//拼团价格
					LimitNum: _g.LimitNum,//限制用户购买数量
					payprice: price,
					UnitPrice: _g.UnitPrice,
					ImgUrl: _g.ImgUrl,
					HeadDeduct: _g.HeadDeduct || 0,
					//shouldPay: _shouldPay
				});
				that.computedShouldPay();
			}
			else {
				tools.alert("提示", data.msg);
			}
			console.log(data);
		});

	},
	//获取团购详情
	getGroupDetail: function (groupid) {
		return http.postAsync(addr.Address.GetGroupDetail, { appId: app.globalData.appid, groupId: groupid });
	},
	//加1
	plusOne: function () {
		var _d = this.data;
		if (_d.num > 999) {
			tools.alert("提示", "购买数量不能大于999");
			return;
		}
		_d.num += 1;
		this.setData({
			"num": _d.num
		});
		this.computedShouldPay();
	},
	//减1
	reduOne: function () {
		var _d = this.data;
		if (_d.num <= 1) {
			tools.alert("提示", "购买数量不能小于1");
			return;
		}
		_d.num -= 1;
		this.setData({
			"num": _d.num
		});
		this.computedShouldPay();
	},
	computedShouldPay: function () {
		var _d = this.data;

		var _shouldPay = 0;
		if (_d.HeadDeduct > 0 && _d.isGHead == 1) {
			_shouldPay = parseFloat(((_d.payprice * _d.num) - _d.HeadDeduct) / 100).toFixed(2);
		}
		else {
			_shouldPay = parseFloat((_d.payprice * _d.num) / 100).toFixed(2);
		}

		this.setData({
			shouldPay: _shouldPay
		});
	},
	//去支付
	groupPay: function (e) {
		var that = this;
		// 提交备用formId
		var formId = e.detail.formId
		util.commitFormId(formId, that)
		var _d = that.data;
		tools.getUserInfo().then(function (user) {
			console.log(user);
			//构造下单参数
			var Jsondata = {
				appId: app.globalData.appid,
				groupId: _d.groupId,
				UserId: user.UserId,
				num: _d.num,
				isGroup: _d.isGroup,//是否拼团
				isGHead: _d.isGHead,
				gsid: _d.gsid,
				//收货人信息
				phone: _d.telephone,
				username: _d.username,
				addres: _d.Address,
				note: _d.inputNoteValue || "",
			};
			//一、提交信息生成拼团单
			console.log(Jsondata);
			that
				.buildGroupOrder(Jsondata)
				.then(function (order) {
					console.log("【buildGroupOrder】", order);
					if (order.isok) {

					}
					else {
						if (order.msg == "") {
							tools.alert("提示", order.postdata.msg);
						}
						else {
							tools.alert("提示", order.msg);
						}
						return;
					}

					var newjson = {
						openid: user.openId,
						userId: user.UserId,
						appId: app.globalData.appid,
						jsondata: JSON.stringify(order.postdata),
						ordertype: 3001005,
						paytype: _d.payType,
					}

					//储值支付
					if (_d.payType == 1) {
						wx.showModal({
							title: '提示',
							content: '确定使用储值进行支付吗？',
							success: function (res) {
								//储值支付需要用户点确认
								if (res.confirm) {
									util.AddOrderNew(newjson, function (reparam, oradid) {
										if (reparam == "success") {
											var _parameter = '?orderid=' + oradid + '&gsid=' + order.postdata.gsid + "&paytype=" + _d.payType;
											wx.redirectTo({
												url: '/pages/groupOrder/orderSuccess' + _parameter
											})
										}
									});
								} else if (res.cancel) {
                  http.postAsync(addr.Address.CancelPay, { guid: order.postdata.guid, appId: app.globalData.appid })
                    .then(function (_data) {
                      console.log("【微信支付失败】", _data);
                    });
								}
							}
						})
					}
					//微信支付
					else {
						util.AddOrderNew(newjson, function (reparam, oradid) {
							if (reparam == "success") {
								var _parameter = '?orderid=' + oradid + '&gsid=' + order.postdata.gsid + "&paytype=" + _d.payType;
								wx.redirectTo({
									url: '/pages/groupOrder/orderSuccess' + _parameter
								})
							}
							else {
                
								//order.guid  CancelPay
								http.postAsync(addr.Address.CancelPay, { guid: order.postdata.guid, appId: app.globalData.appid })
									.then(function (_data) {
										console.log("【微信支付失败】", _data);
									});
							}
						});
					}

				});
		});

	},
	//创建拼团单
	buildGroupOrder: function (_Jsondata) {
		return http.postAsync(addr.Address.AddGroup, { Jsondata: JSON.stringify(_Jsondata) });
	},
	//选择地址
	pickAddress: function () {
		var that = this;
		wx.chooseAddress({
			success: function (res) {
				console.log(res);
				that.setData({
					Address: res.provinceName + res.cityName + res.countyName + res.detailInfo,
					telephone: res.telNumber,
					username: res.userName,
				});
				tools.updateUserAddress(that.data.vm.uinfo.UserId, JSON.stringify(res));
			}
		})
	},
	inputNote: function (e) {
		this.setData({
			inputNoteValue: e.detail.value
		})
	}
})