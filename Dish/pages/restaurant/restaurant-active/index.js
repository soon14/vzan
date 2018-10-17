const app = getApp();
const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
Page({
	data: {
		this_dish_info: {},
	},
	onLoad: function (options) {
		var that = this;
		var dish_id = options.dish_id;
		that.setData({ this_dish_id: dish_id });
		requestUtil.post(_DuoguanData.getDishInfo, { dish_id: dish_id }, (info) => {
			info.opentime = ''
			for (let i = 0; i < info.dish_info.open_time.length; i++) {
				info.opentime += (info.dish_info.open_time[i].dish_open_btime + '-' + info.dish_info.open_time[i].dish_open_etime) + ';'
			}
			info.dish_comment_fenshu = Number(info.dish_comment_fenshu)
			that.setData({ this_dish_info: info });
		}, this, {});
	},
	go_back_bind: function () {
		wx.navigateBack(1);
	}
})