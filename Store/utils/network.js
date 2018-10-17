/**
 * @author liu-feng
 * @date 2016/12/20 0020.
 * Email:w710989327@foxmail.com
 */
import Promise from './es6-promise.min.js';
var util = require("util");
var app;

function Actions() {
  util.log("xxx");
}

Actions.request_none = 0;
Actions.request_firstIn = 1;
Actions.request_refresh = 2;
Actions.request_loadmore = 3;

const code_unlogin = 0x1001;
const code_unfound = 0x1009;

function requestConfig() {
  app = getApp();
  this.page;  //页面对象
  this.urlTail = '';
  // console.log(app);
  this.params = {
    pageIndex: 1,
    pageSize: app.globalData.pageSize,
    session_id: app.globalData.session_id
  };
  this.netMethod = 'POST';
  this.callback = {
    onPre: function () {
    },
    onEnd: function () {

    },
    parseJson: function (data) {
      // 解析Json数据
    },
    onFillData: function (data, params) {
      // 填充数据
    },
    onSuccess: function (data) {
    },
    onEmpty: function () {
    },
    onError: function (msgCanShow, code, hiddenMsg) {
    },
    onUnLogin: function () {
      this.onError("您还没有登录或登录已过期,请登录", code_unlogin, '')
    },
    onUnFound: function () {
      this.onError("您要的内容没有找到", code_unfound, '')
    }
  };

  this.send = function () {
    request(this);
  }
}

/**
 * 刷新和分页加载的请求
 * @param page
 * @param urlTail
 * @param params
 * @param action  本次请求的动作
 * @param currentPageIndex 需要请求的页面index
 * @param currentDatas 页面已经有的数据,是一个jsonarray
 * @param callback 回调  拷贝上方注释区的代码使用
 */
function sendRequestByAction(netMethod, page, urlTail, action, currentPageIndex, params = {}, currentDatas = [], callback = "function") {
  // console.log("callback", callback);
  var pageIndex = currentPageIndex;
  if (action == Actions.request_refresh || action == Actions.request_firstIn) {
    pageIndex = 1;
  } else if (action == Actions.request_loadmore) {
    pageIndex = pageIndex + 1;
  }

  buildRequest(netMethod, page, pageIndex, urlTail, params, {
    onPre: function () {
      if (action == Actions.request_refresh) {
        util.showNavigationBarLoading();
      } else if (action == Actions.request_loadmore) {
        util.showNavigationBarLoading();
      } else if (action == Actions.request_firstIn) {
        util.showLoadingDialog();
      }
    },
    onEnd: function () {
      util.hideNavigationBarLoading();
      util.hideLoadingDialog();
      if (util.isFunction(callback.onEnd))
        callback.onEnd();
    },
    onSuccess: function (data) {
      var infos = callback.parseJson(data);
      if (util.isOptStrNull(infos)) {  // 如果page没有实现parseJson方法，则使用默认json解析
        // try{
        //     infos = JSON.parse(data);
        // }catch(e){
        //     console.log(e);
        // }
      }
      console.log("infos", infos);

      if (infos instanceof Array) {
        var length = infos.length;
        if (length <= 0) {
          this.onEmpty();
          return;
        }
      }

      if (action == Actions.request_refresh) {
        // util.log("数据已刷新");
        // page.setData({
        //     pageObject: infos
        // });
        if (!util.isOptStrNull(infos)) {
          callback.onFillData(infos, params)
        }

      } else if (action == Actions.request_loadmore) {
        var newInfos = currentDatas.concat(infos);


        var hasMore = true;
        if (params.pageSize == undefined || params.pageSize == null || params.pageSize == 0) {
          if (infos.length < app.globalData.pageSize) {
            hasMore = false;
          }
        } else if (infos.length < params.pageSize) {
          hasMore = false;
        }

        page.setData({
          // pageObject: newInfos,
          currentPageIndex: pageIndex
        });

        if (!util.isOptStrNull(infos)) {
          callback.onFillData(newInfos, params)
        }

        if (!hasMore) {
          loadMoreNoData(page, urlTail);
        }

      } else if (action == Actions.request_firstIn) {
        showContent(page, urlTail);
        // page.setData({
        //     pageObject: infos
        // });
        if (!util.isOptStrNull(infos)) {
          callback.onFillData(infos, params)
        }
      }

    },
    onEmpty: function () {
      if (action == Actions.request_refresh) {
        console.log("Empty");
      } else if (action == Actions.request_loadmore) {
        loadMoreNoData(page, urlTail);
      } else if (action == Actions.request_firstIn) {
        showEmptyPage(page, urlTail);
      }
    },
    onError: function (msgCanShow, code, hiddenMsg) {
      util.log("msg:" + msgCanShow + "\ncode:" + code + "\nhiddenMsg:" + hiddenMsg);
      if (action == Actions.request_refresh) {

      } else if (action == Actions.request_loadmore) {
        page.setData({
          currentPageIndex: --pageIndex
        });
        loadMoreError(page, urlTail);
      } else if (action == Actions.request_firstIn) {
        showErrorPage(page, msgCanShow, urlTail);
      }
    },
    onUnLogin: function () {
      this.onError("您还没有登录或登录已过期,请登录", code_unlogin, '')
    },
    onUnFound: function () {
      this.onError("您要的内容没有找到", code_unfound, '')
    }
  }).send();
}

