module.exports = {

  gonewpage: function(url) {
    if (getCurrentPages().length >= 8) {
      wx.redirectTo({
        url: url,
      })
    } else {
      wx.navigateTo({
        url: url,
      })
    }
  },

  gonewpagebyrd: function(url) {
    wx.redirectTo({
      url: url,
    })
  },

  switchtab: function(url) {
    wx.switchTab({
      url: url,
    })
  },

  goback: function(delta) {
    wx.navigateBack({
      delta: delta
    })
  },

  showtoast: function(title, icon) {
    wx.showToast({
      title: title,
      icon: icon
    })
  },

  stoppulldown: function() {
    var that = this
    setTimeout(() => {
      // that.showtoast('刷新成功', 'success')
      wx.stopPullDownRefresh()
    }, 1000)
  },

  // 图片点击放大
  previewsingleimage: function(current, imglst) {
    let urls = []
    imglst ? urls = imglst : urls.push(current)
    wx.previewImage({
      current: current,
      urls: urls,
    })
  },

  formatDuring: function(mss) {
    if (mss < 0) {
      return "00:00:00";
    } else {
      var days = parseInt(mss / (1000 * 60 * 60 * 24));
      var hours = parseInt((mss % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
      var minutes = parseInt((mss % (1000 * 60 * 60)) / (1000 * 60));
      var seconds = (mss % (1000 * 60)) / 1000;
      if (hours == 0) {
        return minutes + ":" + Math.round(seconds);
      } else if (days == 0) {
        return hours + ":" + minutes + ":" + Math.round(seconds);
      } else {
        return days + ":" + hours + ":" + minutes + ":" + Math.round(seconds);
      }
    }
  },

  checksetting: function(that) {
    wx.getSetting({
      success: function(res) {
        if (!res.authSetting["scope.writePhotosAlbum"] && res.authSetting["scope.writePhotosAlbum"] != undefined) {
          that.setData({
            AlbumUnset: true
          })
        } else {
          that.setData({
            AlbumUnset: false
          })
        }
      }
    })
  },

  canvasToTempFilePath: function(index, canvasId, width, height, that) {
    wx.canvasToTempFilePath({
      x: 0,
      y: 0,
      width: width,
      height: height,
      destWidth: width,
      destHeight: height,
      canvasId: canvasId,
      success: function(res) {
        console.log(res.tempFilePath)
        wx.saveImageToPhotosAlbum({
          filePath: res.tempFilePath,
          success(res) {
            if (index == 0) {
              wx.showToast({
                title: '图片保存成功',
              })
            }
            if (index == 1) {
              wx.showModal({
                title: '提示',
                content: '保存已保存成功！您可以用该图片去分享朋友圈哦',
                showCancel: false
              })
            }
          },
          fail: function(res) {
            if (res.errMsg = 'saveImageToPhotosAlbum:fail auth deny') {
              that.setData({
                AlbumUnset: true
              })
            }
          }
        })
      }
    })
  }
}