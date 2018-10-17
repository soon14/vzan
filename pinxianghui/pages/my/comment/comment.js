// pages/my/comment/comment.js
Page({

  /**
   * 页面的初始数据
   */
  data: {
    isComment: false,
    comment_score: [{
        content: '好评',
        fontcss: 'icon-hua_flower3',
        ischoose: true
      },
      {
        content: '中评',
        fontcss: 'icon-hua_flower2',
        ischoose: false
      },
      {
        content: '差评',
        fontcss: 'icon-hua_flower2',
        ischoose: false
      }
    ],

    you: 0,
  },
  hidden_modal: function() {
    this.setData({
      isComment: false
    })
  },
  set_score: function(e) {
    var index = e.currentTarget.id
    var shixin = e.currentTarget.dataset.shixin
    if (shixin == 'true') {
      this.data.you = Number(index)
    } else {
      this.data.you += Number(index)
    }
    this.setData({
      you: Number(this.data.you)
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {

  },

  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function() {

  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function() {

  },

  /**
   * 生命周期函数--监听页面隐藏
   */
  onHide: function() {

  },

  /**
   * 生命周期函数--监听页面卸载
   */
  onUnload: function() {

  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function() {

  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function() {

  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function() {

  }
})