/**
 * 注意,此方法调用后还要调用.send()才是发送出去.
 * @param page
 * @param urlTail
 * @param params
 * @param callback
 * @returns {requestConfig}
 */
function buildRequest(netMethod, page, pageIndex, urlTail, params, callback) {
  var config = new requestConfig();
  config.page = page;
  config.urlTail = urlTail;
  config.params.pageIndex = pageIndex;
  config.netMethod = netMethod
  util.extendObject(config.params, params)
  // config.params.assign(params); //对象合并,目标对象自身也会改变


  if (app.globalData.session_id == null || app.globalData.session_id == '') {
    config.params.session_id = ''
  } else {
    config.params.session_id = app.globalData.session_id;
  }
  if (config.params.pageIndex == undefined || config.params.pageIndex <= 0 || config.params.pageSize == 0) {
    config.params.pageSize = 0
  } else {
    if (config.params.pageSize == undefined) {
      config.params.pageSize = app.globalData.pageSize;
    }
  }

  if (util.isFunction(callback.onPre)) {
    config.callback.onPre = callback.onPre;
  }

  if (util.isFunction(callback.onEnd)) {
    config.callback.onEnd = callback.onEnd;
  }

  if (util.isFunction(callback.onEmpty)) {
    config.callback.onEmpty = callback.onEmpty;
  }

  if (util.isFunction(callback.onSuccess)) {
    config.callback.onSuccess = callback.onSuccess;
  }

  if (util.isFunction(callback.onError)) {
    config.callback.onError = callback.onError;
  }

  if (util.isFunction(callback.onUnLogin)) {
    config.callback.onUnLogin = callback.onUnLogin;
  }
  if (util.isFunction(callback.onUnFound)) {
    config.callback.onUnFound = callback.onUnFound;
  }
  return config;
}

/**
 * @deprecated 已过期,请使用buildRequest().send()来发送请求
 * @param requestConfig
 */
