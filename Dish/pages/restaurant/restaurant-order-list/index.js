var app = getApp()
const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
Page({
	data: {
		postlist: [],
		this_weiba_id: 0,
		hasMore: false,
		showLoading: false,
		isScrollY: true,
		this_page: 1,//当前页码
		pagesize: 10,//每页数量
		this_nav_name: 'index',
		this_is_jinghua: 0,
		this_finish_page: 0,
		glo_is_load: true,
		this_group_val: -1
	},
	//订单详情
	user_orderinfo_bind: function (e) {
		var oid = e.currentTarget.id;
		wx.setStorageSync('dish_id', e.currentTarget.dataset.dish_id)
		wx.navigateTo({
			url: '../restaurant-order-info/index?oid=' + oid
		});
	},
	//订单状态切换
	select_status_show: function (e) {
		this.setData({ this_page: 1, this_group_val: e.currentTarget.dataset.val });
		this.onShow();
	},
	//评价
	order_go_comment_bind: function (e) {
		wx.setStorageSync('dish_id', e.currentTarget.dataset.dish_id)
		var oid = e.currentTarget.id;
		wx.navigateTo({
			url: '../restaurant-order-comment/index?oid=' + oid
		});
	},
	//删除订单
	delete_user_order: function (e) {
		var that = this
		var oid = e.currentTarget.id;
		wx.showModal({
			title: '提示',
			content: "确认要删除该订单吗?",
			success: function (res) {
				if (res.confirm == true) {
					requestUtil.post(_DuoguanData.DeleteUserOrder, { oid: oid }, (info) => {
						that.onShow();
					}, that, {});
				}
			}
		})
	},

	onShow: function () {
		var that = this;
		if (getCurrentPages().length == 2) {
			wx.setStorageSync('dish_id', 0)
		}
		var requestData = {};
		requestData.pagesize = that.data.pagesize;
		requestData.pagenum = that.data.this_page;
		requestData.order_status = that.data.this_group_val;
		requestUtil.post(_DuoguanData.GetUserOrderList, requestData, (info) => {

			if (info == null) {
				that.setData({ showLoading: false });
			} else {
				if (info.info.length >= that.data.pagesize) {
					that.setData({ showLoading: true });
				} else {
					that.setData({ showLoading: false });
				}
			}
			for (let i = 0; i < info.info.length; i++) {
				info.info[i].add_time = requestUtil.ChangeDateFormat(info.info[i].add_time)
				info.info[i].confirm_time = requestUtil.ChangeDateFormat(info.info[i].confirm_time)
				info.info[i].ctime = requestUtil.ChangeDateFormat(info.info[i].ctime)
				info.info[i].pay_end_time = requestUtil.ChangeDateFormat(info.info[i].pay_end_time)
				info.info[i].pay_time = requestUtil.ChangeDateFormat(info.info[i].pay_time)
				info.info[i].shipping_time = requestUtil.ChangeDateFormat(info.info[i].shipping_time)
			}
			that.setData({ postlist: info.info, glo_is_load: false });
			wx.hideToast();
		}, this, {});
	},
	//下拉刷新
	onPullDownRefresh: function () {
		var that = this;
		that.setData({
			this_page: 1,
			this_group_val: -1
		});
		that.onShow();
		setTimeout(() => {
			wx.stopPullDownRefresh();
		}, 1000)
	},
	onReachBottom: function (e) {
		var that = this;
		if (that.data.showLoading == true) {
			that.setData({ this_page: that.data.this_page + 1 });
		} else {
			return false;
		}
		var requestData = {};
		requestData.pagesize = that.data.pagesize;
		requestData.pagenum = that.data.this_page;
		requestData.order_status = that.data.this_group_val;
		requestUtil.post(_DuoguanData.GetUserOrderList, requestData, (info) => {
			if (info == null) {
				that.setData({ showLoading: false });
			} else {
				if (info.info.length >= that.data.pagesize) {
					that.setData({ showLoading: true });
				} else {
					that.setData({ showLoading: false });
				}
				for (let i = 0; i < info.info.length; i++) {
					info.info[i].add_time = requestUtil.ChangeDateFormat(info.info[i].add_time)
					info.info[i].confirm_time = requestUtil.ChangeDateFormat(info.info[i].confirm_time)
					info.info[i].ctime = requestUtil.ChangeDateFormat(info.info[i].ctime)
					info.info[i].pay_end_time = requestUtil.ChangeDateFormat(info.info[i].pay_end_time)
					info.info[i].pay_time = requestUtil.ChangeDateFormat(info.info[i].pay_time)
					info.info[i].shipping_time = requestUtil.ChangeDateFormat(info.info[i].shipping_time)
				}
				if (info.info.length > 0) {
					that.setData({
						postlist: that.data.postlist.concat(info.info)
					});
				}
			}
			that.setData({ glo_is_load: false });
			wx.hideToast();
		}, this, {});
	},
	//开始支付
	order_go_pay_bind: function (e) {
		var that = this;

	}
})