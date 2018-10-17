//String prototype
//去除字符串头尾空格或指定字符  
String.prototype.Trim = function (c) {
    var temp = c;
    if (!c) { temp = " "; }
    return this.trimStart(temp).trimEnd(temp);
}
//去除字符串头部空格或指定字符  
String.prototype.TrimStart = function (c) {
    if (c == null || c == "") {
        var str = this.replace(/^\s*/, '');
        return str;
    }
    else {
        var temp = this;
        while (true) {
            if (temp.substr(0, c.length) != c) {
                break;
            }
            temp = temp.substr(c.length);
        }
        return temp;
    }
}
String.prototype.TrimEnd = function (c) {
    if (c == null || c == "") {
        var str = this;
        var rg = /\s/;
        var i = str.length;
        while (rg.test(str.charAt(--i)));
        return str.slice(0, i + 1);
    }
    else {
        var temp = this;
        while (true) {
            if (temp.substr(temp.length - c.length, c.length) != c) {
                break;
            }
            temp = temp.substr(0, temp.length - c.length);
        }
        return temp;
    }
}  