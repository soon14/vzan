/**
 * @author liu-feng
 * @date 2017/2/28 0028.
 * Email:w710989327@foxmail.com
 */
var util = require('../../utils/util')
function dateParse(arg) {
    // arg = arg.replace(/\/Date\((\d+)\)\//gi)
    if (util.isOptStrNull(arg)) {
        return NaN;
    }
    try {
        var arg = parseInt(arg.replace(/[^0-9]/ig, ""))
        var y = new Date().getFullYear();
        var s = new Date(y + "-01-01").getTime()
        if (s > arg) {
            return new Date(arg).toJSON().slice(0, 10)
        } else {
            return new Date(arg).toJSON().slice(5, 10)
        }
    } catch (e) {
        return NaN
    }
}


module.exports = {
    dateParse: dateParse
}