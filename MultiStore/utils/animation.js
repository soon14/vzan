// X轴左右平移
function utilFilter(currentStatu, fpage) {
  // 创建动画实例
  var animation = wx.createAnimation({
    duration: 300,  //动画时长
    timingFunction: "ease", //线性
    delay: 0  //0则不延迟
  });
  this.animation = animation;
  animation.translateX(168).step();
  fpage.setData({
    animationDataFilter: animation.export()
  })
  setTimeout(function () {
    animation.translateX(0).step()
    fpage.setData({
      animationDataFilter: animation
    })
    //关闭抽屉
    if (currentStatu == "close") {
      fpage.setData(
        {
          showMadalFilterStatus: false
        }
      );
    }
  }.bind(this), 200)

  // 显示抽屉
  if (currentStatu == "open") {
    fpage.setData(
      {
        showMadalFilterStatus: true
      }
    );
  }
}
// Y轴往下平移
function utilDown(currentStatu, fpage) {
  // 创建动画实例
  var animation = wx.createAnimation({
    duration: 300,  //动画时长
    timingFunction: "ease", //线性
    delay: 0  //0则不延迟
  });
  this.animation = animation;
  animation.translateY(-168).step();
  fpage.setData({
    animationData: animation.export()
  })
  setTimeout(function () {
    animation.translateY(0).step()
    fpage.setData({
      animationData: animation
    })
    //关闭抽屉
    if (currentStatu == "close") {
      fpage.setData(
        {
          showModalStatus: false
        }
      );
    }
  }.bind(this), 200)

  // 显示抽屉
  if (currentStatu == "open") {
    fpage.setData(
      {
        showModalStatus: true
      }
    );
  }
}
// Y轴往上平移
function utilUp(currentStatu, fpage) {
  // 创建动画实例
  var animation = wx.createAnimation({
    duration: 300,  //动画时长
    timingFunction: "ease", //线性
    delay: 0  //0则不延迟
  });
  this.animation = animation;
  animation.translateY(500).step();
  fpage.setData({
    animationData: animation.export()
  })
  setTimeout(function () {
    animation.translateY(0).step()
    fpage.setData({
      animationData: animation
    })
    //关闭抽屉
    if (currentStatu == "close") {
      fpage.setData(
        {
          showModalStatus: false
        }
      );
    }
  }.bind(this), 200)

  // 显示抽屉
  if (currentStatu == "open") {
    fpage.setData(
      {
        showModalStatus: true
      }
    );
  }
}

//x轴左右切换
function utilRow(currentStatu, fpage) {
  // 创建动画实例
  var animation = wx.createAnimation({
    duration: 300,  //动画时长
    timingFunction: "ease", //线性
    delay: 0  //0则不延迟
  });
  this.animation = animation;
  animation.translateX(500).step();
  fpage.setData({
    animationData2: animation.export()
  })
  setTimeout(function () {
    animation.translateX(0).step()
    fpage.setData({
      animationData2: animation
    })
    //关闭抽屉
    if (currentStatu == "close") {
      fpage.setData(
        {
          showModalStatus2: false
        }
      );
    }
  }.bind(this), 200)

  // 显示抽屉
  if (currentStatu == "open") {
    fpage.setData(
      {
        showModalStatus2: true
      }
    );
  }
}
function utilSecond(currentStatu, fpage) {
  var animation = wx.createAnimation({
    duration: 300,  //动画时长
    timingFunction: "ease", //线性
    delay: 0  //0则不延迟
  });
  this.animation = animation;
  animation.translateY(728).step();
  fpage.setData({
    animationDataSecond: animation.export()
  })
  setTimeout(function () {
    animation.translateY(0).step()
    fpage.setData({
      animationDataSecond: animation
    })
    if (currentStatu == "close") {
      fpage.setData(
        {
          showModalStatus3: false
        }
      );
    }
  }.bind(this), 200)
  if (currentStatu == "open") {
    fpage.setData(
      {
        showModalStatus3: true
      }
    );
  }
}


//x轴左右切换
function utilRight(fpage) {
  // 创建动画实例
  var animation = wx.createAnimation({
    duration: 300,  //动画时长
    timingFunction: "ease", //线性
    delay: 0  //0则不延迟
  });
  this.animation = animation;
  animation.translateX(100).step();
  fpage.setData({
    animationDataRight: animation.export()
  })
  setTimeout(function () {
    animation.translateX(0).step()
    fpage.setData({
      animationDataRight: animation
    })
    //关闭抽屉
    // if (currentStatu == "close") {
    //   fpage.setData(
    //     {
    //       showModalStatus2: false
    //     }
    //   );
    // }
  }.bind(this), 200)

  // 显示抽屉
  // if (currentStatu == "open") {
  //   fpage.setData(
  //     {
  //       showModalStatus2: true
  //     }
  //   );
  // }
}

module.exports = {
  utilFilter,
  utilDown,
  utilUp,
  utilRow,
  utilSecond,
  utilRight,
}