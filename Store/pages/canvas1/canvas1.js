// pages/canvas1/canvas1.js
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
  data: {

  },
  canvasIdErrorCallback: function (e) {
    console.error(e.detail.errMsg)
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {

  },
  // 圆圈进度条
  canvasCircle: function (a, b) {
    var cxt_arc = wx.createCanvasContext('canvasArc');//创建并返回绘图上下文context对象。 
    cxt_arc.setLineWidth(2);
    cxt_arc.setStrokeStyle('#f7f7f7');
    cxt_arc.setLineCap('round')
    cxt_arc.beginPath();//开始一个新的路径 
    cxt_arc.arc(64, 64, 60, 0, 2 * Math.PI, false);//设置一个原点(64,64)，半径为60的圆的路径到当前路径 
    cxt_arc.stroke();//对当前路径进行描边 
    // 颜色圈圈
    cxt_arc.setLineWidth(2);
    cxt_arc.setStrokeStyle('#f20033');
    cxt_arc.setLineCap('round')
    cxt_arc.beginPath();//开始一个新的路径 
    cxt_arc.arc(64, 64, 60, 1.5 * Math.PI, 2 * Math.PI * (1 / 4) + 1.5 * Math.PI, false);
    cxt_arc.stroke();//对当前路径进行描边 
    cxt_arc.draw();
  },
  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  canvasToTempFilePath: function () {
    wx.canvasToTempFilePath({
      x: 0,
      y: 0,
      width: 200,
      height: 200,
      destWidth: 200,
      destHeight: 200,
      canvasId: 'firstCanvas',
      success: function (res) {
        console.log(res.tempFilePath)
        wx.saveImageToPhotosAlbum({
          filePath: res.tempFilePath,
          success(res) {
          }
        })
      }
    })
  },
  onReady: function () {
    var that = this
    var windowWidth = 325 //系统宽度
    var windowHeight = 440 //系统高度
    var context = wx.createCanvasContext('firstCanvas')
    var ImgUrl = '/image/a34.png'
    var topic = '/image/c1.png'
    var finger = '/image/c2.png'
    var code = '/image/c2.png' //先下载二维码 返回的tempFilePath用作canvas拼图（只能本地图片）
    var bottomprice = '最低' + '1' + '元，原价' + '99' + '元，等你来砍'
    var bottomprice1 = '能砍多少看你本事了'
    context.setFillStyle('white')
    context.fillRect(0, 0, windowWidth, windowHeight)
    context.drawImage(ImgUrl, windowWidth * 0.03, windowHeight * 0.02, windowWidth * 0.94, windowHeight * 0.38);
    context.drawImage(topic, windowWidth * 0.15, windowHeight * 0.42, windowWidth * 0.7, windowHeight * 0.16);
    context.drawImage(finger, windowWidth * 0.58, windowHeight * 0.72, windowWidth * 0.3, windowHeight * 0.22);
    context.drawImage(code, windowWidth * 0.13, windowHeight * 0.72, windowWidth * 0.3, windowHeight * 0.22);
    context.setFontSize(13)
    context.setFillStyle('#fbb47b')
    context.fillText(bottomprice, windowWidth * 0.24, windowHeight * 0.63)
    context.fillText(bottomprice1, windowWidth * 0.33, windowHeight * 0.68)
    context.draw()
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