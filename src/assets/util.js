import $ from 'jquery';
import OSS from 'ali-oss';
import http from "web-http-kaaden";
import time from 'web-time-kaaden';
const host = 'http://kaaden.orrzt.com';
// const host = "http://localhost:8082"
const addr = {
  gossipLogin: host + "/gossipLogin",
  gossipRegister: host + "/gossipRegister",
  gossipCheck: host + "/gossipCheck",
  gossipSendMail: host + "/gossipSendMail",
  gossipFind: host + "/gossipFind",
  gossipUpdate: host + "/gossipUpdate",
  gossipConcactLst: host + "/gossipConcactLst",
  gossipUpdateUserInfo: host + "/gossipUpdateUserInfo",
  gossipFriend: host + "/gossipFriend",
};
const tools = {
  goNewPage(url, target) {
    target.$router.push(url)
  },
  goBack(target) {
    target.$route.back(-1)
  },
  showError(msg, target) {
    target.$notify.error({
      title: '错误',
      message: msg
    });
  },
  showSuccess(msg, tareget) {
    tareget.$notify({
      title: '成功',
      message: msg,
      type: 'success'
    });
  },
  fontSize() {
    var deviceWidth = $(document).width();
    if (deviceWidth > 1920) {
      deviceWidth = 1920;
    }
    var fontSize = deviceWidth / 19.2;
    $("html").css("fontSize", fontSize);
    console.log(fontSize)
  },
  browserJudg() {
    var userAgent = navigator.userAgent; //取得浏览器的userAgent字符串
    var isOpera = userAgent.indexOf("Opera") > -1;
    if (isOpera) {
      return "Opera"
    }; //判断是否Opera浏览器
    if (userAgent.indexOf("Firefox") > -1) {
      return "FF";
    } //判断是否Firefox浏览器
    if (userAgent.indexOf("Chrome") > -1) {
      return "Chrome";
    }
    if (userAgent.indexOf("Safari") > -1) {
      return "Safari";
    } //判断是否Safari浏览器
    if (userAgent.indexOf("compatible") > -1 && userAgent.indexOf("MSIE") > -1 && !isOpera) {
      return "IE";
    }; //判断是否IE浏览器
  },
  showChat(item, target) {
    var changeLine = false
    if (target.vm._index != -1) {
      if (target.vm._index == item.friendId) {
        return;
      } else {
        changeLine = true
      }
    } else {
      changeLine = false
    }
    target.vm._index = item.friendId
    target.vm.selIndex = item.friendId
    target.vm.chatTitle = item.friendName
    target.vm.rUserInfo = item
    let vm = {
      userId: target.vm.userInfo.userId,
      friendId: item.friendId
    }
    let add = {
      changeLine,
      friendId: item.friendId
    }
    target.$socket.emit('addChatMsg', add)
    target.$socket.emit('msghistory', vm)
    target.vm.search = ""
  },
  sendMsg(g, target) {
    target.$socket.emit('Msg', g)
    target.vm.chatOneLst.push(g)
    target.vm.msg = ''
  },
  upLoadBaseImg(file) {
    return new Promise(function (resolve, reject) {
      // 调用函数处理图片 　　　　　　
      tools.base64(file).then(data => {
        // 图片base64 → blob对象 → arrayBuffer对象 → buffer对象 → 成功上传(。・∀・)ノ゛
        var base64 = data.split(',')[1];
        var fileType = data.split(';')[0].split(':')[1];
        var blob = tools.toBlob(base64, fileType);
        var reader = new FileReader();
        reader.readAsArrayBuffer(blob);
        reader.onload = function (event) {
          const client = new OSS({
            region: 'oss-cn-hangzhou',
            accessKeyId: 'LTAIBP8SaLnUNWnE',
            accessKeySecret: '2oLe5CYTuyGmqxZZw1CqTxaGg0ORNT',
            bucket: 'kaaden-upload'
          });
          const obj = time.timestamp();
          const suffix = file.name.substr(file.name.indexOf("."))
          const storeAs = 'img/' + obj + suffix
          const buffer = new OSS.Buffer(event.target.result);
          client.put(storeAs, buffer).then(res => {
            resolve(res)
          })
        }
      })
    })
  },
  //图片压缩
  base64(file) {
    return new Promise(function (resolve, rejct) {
      var reader = new FileReader();
      var image = new Image();
      var canvas = tools.createCanvas();
      var ctx = canvas.getContext("2d");
      reader.onload = function () { // 文件加载完处理
        var result = this.result;
        image.onload = function () { // 图片加载完处理
          var imgScale = tools.imgScaleW(1500, this.width, this.height);
          canvas.width = imgScale.width;
          canvas.height = imgScale.height;
          ctx.drawImage(image, 0, 0, imgScale.width, imgScale.height);
          var dataURL = canvas.toDataURL('image/jpeg'); // 图片base64
          ctx.clearRect(0, 0, imgScale.width, imgScale.height); // 清除画布
          resolve(dataURL)
        }
        image.src = result;
      };
      reader.readAsDataURL(file);
    })
  },
  createCanvas() { // 创建画布
    var canvas = document.getElementById('canvas');
    if (!canvas) {
      var canvasTag = document.createElement('canvas');
      canvasTag.setAttribute('id', 'canvas');
      canvasTag.setAttribute('style', 'display:none;'); //隐藏画布
      document.body.appendChild(canvasTag);
      canvas = document.getElementById('canvas');
    }
    return canvas;
  },
  imgScaleW(maxWidth, width, height) {
    var imgScale = {};
    var w = 0;
    var h = 0;
    if (width <= maxWidth && height <= maxWidth) { // 如果图片宽高都小于限制的最大值,不用缩放
      imgScale = {
        width: width,
        height: height
      }
    } else {
      if (width >= height) { // 如果图片宽大于高
        w = maxWidth;
        h = Math.ceil(maxWidth * height / width);
      } else { // 如果图片高大于宽
        h = maxWidth;
        w = Math.ceil(maxWidth * width / height);
      }
      imgScale = {
        width: w,
        height: h
      }
    }
    return imgScale;
  },
  toBlob(urlData, fileType) {
    var bytes = window.atob(urlData),
      n = bytes.length,
      u8arr = new Uint8Array(n);
    while (n--) {
      u8arr[n] = bytes.charCodeAt(n);
    }
    return new Blob([u8arr], {
      type: fileType
    });
  }
}
$(window).resize(function () {
  tools.fontSize();
});
const core = {
  // 注册检查账号是否存在
  gossipCheck(account) {
    return new Promise(function (resolve, reject) {
      http.post(addr.gossipCheck, {
        account,
      }).then(data => {
        resolve(data)
      })
    })
  },
  // 发送邮件
  gossipEmail(email) {
    return new Promise(function (resolve, reject) {
      http.post(addr.gossipSendMail, {
        email,
      }).then(data => {
        resolve(data)
      })
    })
  },
  // 注册
  gossipRegister(vm) {
    return new Promise(function (resolve, rejcet) {
      http.post(addr.gossipRegister, {
        vm,
      }).then(data => {
        resolve(data)
      })
    })
  },
  // 登录
  gossipLogin(vm) {
    return new Promise(function (resolve, rejcet) {
      http.post(addr.gossipLogin, {
        vm,
      }).then(data => {
        resolve(data)
      })
    })
  },
  //查找用户信息
  gossipFind(type, userId, account) {
    return new Promise(function (resolve, rejcet) {
      http.post(addr.gossipFind, {
        type,
        userId,
        account,
      }).then(data => {
        resolve(data)
      })
    })
  },
  // 更新用户信息
  gossipUpdate(vm) {
    return new Promise(function (resolve, reject) {
      http.post(addr.gossipUpdate, {
        vm
      }).then(data => {
        resolve(data)
      })
    })
  },
  // 图片上传
  gossipUpload(formData) {
    return new Promise(function (resolve, reject) {
      http.uploadImg(formData).then(data => {
        resolve(data)
      })
    })
  },
  //获取好友列表
  gossipConcactLst(userId) {
    return new Promise(function (resolve, reject) {
      http.post(addr.gossipConcactLst, {
        userId
      }).then(data => {
        resolve(data)
      })
    })
  },
  gossipUpdateUserInfo(vm) {
    return new Promise(function (resolve, reject) {
      http.post(addr.gossipUpdateUserInfo, {
        vm
      }).then(data => {
        resolve(data)
      })
    })
  },
  //同意成为好友
  gossipFriend(vm) {
    return new Promise(function (resolve, reject) {
      http.post(addr.gossipFriend, {
        vm
      }).then(data => {
        resolve(data)
      })
    })

  }
}
export {
  tools,
  addr,
  time,
  core,
}
