var addr = require('./addr.js');
var isdebug = true;

var vm = {
  list: [],
  ispost: false,
  loadall: false,
  pagesize: 20,
  pageindex: 1
};

var requestParameter = {
	url: "",
	data: {},
	method: "GET",
	header: {
		"content-type": "application/x-www-form-urlencoded"
	}
};
var http = {
	//异步请求
	post: function (url, data, isjson) {
		return new Promise(function (resolve, reject) {
			let p = JSON.parse(JSON.stringify(requestParameter));
			if (isjson) {
				p.header["content-type"] = "application/json";
			}
			wx.request(
				Object.assign({}, p, {
					url,
					data,
					method: "POST",
					fail: function (e) {
						isdebug && console.log(e, data);
						resolve("");
					},
					success: function (e) {
						if (e.statusCode == 200) {
							resolve(e.data);
						} else {
							isdebug && console.log(e);
							resolve("");
						}
					}
				})
			);
		});
	},
	get: function (url, data) {
		return new Promise(function (resolve, reject) {
			wx.request(
				Object.assign({}, requestParameter, {
					url,
					data,
					fail: function (e) {
						isdebug && console.log(`请求 ${url} 失败！\r\n 错误信息：${e.errMsg}`);
						resolve("");
					},
					success: function (e) {
						if (e.statusCode == 200) {
							resolve(e.data);
						} else {
							isdebug && console.log(e);
							resolve("");
						}
					}
				})
			);
		});
	}
};

var core = {
  trim: function(input) {
    if (input) {
      return input.replace(/\s+/gm, "");
    }
    return "";
  },
  safeNum: function(num, isreqint) {
    num = Number(num) || 0;
    if (isreqint) {
      num = parseInt(num);
    } else {
      if (Number.isInteger(num)) {

      } else {
        num = Number(parseFloat(num).toFixed(2));
      }
    }
    if (num < 0)
      num = 0;
    return num;
  },
  alert: function(content, title, ok, showCancel) {
    wx.showModal({
      title: title || "提示",
      content: content,
      showCancel: showCancel || true,
      success: function(res) {
        if (res.confirm) {
          if (ok) {
            ok();
          }
        } else if (res.cancel) {

        }
      }
    });
  },
  setStorageSync: function (key, value) {
    try {
      wx.setStorageSync(key, value)
    } catch (e) {
      return "";
    }
  },
  getStorageSync: function (key) {
    try {
      var value = wx.getStorageSync(key)
      return value;
    } catch (e) {
      return "";
    }
  },
  upload: function(filetype = "img", count = 1) {
    var isbreak = false;
    return new Promise(function(resolve, reject) {
      if (filetype == "img") {
        wx.chooseImage({
          count, //默认只能传一张
          success: function(res) {
            var uploadCount = 0;
            var uploadImgs = [];
            var tempFilePaths = res.tempFilePaths;

            function uploadOne() {
              wx.showLoading({
                mask: true,
                title: "上传中..."
              });
              const uploadTask = wx.uploadFile({
                url: addr.UploadFile,
                filePath: tempFilePaths[uploadCount],
                name: "file",
                formData: {
                  filetype: filetype
                },
                success: function(res) {
                  console.log(res);
                  var data = JSON.parse(res.data);
                  if (data.result) {
                    uploadCount += 1;
                    console.log("上传成功", data.msg);
                    uploadImgs.push(data.msg);
                  } else {
                    isbreak = true;
                    console.log("上传失败", data);
                    resolve("");
                  }
                  if (!isbreak && uploadCount < tempFilePaths.length) {
                    uploadOne();
                  } else {
                    console.log("上传完毕", uploadImgs);
                    resolve(uploadImgs);
                  }
                },
                complete: function() {
                  wx.hideLoading();
                }
              });
              uploadTask.onProgressUpdate(res => {
                wx.showLoading({
                  mask: true,
                  title: "上传中" + res.progress + "%"
                });
              });
            }
            uploadOne();
          }
        });
      }
    });
  },
  GoodList: function(postData) {
    return http.get(addr.GoodList, postData);
  },
  UserStore: function(queryString) {
    return http.get(addr.UserStore, queryString)
  },
  CategoryList: function(queryString) {
    return http.get(addr.CategoryList, queryString);
  },
  EditGoods: function(utoken, postData) {
    return http.post(addr.EditGoods + "?utoken=" + utoken, postData, true);
  },
  GoodInfo: function(queryString) {
    return http.get(addr.GoodInfo, queryString);
  },
  GetGoodsTypesAll: function(queryString) {
    return http.get(addr.GetGoodsTypesAll, queryString);
  },
  GoodType: function(utoken, act, postData) {
    return http.post(addr.GoodType + "?utoken=" + utoken + "&act=" + act, postData, true);
  },
  GoodAttr: function(utoken, act, postData) {
    return http.post(addr.GoodAttr + "?utoken=" + utoken + "&act=" + act, postData, true);
  },
  SaveGoodsInfo: function (utoken, act, postData){
    return http.post(addr.SaveGoodsInfo + "?utoken=" + utoken + "&act=" + act, postData, true);
  },
  GetQrCode:function(queryString){
    return http.get(addr.GetQrCode, queryString);
  },
}
module.exports = {
  http,
  core,
  vm,
  addr
};