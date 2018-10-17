if (!Date.prototype.Format) {
	Date.prototype.Format = function (fmt) {
		var o = {
			"M+": this.getMonth() + 1,                 //月份 
			"d+": this.getDate(),                    //日 
			"H+": this.getHours(),                   //小时 
			"m+": this.getMinutes(),                 //分 
			"s+": this.getSeconds(),                 //秒 
			"q+": Math.floor((this.getMonth() + 3) / 3), //季度 
			"S": this.getMilliseconds()             //毫秒 
		};

		if (/(y +)/.test(fmt)) {
			fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
		}
		for (var k in o)
			if (new RegExp("(" + k + ")").test(fmt))
				fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
		return fmt;
	}
}


// 浮点数求和
function accAdd(arg1, arg2) {
	var r1, r2, m;
	try { r1 = arg1.toString().split(".")[1].length } catch (e) { r1 = 0 }
	try { r2 = arg2.toString().split(".")[1].length } catch (e) { r2 = 0 }
	m = Math.pow(10, Math.max(r1, r2))
	return (arg1 * m + arg2 * m) / m
}
Number.prototype.add = function (arg) {
	return accAdd(arg, this);
}
// 浮点数相减
function accSubtr(arg1, arg2) {
	var r1, r2, m, n;
	try { r1 = arg1.toString().split(".")[1].length } catch (e) { r1 = 0 }
	try { r2 = arg2.toString().split(".")[1].length } catch (e) { r2 = 0 }
	m = Math.pow(10, Math.max(r1, r2));
	//动态控制精度长度
	n = (r1 >= r2) ? r1 : r2;
	return Number(((arg1 * m - arg2 * m) / m).toFixed(n));
}
Number.prototype.sub = function (arg) {
	return accSubtr(this, arg);
}

// 浮点数相乘
function accMul(arg1, arg2) {
	var m = 0, s1 = arg1.toString(), s2 = arg2.toString();
	try { m += s1.split(".")[1].length } catch (e) { }
	try { m += s2.split(".")[1].length } catch (e) { }
	return Number(s1.replace(".", "")) * Number(s2.replace(".", "")) / Math.pow(10, m)
}
Number.prototype.mul = function (arg) {
	return accMul(arg, this);
}
// 浮点数相除
function accDiv(arg1, arg2) {
	var t1 = 0, t2 = 0, r1, r2;
	try { t1 = arg1.toString().split(".")[1].length } catch (e) { }
	try { t2 = arg2.toString().split(".")[1].length } catch (e) { }
	r1 = Number(arg1.toString().replace(".", ""))
	r2 = Number(arg2.toString().replace(".", ""))
	return (r1 / r2) * Math.pow(10, t2 - t1);
}
Number.prototype.div = function (arg) {
	return accDiv(this, arg);
}





//工具类
var addr = require("addr.js");
var http = require("http.js");


var isEndClock = null;

