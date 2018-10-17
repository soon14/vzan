const app = getApp();
const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
Page({
	data: {
		this_user_info: {},
		this_dish_id: 0,
		this_account_list: null,
		this_account_type: 1,
		this_page_size: 1,
		this_page_num: 10,
		is_loadmore: true
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
	},
	onShow: function () {
		var that = this;
		var requestData = {};
		requestData.pageIndex = 1;
		requestData.pageSize = that.data.this_page_num;
		requestData.shop_id = that.data.this_dish_id;
		requestData.account_type = that.data.this_account_type;
		requestUtil.post(_DuoguanData.GetLogList, requestData, (info) => {
			if (info.list == null) {
				that.setData({ is_loadmore: false });
			} else {
				if (info.list.length < that.data.this_page_num) {
					that.setData({ is_loadmore: false });
				}
			}
			for (let i = 0; i < info.list.length; i++) {
				info.list[i].add_time = requestUtil.ChangeDateFormat(info.list[i].add_time)
			}
			that.setData({ this_account_list: info })
		}, that, { isShowLoading: true });
	},
	onReachBottom: function (e) {
		var that = this;
		if (that.data.is_loadmore == false) {
			return false;
		}
		var requestData = {};
		requestData.pageIndex = that.data.this_page_size + 1;
		requestData.pageSize = that.data.this_page_num;
		requestData.shop_id = that.data.this_dish_id;
		requestData.account_type = that.data.this_account_type;
		requestUtil.post(_DuoguanData.GetLogList, requestData, (info) => {
			if (info.list == null) {
				that.setData({ is_loadmore: false });
			} else {
				if (info.list.length < that.data.this_page_num) {
					that.setData({ is_loadmore: false });
				}
				for (let i = 0; i < info.list.length; i++) {
					info.list[i].add_time = requestUtil.ChangeDateFormat(info.list[i].add_time)
				}
				var this_new_dish_data = that.data.this_account_list;
				this_new_dish_data.list = this_new_dish_data.list.concat(info.list);
				that.setData({ this_account_list: this_new_dish_data, this_page_size: requestData.pagesize });
			}
		}, that, { isShowLoading: true });
	},
	select_account_type: function (e) {
		let tid = e.currentTarget.id;
		this.setData({ this_account_type: tid, this_page_size: 1, is_loadmore: true });
		this.onShow();
	}
});