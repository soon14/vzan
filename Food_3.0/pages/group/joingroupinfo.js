// pages/group/joingroupinfo.js
var tool = require('../../template/Food2.0.js');
var template = require('../../template/template.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		islogin: false,
		showmoregroup: false,
		showpublic: false,
	},
	showmore: function () {
		this.setData({ showmoregroup: !this.data.showmoregroup })
	},
	showpublic: function () {
		this.setData({ showpublic: !this.data.showpublic })
	},
	go_joingroup: function (e) {
		template.goNewPage('../group/group_goodinfo?groupid=' + e.currentTarget.dataset.goodid + '&isshareGroup=' + this.data.isshareGroup + '&GID=' + this.data.postdata.Id)
	},
	navtoindex: function () {
		wx.reLaunch({ url: '../home/home' })
	},
	navito_joingroupinfo: function (e) {
		template.goNewPage('../group/joingroupinfo?groupid=' + e.currentTarget.dataset.groupid)
	},
	navito_groupgoodinfo: function (e) {
		template.goNewPage('../group/group_goodinfo?groupid=' + e.currentTarget.dataset.goodid + '&isshareGroup=1' + '&GID=' + e.currentTarget.dataset.gid)
	},
	getUserInfo: function (e) {
		var that = this
		var _e = e.detail
		if (e.detail.errMsg != 'getUserInfo:fail auth deny') {
			wx.login({
				success: function (res) {
					app.login(res.code, _e.encryptedData, _e.signature, _e.iv, function (cb) {
						that.setData({ islogin: true })
					}, 0)
				}
			})
		} else {
			wx.showModal({
				title: '提示',
				content: '你拒绝了登录授权，请再次点击登录进行操作。',
				showCancel: false
			})
		}
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		that.data.isshareGroup = options.isshareGroup != undefined ? options.isshareGroup : 0
		app.new_login(function (e) {
			tool.GetGroupDetail(that, options.groupid)
		})
		that.setData({ islogin: wx.getStorageSync('userInfo').avatarUrl == null ? false : true })

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
		var that = this
		setTimeout(function () {
			that.data.timeInter = setInterval(function () {
				if (that.data.postdata != null) {
					var nowTime = new Date().getTime()
					var endtime = ((new Date(that.data.postdata.EndDateStr)).getTime() - nowTime)
					that.data.postdata.endtime = template.formatDuring(endtime)

					for (var i = 0; i < that.data.GroupSponsorList.length; i++) {
						var endtimeList = ((new Date(that.data.GroupSponsorList[i].ShowEndTime)).getTime() - nowTime)
						that.data.GroupSponsorList[i].endtimeList = template.formatDuring(endtimeList)
					}
					that.setData({ postdata: that.data.postdata, GroupSponsorList: that.data.GroupSponsorList })
				}
			}, 1000)
		}, 1000)
	},

  /**
   * 生命周期函数--监听页面隐藏
   */
	onHide: function () {
		clearInterval(this.data.timeInter)
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

  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {
		return {
			title: this.data.postdata.GroupName,
			imageUrl: this.data.postdata.GroupImage,
			path: 'pages/group/joingroupinfo?groupid=' + this.data.postdata.Id + '&isshareGroup=1',
		}
	}
})