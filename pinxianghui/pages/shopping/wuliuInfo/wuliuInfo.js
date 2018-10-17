// pages/shopping/wuliuInfo/wuliuInfo.js
Page({

  /**
   * 页面的初始数据
   */
	data: {
		aaa: { "Id": 301, "CreateDate": "2018-04-28T16:22:35", "OrderId": 21994, "OrderType": 0, "ContactName": "bhh", "ContactTel": "13544697539", "CompanyCode": "YZPY", "CompanyTitle": "邮政快递包裹", "DeliveryNo": "9892485158136", "Address": "内蒙古自治区 赤峰市 阿鲁科尔沁旗 ghhh", "IsTrack": false, "FeedBack": "[{\"AcceptStation\":\"【全国中心邮政无锡市大宗邮件】已收寄\",\"AcceptTime\":\"2018-04-13 17:20:14\",\"Remark\":null},{\"AcceptStation\":\"离开【邮政无锡市大宗邮件】\",\"AcceptTime\":\"2018-04-13 19:21:35\",\"Remark\":null},{\"AcceptStation\":\"到达【无锡】\",\"AcceptTime\":\"2018-04-13 19:34:00\",\"Remark\":null},{\"AcceptStation\":\"离开【无锡中心】，下一站【无锡速转】\",\"AcceptTime\":\"2018-04-13 19:36:27\",\"Remark\":null},{\"AcceptStation\":\"离开【无锡速转】，下一站【速递三角】\",\"AcceptTime\":\"2018-04-14 06:09:00\",\"Remark\":null},{\"AcceptStation\":\"离开【广州航站】，下一站【番禺特快】\",\"AcceptTime\":\"2018-04-16 13:03:00\",\"Remark\":null},{\"AcceptStation\":\"到达【番禺特快】\",\"AcceptTime\":\"2018-04-16 13:09:10\",\"Remark\":null},{\"AcceptStation\":\"离开【番禺特快】，下一站【天河中信】\",\"AcceptTime\":\"2018-04-16 13:53:00\",\"Remark\":null},{\"AcceptStation\":\"正在投递,投递员:黄嘉文,电话:18922185431【广州市天河区中信】\",\"AcceptTime\":\"2018-04-16 14:07:35\",\"Remark\":null},{\"AcceptStation\":\"已签收,本人签收:马潇;,投递员：黄嘉文 18922185431 \",\"AcceptTime\":\"2018-04-16 17:01:36\",\"Remark\":null}]", "Mark": null, "Status": 4, "Reason": "" },
		item1: [
			{},
			{}
		],
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		this.setData({
			'aaa.FeedBackStr': JSON.parse(this.data.aaa.FeedBack)
		})
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