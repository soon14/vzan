// pages/editMyinfo/editMyinfo.js
var addr = require('../../utils/addr.js')
var tool = require('../../template/Pediluvium.js')
var template = require('../../template/template.js')
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
	data: {
		imgLogo: '',
		username: '',
		sign: '',
		imgArray: [],

		isPreview: 0,// 0是编辑模式 1是预览模式
	},

  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		if (options.isPreview != undefined) {
			this.setData({
				imgLogo: options.imgLogo,
				username: options.username,
				sign: options.sign,
				imgArray: JSON.parse(options.imgArray),
				isPreview: options.isPreview,
			})
			wx.setNavigationBarTitle({
				title: '预览'
			})
		} else {
			tool.GetTechInfo(this, 5)
		}
	},
	
	chooseImage(count, setwhitch) { //setwhitch==1=>imgLogo  setwhitch==2=>imgArray
		var that = this
		wx.chooseImage({
			count: count, // 默认9
			sizeType: ['original', 'compressed'],
			sourceType: ['album'],
			success: function (res) {
				wx.showLoading()
				var tempFilePaths = res.tempFilePaths
				var j = 0
				function upload() {
					wx.uploadFile({
						url: addr.Address.Upload,
						filePath: tempFilePaths[j],
						name: 'file',
						formData: {
							filetype: 'img',
						},
						success: function (imgpath) {
							var imgpath = JSON.parse(imgpath.data)
							if (setwhitch == 1) {
								that.setData({ imgLogo: imgpath.msg })
								wx.hideLoading()
							}
							else {
								that.data.imgArray.push(imgpath.msg)
								that.setData({ imgArray: that.data.imgArray })
								wx.hideLoading()
							}
							j++
							if (j < tempFilePaths.length) {
								upload()
							}
						},
						fail: function (res) {
							template.showtoast('上传失败', 'loading')
						}
					})
				}
				upload()
			}
		})
	},

	uploadInfo: function () {
		var that = this
		var photo = ''
		for (var j = 0; j < that.data.imgArray.length; j++) {
			photo += that.data.imgArray[j] + ','
		}
		var userInfo = {
			id: app.globalData.id,
			headImg: that.data.imgLogo,
			jobNumber: that.data.username,
			desc: that.data.sign,
			photo: photo
		}
		tool.SaveUserInfo(that, userInfo, function (res) {
			if (res.data.isok == true) {
				template.showtoast('更新成功', 'success')
				setTimeout(function () {
					template.goBackPage(2)
				}, 1000)
			}
		})
	},

	uploadLogo: function () {
		this.chooseImage(1, 1)
	},
	inputname: function (e) {
		this.setData({ username: e.detail.value })
	},
	inputsign: function (e) {
		this.setData({ sign: e.detail.value })
	},
	uploadArray: function () {
		if (this.data.imgArray.length == 0) {
			var count = 9
		} else {
			var count = 20 - this.data.imgArray.length
		}
		this.chooseImage(count, 2)
	},

	delimgLogo: function () {
		this.setData({ imgLogo: '' })
	},
	delimgArray: function (e) {
		this.data.imgArray.splice(e.currentTarget.id, 1)
		this.setData({ imgArray: this.data.imgArray })
	},
	previewInfo: function () {
		template.goNewPage('../editMyinfo/editMyinfo?imgLogo=' + this.data.imgLogo + '&username=' + this.data.username + '&sign=' + this.data.sign + '&imgArray=' + JSON.stringify(this.data.imgArray) + '&isPreview=1')
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

	}
})