function request(requestConfig) {

  //检验三个公有参数并处理
  if (requestConfig.params.session_id == null || requestConfig.params.session_id == '') {
    delete requestConfig.params.session_id;
  }
  if (requestConfig.params.pageIndex == 0 || requestConfig.params.pageSize == 0) {
    delete requestConfig.params.pageIndex;
    delete requestConfig.params.pageSize;
  }


  var paramStr = util.objToStr(requestConfig.params);//拼接请求参数成一个String
  if (util.isFunction(requestConfig.callback.onPre)) {
    requestConfig.callback.onPre();//请求发出前
  }

  // util.log(requestConfig.params);

  //根据请求方法来拼接url:
  var wholeUrl = requestConfig.urlTail;
  if (wholeUrl.indexOf("http://") < 0 && wholeUrl.indexOf("https://") < 0) {
    wholeUrl = app.globalData.Host + requestConfig.urlTail;
  }

  var contentType = '';
  var body = undefined;
  if (requestConfig.netMethod == 'GET') {
    wholeUrl = wholeUrl + "?" + paramStr;  // 参数拼在URL的后面
    contentType = 'application/json';
  } else if (requestConfig.netMethod == 'POST') {
    body = requestConfig.params; // 参数传Object
    contentType = "application/x-www-form-urlencoded";
  }


  console.log("Params", body);

  wx.request({
    url: wholeUrl,
    method: requestConfig.netMethod,
    header: {
      'Content-Type': contentType
    },
    data: body,
    success: function (res) {
      // util.log(res);
      if (util.isFunction(requestConfig.callback.onEnd)) {
        requestConfig.callback.onEnd();//请求发出前
      }
      if (res.statusCode == 200) {
        var responseData = res.data;

        var code = responseData.result;
        var msg = responseData.msg;

        if (code == 1) {
          var data = responseData;
          var isDataNull = util.isOptStrNull(data);

          if (isDataNull) {
            requestConfig.callback.onEmpty();
          } else {
            requestConfig.callback.onSuccess(data);
          }
        } else if (code == code_unfound) {
          if (util.isOptStrNull(msg)) {
            requestConfig.callback.onUnFound();
          } else {
            requestConfig.callback.onError(msg, code, msg);
          }

        } else if (code == code_unlogin) {
          if (util.isOptStrNull(msg)) {
            requestConfig.callback.onUnLogin();
          } else {
            requestConfig.callback.onError(msg, code, msg);
          }

        } else {
          var isMsgNull = util.isOptStrNull(msg);
          if (isMsgNull) {
            var isCodeNull = util.isOptStrNull(code);
            if (isCodeNull) {
              requestConfig.callback.onError("数据异常解析异常,请核查", code, '');
            } else {
              requestConfig.callback.onError("数据异常,错误码为" + code, code, '');
            }

          } else {
            requestConfig.callback.onError(msg, code, '');
          }
        }
      } else if (res.statusCode >= 500) {
        requestConfig.callback.onError("服务器异常！", res.statusCode, '');
      } else if (res.statusCode >= 400 && res.statusCode < 500) {
        requestConfig.callback.onError("没有找到内容", res.statusCode, '');
      } else {
        requestConfig.callback.onError("网络请求异常！", res.statusCode, '');
      }
    },
    fail: function (res) {
      util.log("fail", res);
      if (util.isFunction(requestConfig.callback.onEnd)) {
        requestConfig.callback.onEnd();//请求发出前
      }
      requestConfig.callback.onError("网络请求异常！", res.statusCode, '');

    },
    complete: function (res) {
      // requestConfig.callback.onEnd();  // success和error已经调用了End


      // that.setData({hidden:true,toast:true});
    }
  })
}

/**
 * 不需要状态Page的请求接口
 *
 * @param url
 * @param params
 * @param method
 * @returns {boolean}
 */
function requestFormNetByDetail(url, params, method = 'GET') {
  util.showLoadingDialog();
  return new Promise((resolve, reject) => {
    wx.request({
      url: url,
      method: method,
      head: {
        'Content-Type': 'application/json'
      },
      data: params,
      success: function (res) {
        util.hideLoadingDialog();
        resolve(res);
      },
      fail: function (res) {
        util.hideLoadingDialog();
        reject(res);
      }
    });
  })
    ;
}

