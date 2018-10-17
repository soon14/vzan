// 请求url地址
var add = require('address.js')

// 请求成功状态，外界不需要
let SuccessCode = {
    NORMAL: "",
    NO_DATA: "没有更多数据"
}

let ErrorCode = {
    URL_IS_NULL: "URL为空",
    PARAMS_IS_NULL: "参数为空",
    FILE_UPLOAD_FILEPATH_IS_NULL: "上传文件资源路径为空",
    FILE_UPLOAD_NAME_IS_NULL: "文件上传对应的key为空",
    CALLBACK_IS_NULL: "回调为空",
    CALLBACK_SUCCESS_IS_NOT_FUNCTION: "回调的success不是一个function",
    CALLBACK_FAILURE_IS_NOT_FUNCTION: "回调的failure不是一个function",
    REQUEST_FALIURE_400: "加载失败，请求异常",
    REQUEST_FALIURE_500: "加载失败，服务异常",
    REQUEST_FAILURE_UNKNOW: "加载失败，请检查网络"
}

var MessageCode = {
    SuccessCode: SuccessCode,
    ErrorCode: ErrorCode
}


function statusCodeHandle(statusCode) {
    if (statusCode == 200) {
        return MessageCode.SuccessCode
    }
    else if (statusCode >= 500) {
        return MessageCode.ErrorCode.REQUEST_FALIURE_500
    }
    else if (statusCode >= 400 && statusCode < 500) {
        return MessageCode.ErrorCode.REQUEST_FALIURE_400
    }
    else {
        return MessageCode.ErrorCode.REQUEST_FAILURE_UNKNOW
    }
}


function isNull(value) {
    if (value == undefined || value == null || value == '' || value == 'null' || value == '[]' || value == '{}') {
        return true
    }
    return false
}

/** 判断对象是否为函数 */
function isFunction(value) {
    if (typeof (value) == "function") {
        return true;
    } else {
        return false;
    }
}

function networkRequest(url, params, callback, method = "POST") {
    try {
        if (isNull(url)) throw MessageCode.ErrorCode.URL_IS_NULL
        if (isNull(params)) throw MessageCode.ErrorCode.PARAMS_IS_NULL
        if (isNull(callback)) throw MessageCode.ErrorCode.CALLBACK_IS_NULL

        wx.request({
            url: url,
            data: params,
            method: method,
            header: {
                'content-type': 'application/json'
            },
            success: function (res) {
                var status = statusCodeHandle(res.statusCode)
                if (status == MessageCode.SuccessCode) {
                    if (isFunction(callback.success)) {
                        if (isNull(res.data) || isNull(res.data.data)) {
                            callback.success(null, MessageCode.SuccessCode.NO_DATA)
                        }
                        else {
                            callback.success(res.data, MessageCode.SuccessCode.NORMAL)
                        }
                    }
                    else {
                        if (isFunction(callback.failure)) {
                            callback.failure(MessageCode.ErrorCode.CALLBACK_SUCCESS_IS_NOT_FUNCTION)
                        }
                    }
                }
                else {
                    if (isFunction(callback.failure)) {
                        callback.failure(status)
                    }
                }
            },
            fail: function (res) {
                if (isFunction(callback.failure)) {
                    callback.failure(statusCodeHandle(res.statusCode))
                }
            }
        })
    }
    catch (error) {
        if (!isNull(callback)) {
            if (isFunction(callback.failure)) {
                callback.failure(error)
            }
            else {
                console.log(MessageCode.ErrorCode.CALLBACK_FAILURE_IS_NOT_FUNCTION)
            }
        }
        else {
            console.log(error)
        }
    }
}

function UPLOAD(url, filePath, name, formData = {}, callback) {
    try {
        if (isNull(url)) throw MessageCode.ErrorCode.URL_IS_NULL
        if (isNull(filePath)) throw MessageCode.ErrorCode.FILE_UPLOAD_FILEPATH_IS_NULL
        if (isNull(name)) throw MessageCode.ErrorCode.FILE_UPLOAD_NAME_IS_NULL
        if (isNull(callback)) throw MessageCode.ErrorCode.CALLBACK_IS_NULL
        // 异步请求，catch失效
        wx.uploadFile({
            url: url,
            filePath: filePath,
            name: name,
            formData: formData,
            success: function (res) {
                var status = statusCodeHandle(res.statusCode)
                if (status == MessageCode.SuccessCode) {
                    if (isFunction(callback.success)) {
                        if (isNull(res.data)) {
                            callback.success(null, MessageCode.SuccessCode.NO_DATA)
                        }
                        else {
                            callback.success(res.data, MessageCode.SuccessCode.NORMAL)
                        }
                    }
                    else {
                        if (isFunction(callback.failure)) {
                            callback.failure(MessageCode.ErrorCode.CALLBACK_SUCCESS_IS_NOT_FUNCTION)
                        }
                    }
                }
                else {
                    if (isFunction(callback.failure)) {
                        callback.failure(status)
                    }
                }
            },
            fail: function (res) {
                if (isFunction(callback.failure)) {
                    callback.failure(statusCodeHandle(res.statusCode))
                }
            }
        })
    }
    catch (error) {
        if (!isNull(callback)) {
            if (isFunction(callback.failure)) {
                callback.failure(error)
            }
            else {
                console.log(MessageCode.ErrorCode.CALLBACK_FAILURE_IS_NOT_FUNCTION)
            }
        }
        else {
            console.log(error)
        }
    }

}

function GET(url, params, callback) {
    networkRequest(url, params, callback, "GET")
}

function POST(url, params, callback) {
    networkRequest(url, params, callback)
}

module.exports = {
    Address: add.Address,
    MessageCode: MessageCode,
    GET: GET,
    POST: POST,
    UPLOAD: UPLOAD
}