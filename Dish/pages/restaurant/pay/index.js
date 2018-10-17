const app = getApp();
const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
Page({
	data: {
		this_user_info: {},
		submitIsLoading: false,
		buttonIsDisabled: false,
		this_dish_id: 0,
		this_dish_info: [],
		is_beizhu_show: false
	},
	change_beizhu_show: function () {
		this.setData({ is_beizhu_show: this.data.is_beizhu_show ? false : true });
	},
	onLoad: function (options) {
		var that = this;
		console.log('收款二维码参数', options)
		wx.setStorageSync('dish_id', options.dish_id || 0)
		that.setData({ this_dish_id: options.dish_id });
	},
	onShow: function () {
		var that = this;
		setInterval(() => {
			if (that.data.this_dish_info.length == 0) {
				requestUtil.post(_DuoguanData.getDishInfo, {}, (info) => {
					that.setData({ this_dish_info: info, glo_is_load: false });
				}, that, {});
			}
		}, 500)
	},
	formSubmit: function (e) {
		var that = this;
		if (Number(e.detail.value.money) <= 0) {
			wx.showToast({
				title: '请填写金额',
				icon: 'loading'
			})
			return
		}
		that.setData({ submitIsLoading: true, buttonIsDisabled: true });
		let pay_type = e.detail.value.pay_name;
		var rdata = e.detail.value;
		rdata.beizhu = rdata.beizhu ? rdata.beizhu : ''
		rdata.dish_id = that.data.this_dish_id;
		if (pay_type == 4) {
			wx.showModal({
				title: '提示',
				content: '是否确认使用余额支付?',
				success: function (res) {
					if (res.confirm) {
						that.paymoney(rdata)
					}
				},
			})
		} else {
			that.paymoney(rdata)
		}
	},
	paymoney: function (rdata) {
		var that = this
		requestUtil.post(_DuoguanData.PayOrder, rdata, (info) => {
		}, this, {
				isShowLoading: true, error: function (code, msg) {
					if (code == 9) {
						wx.showModal({
							title: '提示',
							content: msg,
							showCancel: false,
							success: function (res) {
								wx.navigateBack({
									delta: 1
								});
							}
						});
						return false;
					}
				}, completeAfter: function () { that.setData({ submitIsLoading: false, buttonIsDisabled: false }); }
			});
	}
})