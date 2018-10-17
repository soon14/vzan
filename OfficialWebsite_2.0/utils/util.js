function formatTime(date) {
    var year = date.getFullYear()
    var month = date.getMonth() + 1
    var day = date.getDate()

    var hour = date.getHours()
    var minute = date.getMinutes()
    var second = date.getSeconds()


    return [year, month, day].map(formatNumber).join('/') + ' ' + [hour, minute, second].map(formatNumber).join(':')
}

function formatNumber(n) {
    n = n.toString()
    return n[1] ? n : '0' + n
}

//加载对话框的显示和隐藏
function showLoadingDialog(title="加载中") {
    wx.showToast({
        title: title,
        mask: true,
        icon: 'loading',
        duration: 10000
    })

}

function hideLoadingDialog() {
    wx.hideToast();
}

function stopPullDownRefresh() {
    wx.stopPullDownRefresh()
}

function showNavigationBarLoading() {
    wx.showNavigationBarLoading();
}

function hideNavigationBarLoading() {
    wx.hideNavigationBarLoading();
}

/** 判断对象是否为空 */
function isOptStrNull(str) {
    if (str == undefined || str == null || str == '' || str == 'null' || str == '[]' || str == '{}') {
        return true
    } else {
        return false;
    }
}

/** 判断对象是否为函数 */
function isFunction(value) {
    if (typeof ( value) == "function") {
        return true;
    } else {
        return false;
    }
}

module.exports = {
    formatTime: formatTime,
    showLoadingDialog: showLoadingDialog,
    hideLoadingDialog: hideLoadingDialog,
    stopPullDownRefresh: stopPullDownRefresh,
    showNavigationBarLoading: showNavigationBarLoading,
    hideNavigationBarLoading: hideNavigationBarLoading,
    isOptStrNull: isOptStrNull,
    isFunction: isFunction,
}
