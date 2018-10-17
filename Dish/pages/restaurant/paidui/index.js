const app = getApp();
const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
Page({
	data: {
		this_user_info: {},
		submitIsLoading: false,
		buttonIsDisabled: false,
		this_dish_id: 0,
		this_q_list_data: '',
		this_q_list_info: null
	},
	onLoad: function (options) {
		var that = this;
		var dish_id = options.dish_id;
		that.setData({ this_dish_id: dish_id });
	},
	onShow: function () {
		var that = this;
		requestUtil.post(_DuoguanData.getQueueList, { dish_id: that.data.this_dish_id }, (info) => {
			that.setData({ this_q_list_data: info.qlist, this_q_list_info: info.qinfo, glo_is_load: false });
		}, that, {});
	},
	formSubmit: function (e) {
		var that = this;
		var requestData = e.detail.value;
		if (requestData.pd_phone.length != 11) {
			wx.showToast({
				title: '手机号有误',
				icon: 'none'
			})
			return
		}
		that.setData({ submitIsLoading: true, buttonIsDisabled: true });
		var form_id = e.detail.formId;
		that.insertFormID(form_id);
		requestData.dish_id = that.data.this_dish_id;
		requestUtil.post(_DuoguanData.actionQueue, requestData, (info) => {
			that.onShow();
		}, that, { completeAfter: function () { that.setData({ submitIsLoading: false, buttonIsDisabled: false }); } });
	},
	insertFormID: function (form_id) {
		var that = this;
		requestUtil.post(_DuoguanData.commitFormId, { formid: form_id, appid: getApp().globalData.appid, openid: wx.getStorageSync('userInfo').OpenId }, (info) => {

		}, that, { isShowLoading: false });
	},
	action_formSubmit: function (e) {
		var that = this;
		var form_id = e.detail.formId;
		var q_id = that.data.this_q_list_info.id;
		that.insertFormID(form_id);
		var this_action_type = e.detail.target.dataset.type;
		if (this_action_type == 'shuaxin') {
			that.onShow();
		} else if (this_action_type == 'quxiao') {
			wx.showModal({
				title: '提示',
				content: "确认要取消排队吗?",
				success: function (res) {
					if (res.confirm == true) {
						requestUtil.post(_DuoguanData.qxQueueInfo, { q_id: q_id }, (info) => {
							that.onShow();
						}, that, {});
					}
				}
			})
		}
	}
})