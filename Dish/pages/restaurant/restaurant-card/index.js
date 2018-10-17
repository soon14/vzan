const app = getApp();
const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
const WxParse = require('../../../wxParse/wxParse.js');
Page({
	data: {
		this_user_info: {},
		submitIsLoading: false,
		buttonIsDisabled: false,
		this_dish_id: 0,
		this_dish_info: [],
		this_user_card_info: null,
		this_is_recharge: false,
		this_rc_config: null,
		this_rc_ohers_active: false,
		this_rc_jiner: 0,
		this_input_jiner: ''
	},
	//日志
	go_card_log_bind: function () {
		var that = this;
		wx.navigateTo({
			url: '../restaurant-card-log/index?dish_id=' + that.data.this_dish_id
		});
	},
	onLoad: function (options) {
		var that = this;
		var dish_id = options.dish_id;
		that.setData({ this_dish_id: dish_id });
		if (options.savemoney == 1) {
			that.user_recharge_view_bind()
		}
	},
	onShow: function () {
		var that = this;
		that.setData({ this_rc_ohers_active: false, this_rc_jiner: 0, this_input_jiner: 0 });
		requestUtil.post(_DuoguanData.getDishInfo, { dish_id: that.data.this_dish_id }, (info) => {
			that.setData({ this_dish_info: info });
			WxParse.wxParse('g_description', 'html', info.dish_info.card_info, that, 5);
			that.get_user_card_info();
		}, that, { isShowLoading: false });
	},
	get_user_card_info: function () {
		var that = this;
		var requestData = {};
		requestData.module_name = 'DuoguanDish';
		requestData.shop_id = that.data.this_dish_id;
		requestUtil.post(_DuoguanData.getCardInfo, requestData, (info) => {
			if (info.is_new == 1) {
				wx.showModal({
					title: '提示',
					content: "会员卡领取成功",
					showCancel: false
				});
			}
			that.setData({ this_user_card_info: info });
		}, that, { isShowLoading: true });
	},
	//充值层
	user_recharge_view_bind: function () {
		//获取充值配置
		var that = this;
		var requestData = {};
		requestData.module_name = 'DuoguanDish';
		requestData.dish_id = that.data.this_dish_id;
		requestUtil.post(_DuoguanData.getRechargeConfig, requestData, (info) => {
			for (let i = 0; i < info.length; i++) {
				info[i].rc_man = Number(info[i].rc_man).toFixed(2)
				info[i].rc_song = Number(info[i].rc_song).toFixed(2)
			}
			that.setData({ this_rc_config: info, this_is_recharge: that.data.this_is_recharge ? false : true });
		}, that, { isShowLoading: true });
	},
	//关闭充值层
	user_recharge_view_close_bind: function () {
		var that = this;
		that.setData({ this_is_recharge: that.data.this_is_recharge ? false : true, this_rc_ohers_active: false, this_rc_jiner: 0, this_input_jiner: 0 });
	},
	//选择充值金额
	select_rc_bind: function (e) {
		var that = this;
		let rc_config = that.data.this_rc_config;
		for (var i = 0; i < rc_config.length; i++) {
			rc_config[i].is_select = false;
		}
		rc_config[e.currentTarget.id].is_select = true;
		that.setData({ this_rc_config: rc_config, this_rc_ohers_active: false, this_rc_jiner: rc_config[e.currentTarget.id].rc_man, this_input_jiner: 0 });
	},
	//充值其它金额
	select_rc_others_bind: function () {
		var that = this;
		let rc_config = that.data.this_rc_config;
		for (var i = 0; i < rc_config.length; i++) {
			rc_config[i].is_select = false;
		}
		that.setData({ this_rc_config: rc_config, this_rc_ohers_active: true, this_rc_jiner: 0 });
	},
	get_rc_jiner: function (e) {
		var that = this;
		// let rz_jiner = parseFloat(e.detail.value);
		// var reg = /(^[1-9]([0-9]+)?(\.[0-9]{1,2})?$)|(^(0){1}$)|(^[0-9]\.[0-9]([0-9])?$)/;
		// if (reg.test(rz_jiner)) {
		// 	if (rz_jiner < 0 || rz_jiner > 10000) {
		// 		wx.showModal({
		// 			title: '提示',
		// 			content: "单笔充值金额1-10000元",
		// 			showCancel: false
		// 		});
		// 		that.setData({ this_rc_jiner: 0, this_input_jiner: 0 });
		// 		return false;
		// 	}
		that.setData({ this_rc_jiner: e.detail.value, this_input_jiner: e.detail.value });
		// } else {
		// 	wx.showModal({
		// 		title: '提示',
		// 		content: "充值金额格式错误",
		// 		showCancel: false
		// 	});
		// 	that.setData({ this_rc_jiner: 0, this_input_jiner: 0 });
		// 	return false;
		// };
	},
	//确认充值
	go_recharge_bind: function () {
		var that = this;
		let cz_jiner = parseFloat(that.data.this_rc_jiner);
		if (cz_jiner <= 0) {
			wx.showModal({
				title: '提示',
				content: "请选择或输入充值金额",
				showCancel: false
			});
			return false;
		}
		that.setData({ submitIsLoading: true, buttonIsDisabled: true });
		var requestData = {};
		requestData.aid = wx.getStorageSync('aid')
		requestData.module_name = 'DuoguanDish';
		requestData.shop_id = that.data.this_dish_id;
		requestData.rz_account = cz_jiner;
		requestUtil.post(_DuoguanData.cardRecharge, requestData, (info) => {
			// wx.requestPayment({
			//     'timeStamp': info.timeStamp,
			//     'nonceStr': info.nonceStr,
			//     'package': info.package,
			//     'signType': 'MD5',
			//     'paySign': info.paySign,
			//     'success': function (res) {
			//         that.onShow();
			//     },
			//     'fail': function (res) {

			//     }
			// })
		}, that, {
				isShowLoading: true, completeAfter: function () {
					that.setData({ submitIsLoading: false, buttonIsDisabled: false })
				}
			});
	},
	//下拉刷新
	onPullDownRefresh: function () {
		var that = this;
		that.onShow();
		setTimeout(() => {
			wx.stopPullDownRefresh();
		}, 1000);
	},
});