var tools = {

	// 重置条件
	reset: function (list) {
		Object.assign(list, { pageindex: 1, list: [], ispost: false, loadall: false, exttypes: "", pricesort: "" })
	},
	// 返回页面顶部
	pageTop: function (that) {
		wx.pageScrollTo({
			scrollTop: 0,
		})
	},
	// 工具类showtoast
	ShowMsg: function (msg) {
		wx.showModal({
			title: '提示',
			content: msg,
			showCancel: false,
		})
	},
	ShowMsgAndUrl: function (msg, url, mtype = 0) {
		var that = this
		wx.showModal({
			title: '提示',
			showCancel: false,
			content: msg,
			success: function (res) {
				if (res.confirm) {
					if (mtype == 0) {
						that.goNewPage(url)
					} else if (mtype == 1) {
						that.goBackPage(1)
					}
				}
			}
		})
	},
	showToast: function (msg) {
		wx.showToast({
			title: msg,
			duration: 1000,
		})
	},
	showLoadToast: function (msg) {
		wx.showToast({
			title: msg,
			duration: 1000,
			icon: "loading"
		})
	},
	//跳转新页面
	goNewPage: function (url) {
		if (getCurrentPages().length >= 5) {
			wx.redirectTo({
				url: url,
			})
		} else {
			wx.navigateTo({
				url: url,
			})
		}
	},
	//跳转选项卡页
	goBarPage: function (url) {
		wx.switchTab({
			url: url,
		})
	},
	//返回上几页
	goBackPage: function (delta) {
		wx.navigateBack({
			delta: delta
		})
	},
	//重启动
	goLaunch: function (url) {
		wx.reLaunch({
			url: url
		})
	},
	// 拨打电话
	phoneFunc: function (phoneNumber) {
		if (phoneNumber) {
			wx.makePhoneCall({
				phoneNumber: phoneNumber,
			})
		} else {
			tools.showToast("未设置电话")
		}
	},
	// 打开地图
	mapFunc: function (lat, lng) {
		wx.openLocation({
			latitude: lat,
			longitude: lng,
			scale: 28
		})
	},
	// 拼团使用工具
	//类型
	typeEnum: {
		"data": "[object Date]",
		"object": "[object Object]",
		"number": "[object Number]",
		"string": "[object String]",
		"boolean": "[object Boolean]"
	},
	//弹窗
	alert: function (title, content, successCallback, cancelCallback) {
		wx.showModal({
			title: title || '提示',
			content: content || "",
			success: function (res) {
				if (successCallback) {
					successCallback();
				}
				if (cancelCallback) {
					cancelCallback();
				}
			}
		})
	},
	tips: function (title) {
		wx.showToast({
			title: title,
			icon: 'success',
			duration: 1500
		})
	},
	getUserInfo: function () {
		var _user = getApp().globalData.userInfo;
		return new Promise(function (resolve, reject) {
			if (!_user.UserId) {
				getApp().getUserInfo(function (uinfo) {
					resolve(uinfo);
				})
			}
			else {
				resolve(_user);
			}
		})

	},
	updateUserAddress: function (_userid, _WxAddress) {
		http
			.postAsync(addr.Address.UpdateUserWxAddress,
			{
				UserId: _userid,
				WxAddress: _WxAddress
			});
	},
	getType: function (obj) {
		return Object.prototype.toString.call(obj)
	},
	getTimeSpan: function (time) {
		if (tools.getType(time) == tools.typeEnum.string) {
			time = time.replace(/-/g, "/");
		}
		time = new Date(time).getTime();
		var now = new Date().getTime();
		if (time - now <= 0)
			return 0;
		else
			return time - now
	},
	formatMillisecond: function (millisecond) {
		var days = Math.floor(millisecond / (1000 * 60 * 60 * 24));
		var hours = Math.floor((millisecond % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
		var minutes = Math.floor((millisecond % (1000 * 60 * 60)) / (1000 * 60));
		var seconds = Math.floor((millisecond % (1000 * 60)) / 1000);
		if (days.toString().length < 2)
			days = "0" + days;

		if (hours.toString().length < 2)
			hours = "0" + hours;

		if (minutes.toString().length < 2)
			minutes = "0" + minutes;

		if (seconds.toString().length < 2)
			seconds = "0" + seconds;

		return [days, hours, minutes, seconds];
	},
	initEndClock: function (begin, end, whereData) {
		var that = whereData;
		var _begin = begin.replace(/-/g, "/");
		var _end = end.replace(/-/g, "/");
		var now = new Date().getTime();
		var begin = new Date(_begin).getTime();
		var end = new Date(_end).getTime();

		//未开始
		if (begin - now > 0) {
			var _dd = 0, _hh = 0, _mm = 0, _ss = 0;
			var totalSecond = parseInt((begin - now) / 1000);
			var totalMinute = parseInt(totalSecond / 60);
			var totalHour = parseInt(totalMinute / 60);

			_dd = Math.floor(totalHour / 24);
			_hh = Math.floor(totalHour % 24);
			_mm = Math.floor(totalMinute % 60);
			_ss = Math.floor(totalSecond % 60);
			_dd = _dd < 10 ? "0" + _dd : _dd;
			_hh = _hh < 10 ? "0" + _hh : _hh;
			_mm = _mm < 10 ? "0" + _mm : _mm;
			_ss = _ss < 10 ? _ss = "0" + _ss : _ss;
			var _fromTheEnd = {
				dd: _dd,
				hh: _hh,
				mm: _mm,
				ss: _ss,
			}


			that.setData({
				fromTheEnd: _fromTheEnd,
				fromTheEnd_txt: "距离开始",
				groupstate: -1
			});
			isEndClock = setTimeout(function () {
				tools.initEndClock(_begin, _end, that);
			}, 1000);
		}
		//开始中
		else if (begin - now < 0 && end - now > 0) {
			var _dd = 0, _hh = 0, _mm = 0, _ss = 0;
			var totalSecond = parseInt((end - now) / 1000);
			var totalMinute = parseInt(totalSecond / 60);
			var totalHour = parseInt(totalMinute / 60);

			_dd = Math.floor(totalHour / 24);
			_hh = Math.floor(totalHour % 24);
			_mm = Math.floor(totalMinute % 60);
			_ss = Math.floor(totalSecond % 60);
			_dd = _dd < 10 ? "0" + _dd : _dd;
			_hh = _hh < 10 ? "0" + _hh : _hh;
			_mm = _mm < 10 ? "0" + _mm : _mm;
			_ss = _ss < 10 ? _ss = "0" + _ss : _ss;
			var _fromTheEnd = {
				dd: _dd,
				hh: _hh,
				mm: _mm,
				ss: _ss,
			}
			that.setData({
				fromTheEnd: _fromTheEnd,
				fromTheEnd_txt: "距离结束",
				groupstate: 1
			});
			isEndClock = setTimeout(function () {
				tools.initEndClock(_begin, _end, that);
			}, 1000);
		}
		//结束
		else if (now - end >= 0) {
			if (isEndClock != null) {
				clearTimeout(isEndClock);
				isEndClock = null;
			}
			that.setData({
				fromTheEnd_txt: "已结束",
				groupstate: 0
			});
		}
	},
	copyData: function (data) {
		var that = this;
		wx.setClipboardData({
			data: data,
			success: function (res) {
				that.tips("复制成功");
			}
		})
	},
	share: function (group) {
		var _g = group;
		var _path = '/pages/groupOrder/groupInvite?gsid=' + _g.GroupSponsorId;
		var _title = `￥${_g.DiscountPrice / 100}元就能购买${_g.GroupName},一起来拼团吧！`;
		console.log(_path);
		return {
			title: _title,
			path: _path,
			imageUrl: _g.ImgUrl,
			success: function (res) {
				// 转发成功
				tools.tips("转发成功");
			},
			fail: function (res) {

			}
		}
	},
	phoneCall: function () {
		if (getApp().globalData.TelePhone) {
			wx.makePhoneCall({
				phoneNumber: getApp().globalData.TelePhone,
			})
		}
		else {
			tools.tips("未设置电话");
		}
	},
  /*
  获取我的优惠券列表
  appId
  userId
  pageIndex
  state
  */
	GetMyCouponList: function (postData) {
		return http.postAsync(addr.Address.GetMyCouponList, postData);
	},
  /*
  获取店铺优惠券列表
  appId
  goodstype
  */
	GetStoreCouponList: function (postData) {
		return http.postAsync(addr.Address.GetStoreCouponList, postData);
	},
  /*
  领取优惠券
  appId
  couponId
  userId
  */
	GetCoupon: function (postData) {
		return http.postAsync(addr.Address.GetCoupon, postData);
	},
	GetUserAddress: function (postData) {
		return http.postAsync(addr.Address.GetUserAddress, postData);
	},
	EditUserAddress: function (postData) {
		return http.postAsync(addr.Address.EditUserAddress, postData);
	},
	changeUserAddressState: function (postData) {
		return http.postAsync(addr.Address.changeUserAddressState, postData);
	},
	DeleteUserAddress: function (postData) {
		return http.postAsync(addr.Address.DeleteUserAddress, postData)
	},
	GetDadaFreight: function (postData) {
		return http.postAsync(addr.Address.GetDadaFreight, postData);
	},
	/*
倒计时
  appId
  couponId
  userId
  */
	formatDuring: function (mss) {
		if (mss < 0) {
			return "00:00:00";
		} else {
			var days = parseInt(mss / (1000 * 60 * 60 * 24));
			var hours = parseInt((mss % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
			var minutes = parseInt((mss % (1000 * 60 * 60)) / (1000 * 60));
			var seconds = (mss % (1000 * 60)) / 1000;
			if (hours == 0) {
				return minutes + "分钟" + Math.round(seconds) + "秒";
			} else if (days == 0) {
				return hours + "小时" + minutes + "分钟" + Math.round(seconds) + "秒";
			} else {
				return days + "天" + hours + "小时" + minutes + "分钟" + Math.round(seconds) + "秒";
			}
		}
	},
	gochat: function () {
		getApp().GetStoreConfig(function (config) {
			if (config && config.funJoinModel) {
				if (config.kfInfo && config.funJoinModel.imSwitch) {
					var userid = config.kfInfo.uid;
					var nickname = (config.kfInfo.nickName || "").replace(/\s/gi, "");
					var headimg = config.kfInfo.headImgUrl;
					wx.navigateTo({
						url: '/pages/im/chat?userid=' + userid + "&nickname=" + nickname + "&headimg=" + headimg,
					})
				}
				else {
					wx.showModal({
						title: '提示',
						content: '商家已关闭在线客服',
					})
				}
			}
		});
	},
	// 取消预约 后台退款 重置预约信息
	resetappoint: function () {
		getApp().globalData.appoint_goodslist = []
		getApp().globalData.appoint_shopcartlist = []
		getApp().globalData.appoint_alldiscountprice = 0
		getApp().globalData.appoint_paynow = 3
		getApp().globalData.appoint_shopcartlength = 0
		getApp().globalData.appointMsg = {
			datatime: '选择预约日期',
			time: '选择预约时间',
			numsindex: 0,
			msg: '',
			name: '',
			phonenumber: '',
		}
		console.log('重置预约信息')
	}
};
module.exports = tools;