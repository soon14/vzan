// pages/sellCenter/person.js
const app = getApp();
const util = require("../../utils/util.js");
const tools = require("../../utils/tools.js");
const page = require("../../utils/pageRequest.js");
Page({

  /**
   * 页面的初始数据
   */
	data: {

	},

  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		app.getUserInfo(res => {
			page.getSaleMan(res.UserId).then(data => {
				if (data.isok) {
					this.setData({ vm: data.obj })
				} else {
					tools.ShowMsg(data.msg)
				}
			})
		})
		util.setPageSkin(this);
	},
	personMore: function (e) {
		let id = e.currentTarget.id
		if (id == 1 && this.data.vm.state == -1) {
			tools.ShowMsg("你已被清退")
			return;
		}
		wx.redirectTo({
			url: "../sellCenter/personAcount?id=" + id,
		})
		// tools.goNewPage("../sellCenter/personAcount?id=" + id)
	},
	goMoney: function () {
		tools.goNewPage("../sellCenter/personMoney")
	},

	onPullDownRefresh: function () {
		tools.showLoadToast("正在刷新")
		this.onLoad()
		setTimeout(data => {
			tools.showToast("刷新成功")
			wx.stopPullDownRefresh()
		}, 1000)
	},
})