var util = require("util.js");
var api = require("network");
var addr = require("addr");
var app = getApp();

function chooseImage(parmas, that) {
  var items = that.data.items
  var row = parmas.currentTarget.dataset.row
  var currentItem = items[row]
  var images = currentItem.content.imageList
  var maxImageCount = currentItem.content.maxImageCount
  wx.hideLoading()
  wx.chooseImage({
    count: maxImageCount,
    success: function (res) {
      var totalImages = res.tempFilePaths
      if (totalImages.length < maxImageCount) {
        currentItem.content.maxImageCount = maxImageCount - totalImages.length
      }
      else {
        currentItem.content.images_full = true;
      }
      // 上传图片并获取url保存在imageUrlList数组中
      currentItem.content.imageIdList = []
      items[row] = currentItem
      that.data.items = items
      that.setData(that.data)
      var tempimgurl = []

      for (var index = 0; index < totalImages.length; index++) {
        var imagePath = totalImages[index]
        tempimgurl.push(imagePath)
        util.showNavigationBarLoading();
        wx.showToast({
          title: '正在上传图片',
          icon: 'loading',
          mask: true,
          duration: 10000
        })
      }
      uploadoneImg(tempimgurl, 0, currentItem, that, items, row)
    }
  })
}
function uploadoneImg(tempimgurl, index, currentItem,that,items,row)
{
  api.uploadFile(addr.Address.uploadImage, tempimgurl[index], { index: index })
    .then(function (success) {
      console.log(success);
      var json = JSON.parse(success.data)
      var path = json.path
      // for (var imgindex in currentItem.content.imageList) {
      //   var tempurl = currentItem.content.imageList[imgindex]
      //   if (tempurl == json.index) {
      //     currentItem.content.imageList[imgindex] = path
      //     break
      //   }
      // }
      currentItem.content.imageList.push(path)
      currentItem.content.imageAddUrlList.push(path)

      if (json.index < tempimgurl.length-1)
      {
        uploadoneImg(tempimgurl, json.index + 1, currentItem, that, items, row)
      }
      else{
        items[row] = currentItem
        that.data.items = items
        that.setData(that.data)
        util.hideNavigationBarLoading();
        wx.hideToast()
      }
      


    }, function (fail) {
      wx.showModal({
        title: '提示',
        content: "上传图片失败",
      })
      console.log("上传图片失败");
      console.log(fail);
      wx.hideToast()
      util.hideNavigationBarLoading();
    })
}
function previewImage(parmas, that) {
  var items = that.data.items
  var row = parmas.currentTarget.dataset.row
  var current = parmas.target.dataset.src
  var currentItem = items[row]
  var images = currentItem.content.imageList
  wx.previewImage({
    current: current,
    urls: images
  })
}
//删除图片
function clearImage(parmas, that) {
  var items = that.data.items
  var row = parmas.currentTarget.dataset.row
  var index = parmas.target.dataset.index
  var currentItem = items[row]
  var images = currentItem.content.imageList

  var imageIds = currentItem.content.imageIdList
  var imageId = imageIds[index]
  var openId = app.globalData.userInfo.openId

  wx.showModal({
    title: '删除图片',
    content: '删除图片后无法回复，确定上传图片',
    success: function (r) {
      if (r.cancel) {
        return
      }
      //删除图片
      if (currentItem.content.imageIdList.length == 0) {
        currentItem.content.imageList.splice(index, 1);
        currentItem.content.imageAddUrlList.splice(index, 1);
        console.log("是本地的图片")
        if (currentItem.content.imageList.length < currentItem.content.currentmaxImageCount) {
          currentItem.content.images_full = false;
          currentItem.content.maxImageCount = currentItem.content.currentmaxImageCount - currentItem.content.imageList.length
        }
        items[row] = currentItem
        that.setData({
          items: items
        })
        return
      }

      if (imageId == null) {
        wx.showToast({
          title: '删除图片失败',
          icon: 'loading',
          duration: 1000
        })
        return
      }
      items[row] = currentItem
      // if (currentItem.content.imageList.length < currentItem.content.currentmaxImageCount) {
      //   currentItem.content.images_full = false;
      //   currentItem.content.maxImageCount = currentItem.content.currentmaxImageCount - currentItem.content.imageList.length
      // }

      wx.showToast({
        title: '正在删除图片',
        icon: 'loading',
        duration: 10000
      })
      wx.request({
        url: addr.Address.deleteImage,
        data: {
          imageId: imageId,
          openId: openId,
        },
        header: {
          'content-type': 'application/json'
        },
        success: function (res) {
          console.log(res.data)
          if (res.data.result) {

            currentItem.content.imageIdList.splice(index, 1);
            currentItem.content.imageList.splice(index, 1);
            currentItem.content.maxImageCount = currentItem.content.currentmaxImageCount - currentItem.content.imageList.length
            // if (index < currentItem.content.imageList.length) {
            //   // 共用一个地址
            //   if (imageId != 0) {
            //     currentItem.content.imageList.splice(index, 1);
            //   }
            // }
            items[row] = currentItem
            if (currentItem.content.imageList.length < currentItem.content.currentmaxImageCount) {
              currentItem.content.images_full = false;
            }

            that.data.items = items
            that.setData(that.data)
            wx.hideToast()
          }
          else {
            wx.showToast({
              title: '删除图片失败,请稍后重试',
              icon: 'loading',
              duration: 1000
            })
          }
        },
        fail: function () {
          wx.showToast({
            title: '删除图片失败,请稍后重试',
            icon: 'loading',
            duration: 1000
          })
        }
      })
      that.data.items = items
      that.setData(that.data)
    }
  })
}


module.exports = {
  chooseImage: chooseImage,
  clearImage: clearImage,
  previewImage: previewImage,
};