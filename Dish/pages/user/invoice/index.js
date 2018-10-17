const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
var app = getApp();
Page({
	data: {
		dish_data: [],
		this_fapiao_data: null,
		this_dish_id: 0,
		this_show_type: 0,
		this_edit_type: 'add',
		this_fapiao_leixing: 1,
		this_fapiao_info: null
	},
	onLoad: function (op) {
		// this.setData({ this_dish_id: op.dish_id });
	},
	onShow: function () {
		var that = this;
		var rq_data = {};
		requestUtil.post(_DuoguanData.getFapiaoList, rq_data, (info) => {
			that.setData({ this_fapiao_data: info });
		}, this, {});

	},
	//新增发票
	fapiao_add_bind: function () {
		this.setData({ this_show_type: 1 });
	},
	//取消新增
	quxiao_add_bind: function () {
		this.setData({ this_show_type: 0, this_edit_type: 'add', this_fapiao_leixing: 1, this_fapiao_info: null });
	},
	//切换类型
	chang_fapiao_leixing_bind: function () {
		this.setData({ this_fapiao_leixing: this.data.this_fapiao_leixing == 1 ? 2 : 1 });
	},
	//更新发票
	fapiao_formSubmit: function (e) {
		var that = this;
		var q_data = e.detail.value;
		q_data.fapiao_leixing = that.data.this_fapiao_leixing;
		q_data.action_type = that.data.this_edit_type;
		if (that.data.this_fapiao_info != null) {
			q_data.id = that.data.this_fapiao_info.id;
		}
		requestUtil.post(_DuoguanData.addFapiao, q_data, (info) => {
			that.setData({ this_show_type: 0, this_edit_type: 'add', this_fapiao_leixing: 1, this_fapiao_info: null });
			that.onShow();
		}, this, {});
	},
	//编辑发票
	fapiao_edit_bind: function (e) {
		var that = this;
		var fapiao_id = e.target.id;
		requestUtil.post(_DuoguanData.getFapiaoInfo, { fapiao_id: fapiao_id }, (info) => {
			that.setData({ this_fapiao_info: info, this_fapiao_leixing: info.fapiao_leixing, this_edit_type: 'edit', this_show_type: 1 });
		}, this, {});
	},
	//删除发票
	delete_fapiao_bind: function () {
		var that = this;
		var fapiao_id = that.data.this_fapiao_info.id;
		requestUtil.post(_DuoguanData.deleteFapiao, { fapiao_id: fapiao_id }, (info) => {
			that.setData({ this_show_type: 0, this_edit_type: 'add', this_fapiao_leixing: 1, this_fapiao_info: null });
			that.onShow();
		}, this, {});
	},
	//选择发票
	select_fapiao_bind: function (e) {
		var fapiao_data = e.currentTarget.dataset;
		if (fapiao_data.id == 0) {
			wx.setStorageSync('dish_fapiao_select', null);
		} else {
			wx.setStorageSync('dish_fapiao_select', fapiao_data);
		}
		// wx.navigateBack({ delta: 1 });
	}
})