/**
 * 上传文件
 * @param url 上传地址
 * @param filePath 文件路径
 * @param name 文件名
 * @param params   params
 * @returns {boolean}
 */
function uploadFile(url, filePath, params) {
  return new Promise((resolve, reject) => {
    wx.uploadFile({
      url: url,
      filePath: filePath,
      name: "file",
      formData: params,
      header: {
        'Content-Type': "application/x-www-form-urlencoded"
      },
      success: function (res) {
        resolve(res);
      },
      fail: function (res) {
        reject(res);
      }
    });
  })
    ;
}
function uploadFilenew(url, filePath, params) {
  wx.uploadFile({
    url: url,
    filePath: filePath,
    name: "file",
    formData: params,
    header: {
      'Content-Type': "application/x-www-form-urlencoded"
    },
    success: function (res) {
      resolve(res);
    },
    fail: function (res) {
      reject(res);
    }
  });
}

//loadMore的状态与信息
function loadMoreError(that, currentTarget) {
  var bean = that.data.netStateBean;
  bean.loadMoreHidden = false;
  bean.loadMoreMsg = '加载出错,请上拉重试';
  bean.currentTarget = currentTarget;
  that.setData({
    netStateBean: bean
  });

}

function loadMoreNoData(that, currentTarget) {
  var bean = that.data.netStateBean;
  bean.loadMoreHidden = false;
  bean.loadMoreMsg = '没有数据了';
  bean.currentTarget = currentTarget;
  that.setData({
    netStateBean: bean
  });
}

//以下三个方法是用于页面状态管理
function showEmptyPage(that, currentTarget) {
  util.hideLoadingDialog();
  var bean = that.data.netStateBean;
  var empty = that.data.emptyMsg;
  if (util.isOptStrNull(empty)) {
    empty = "没有内容,去别的页面逛逛吧"
  }
  bean.emptyMsg = empty;
  bean.emptyHidden = false;
  bean.loadingHidden = true;
  bean.contentHidden = true;
  bean.errorHidden = true;
  bean.currentTarget = currentTarget;
  that.setData({
    netStateBean: bean
  });
}

function showErrorPage(that, msg, currentTarget) {
  util.hideLoadingDialog();
  var bean = that.data.netStateBean;
  bean.errorHidden = false;
  bean.errorMsg = msg;
  bean.emptyHidden = true;
  bean.loadingHidden = true;
  bean.contentHidden = true;
  bean.currentTarget = currentTarget;
  that.setData({
    netStateBean: bean
  });

}
function showContent(that, currentTarget) {
  util.hideLoadingDialog();
  var bean = that.data.netStateBean;
  bean.errorHidden = true;
  bean.emptyHidden = true;
  bean.contentHidden = false;
  bean.loadingHidden = true;
  bean.currentTarget = currentTarget;
  that.setData({
    netStateBean: bean
  });
}

//请求数据
function requestData(url, params, method, callback) {
  wx.request({
    url: url,
    method: method,
    head: {
      'Content-Type': 'application/json'
    },
    data: params,
    success: function (success) {
      callback(success)
    },
    fail: function (fail) {
      onError("网络请求异常", fail.statusCode, that);
    },
  });
}

function netStateBean() {
  this.toastHidden = true,
    this.toastMsg = '',

    this.loadingHidden = false,
    this.emptyMsg = '暂时没有内容,去别处逛逛吧',
    this.emptyHidden = true,
    this.errorHidden = true,
    this.errorMsg = '',

    this.loadMoreMsg = '加载中...',
    this.loadMoreHidden = true,

    this.contentHidden = true

  this.currentTarget = ''  // 该字段用于单页面存在多个状态 如swiper
}

module.exports = {
  Actions: Actions,
  sendRequestByAction: sendRequestByAction,
  netStateBean: netStateBean,
  showEmptyPage: showEmptyPage,

  uploadFile: uploadFile,
  requestFormNetByDetail: requestFormNetByDetail,
  requestData: requestData,
}