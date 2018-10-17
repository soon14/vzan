// pages/myGroup2/myGroupDetail.js
const addr = require("../../utils/addr.js");
var http = require("../../utils/http.js");
const util = require("../../utils/util.js");
var app = getApp()
var tools = require('../../utils/tools.js');
Page({

  /**
   * 页面的初始数据
   */
	data: {
		showmoreGroup: false,//查看更多拼团弹窗
		showpublic: false,//拼团须知弹窗
	},

  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		if (options.aid != undefined) { wx.setStorageSync('aid', options.aid) }
		app.getUserInfo(function () {
			that.setData({ userInfo: wx.getStorageSync('userInfo'), })
			that.GetGroupDetail(options.groupid)
		})
		util.setPageSkin(that);


		setTimeout(function () {
			setInterval(function () {
				if (that.data.postdata != null) {
					var nowTime = new Date().getTime()
					var endtime = ((new Date(that.data.postdata.EndDateStr)).getTime() - nowTime)
					that.data.postdata.endtime = tools.formatDuring(endtime)

					for (var i = 0; i < that.data.GroupSponsorList.length; i++) {
						var endtimeList = ((new Date(that.data.GroupSponsorList[i].ShowEndTime)).getTime() - nowTime)
						that.data.GroupSponsorList[i].endtimeList = tools.formatDuring(endtimeList)
					}
					that.setData({ postdata: that.data.postdata, GroupSponsorList: that.data.GroupSponsorList })
				}
			}, 1000)
		}, 1000)

	},
	// 查询详情单
	GetGroupDetail: function (groupid) {
		var that = this
		http.getAsync(
			addr.Address.GetentGroupDetail,
			{
				appid: app.globalData.appid,
				groupid: groupid
			}
		).then(function (data) {
			if (data.isok == 1) {
				data.postdata.StartDateStr = util.ChangeDateFormatNew(data.postdata.StartDate)
				data.postdata.EndDateStr = util.ChangeDateFormatNew(data.postdata.EndDate)
				var finduserId = data.postdata.GroupUserList.find(f => f.Id == wx.getStorageSync('userInfo').UserId)
				if (finduserId) {
					data.postdata.Address = finduserId.Address
					data.postdata.Name = finduserId.Name
					that.setData({ haveJoin: true })
				} else {
					that.setData({ haveJoin: false })
				}

				for (var i = 0; i < data.GroupSponsorList.length; i++) {
					var ifjoined = data.GroupSponsorList[i].GroupUserList.find(f => f.Id == wx.getStorageSync('userInfo').UserId)
					if (ifjoined) {
						data.GroupSponsorList[i].haveJoin = true
					} else {
						data.GroupSponsorList[i].haveJoin = false
					}
				}
				if (finduserId) {
					data.postdata.Address = finduserId.Address
					data.postdata.Name = finduserId.Name
					that.setData({ haveJoin: true })
				} else {
					that.setData({ haveJoin: false })
				}

				that.setData({
					postdata: data.postdata,
					GroupSponsorList: data.GroupSponsorList,
					isSponsor: data.postdata.SponsorUserId == wx.getStorageSync('userInfo').UserId ? true : false
				})
			}
			console.log('我是groupid', groupid)
		})
	},
	navtoG2Detail: function (e) {
		tools.goNewPage('../group2/groupDetail?pid=' + e.currentTarget.dataset.goodid + '&groupid=' + e.currentTarget.dataset.groupid)
	},
	navtooDetail: function (e) {
		this.setData({ showmoreGroup: false })
		tools.goNewPage('/pages/myGroup2/myGroupDetail?groupid=' + e.currentTarget.dataset.groupid)
	},
	navtoindex: function () {
		wx.reLaunch({ url: '../index/index' })
	},
	showmore: function () {
		this.setData({ showmoreGroup: !this.data.showmoreGroup })
	},
	showpublic: function () {
		this.setData({ showpublic: !this.data.showpublic })
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

	},

  /**
   * 生命周期函数--监听页面隐藏
   */
	onHide: function () {

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
		var that = this
		return {
			title: that.data.postdata.GroupName,
			imageUrl: that.data.postdata.GroupImage,
			path: 'pages/myGroup2/myGroupDetail?groupid=' + that.data.postdata.Id + '&aid=' + wx.getStorageSync('aid'),
			fail: function (res) {
				console.log(res)
			}
		}
	}
})